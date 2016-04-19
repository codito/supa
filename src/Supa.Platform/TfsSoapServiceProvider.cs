// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsSoapServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the TfsSoapServiceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    using Serilog;

    /// <summary>
    /// Service provider for TeamFoundationServer on-premise (SOAP based).
    /// </summary>
    public class TfsSoapServiceProvider : ITfsServiceProvider
    {
        private readonly Uri serviceUri;

        private readonly ILogger logger;

        private WorkItemStore workItemStore;

        private WorkItem parentWorkItem;

        private string workItemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSoapServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceUri">Endpoint for team foundation server.</param>
        public TfsSoapServiceProvider(Uri serviceUri)
        {
            if (serviceUri == null)
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            this.logger = Log.Logger.ForContext<TfsSoapServiceProvider>();
            this.serviceUri = serviceUri;
        }

        /// <inheritdoc/>
        public async Task ConfigureAsync(TfsServiceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if(string.IsNullOrEmpty(configuration.WorkItemType))
            {
                throw new ArgumentNullException(nameof(configuration.WorkItemType));
            }

            this.logger.Debug("Configure of TfsSoapServiceProvider started...");
            var networkCredential = new NetworkCredential(configuration.Username, configuration.Password);
            TfsClientCredentials tfsClientCredentials = new TfsClientCredentials(new BasicAuthCredential(networkCredential)) { AllowInteractive = false };

            var tfsProjectCollection = new TfsTeamProjectCollection(this.serviceUri, tfsClientCredentials);
            this.workItemType = configuration.WorkItemType;

            try
            {
                tfsProjectCollection.Authenticate();
            }
            catch
            {
                tfsClientCredentials = new TfsClientCredentials(); // fall back to login page
                tfsProjectCollection = new TfsTeamProjectCollection(this.serviceUri, tfsClientCredentials);
                tfsProjectCollection.Authenticate();
            }

            tfsProjectCollection.EnsureAuthenticated();
            this.logger.Debug("Authentication successful for {serviceUri}.", this.serviceUri.AbsoluteUri);

            await Task.Run(
                () =>
                    {
                        this.logger.Debug("Fetching workitem for id {parentWorkItemId}.", configuration.ParentWorkItemId);
                        this.workItemStore = new WorkItemStore(tfsProjectCollection);
                        this.parentWorkItem = this.workItemStore.GetWorkItem(configuration.ParentWorkItemId);
                        this.logger.Debug("Found parent work item '{title}'.", this.parentWorkItem.Title);
                    });
            this.logger.Verbose("Tfs service provider configuration complete.");
        }

        /// <inheritdoc/>
        public TfsWorkItem GetWorkItemForIssue(string issueId, int issueActivityCount)
        {
            var isNew = false;
            this.logger.Debug("Get tfs work item for {issueId}, {issueActivityCount}.", issueId, issueActivityCount);
            if (string.IsNullOrEmpty(issueId))
            {
                throw new ArgumentNullException(nameof(issueId));
            }

            if (this.parentWorkItem == null)
            {
                throw new Exception("Provider is not initialized. Have you called ConfigureAsync?");
            }

            this.parentWorkItem = this.workItemStore.GetWorkItem(this.parentWorkItem.Id);
            WorkItem item = null;
            var hasChange = false;
            this.logger.Debug("Loop through all {count} links of parent work item.", this.parentWorkItem.Links.Count);
            foreach (var link in this.parentWorkItem.Links)
            {
                var itemLink = link as RelatedLink;
                if (itemLink != null && itemLink.Comment.StartsWith(issueId))
                {
                    var commentParts = itemLink.Comment.Split(':');
                    var itemLinkActivityCount = commentParts[1];

                    item = this.workItemStore.GetWorkItem(itemLink.RelatedWorkItemId);
                    this.logger.Debug("Found existing workitem: {Id}, {activityCount}", item.Id, itemLinkActivityCount);
                    if (!itemLinkActivityCount.Equals(issueActivityCount.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                    {
                        this.logger.Debug("Activity count has changed for the thread. Updated count: {Activity}.", issueActivityCount);
                        hasChange = true;
                    }

                    break;
                }
            }

            if (item == null)
            {
                this.logger.Debug("Need a new tfs work item.");
                item = this.parentWorkItem.Project.WorkItemTypes[this.workItemType].NewWorkItem();
                hasChange = true;
                isNew = true;
            }

            var issueSignature = $"{issueId}:{issueActivityCount}";
            this.logger.Verbose("We've a tfs work item for email thread: {title}.", item.Title);
            return new TfsWorkItem(item) { HasChange = hasChange, IssueSignature = issueSignature, IsNew = isNew };
        }

        /// <inheritdoc/>
        public void SaveWorkItem(TfsWorkItem tfsWorkItem)
        {
            var item = tfsWorkItem.Item as WorkItem;
            if (item == null)
            {
                throw new InvalidOperationException("Invalid TfsWorkItem object.");
            }

            if (item.IsNew)
            {
                var linkType = this.workItemStore.WorkItemLinkTypes["System.LinkTypes.Hierarchy"];
                var link = new WorkItemLink(linkType.ReverseEnd, this.parentWorkItem.Id) { Comment = tfsWorkItem.IssueSignature };
                item.Links.Add(link);
            }
            else
            {
                foreach (var link in item.Links)
                {
                    var linkToParent = link as RelatedLink;
                    if (linkToParent != null && linkToParent.RelatedWorkItemId == this.parentWorkItem.Id)
                    {
                        linkToParent.Comment = tfsWorkItem.IssueSignature;
                    }
                }
            }
            this.logger.Debug("Added a link to parent work item with {comment}", tfsWorkItem.IssueSignature);

            foreach (Field field in item.Validate())
            {
                this.logger.Warning($"Work item field has invalid value: {field.Name}: {field.Status}");
            }

            item.Save();
            this.logger.Verbose("Saved the tfs work item. All is well!");
        }
    }
}