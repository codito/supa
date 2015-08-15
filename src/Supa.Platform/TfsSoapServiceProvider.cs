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

            var networkCredential = new NetworkCredential(configuration.Username, configuration.Password);
            var tfsClientCredentials = new TfsClientCredentials(new BasicAuthCredential(networkCredential)) { AllowInteractive = false };
            var tfsProjectCollection = new TfsTeamProjectCollection(this.serviceUri, tfsClientCredentials);
            tfsProjectCollection.Authenticate();
            tfsProjectCollection.EnsureAuthenticated();

            await Task.Run(
                () =>
                    {
                        this.workItemStore = new WorkItemStore(tfsProjectCollection);
                        this.parentWorkItem = this.workItemStore.GetWorkItem(configuration.ParentWorkItemId);
                    });
        }

        /// <inheritdoc/>
        public TfsWorkItem GetWorkItemForIssue(string issueId, int issueActivityCount)
        {
            if (string.IsNullOrEmpty(issueId))
            {
                throw new ArgumentNullException(nameof(issueId));
            }

            if (this.parentWorkItem == null)
            {
                throw new Exception("Provider is not initialized. Have you called ConfigureAsync?");
            }

            WorkItem item = null;
            var hasChange = false;
            foreach (var link in this.parentWorkItem.Links)
            {
                var itemLink = link as RelatedLink;
                if (itemLink != null && itemLink.Comment.StartsWith(issueId))
                {
                    var commentParts = itemLink.Comment.Split(':');
                    var itemLinkActivityCount = commentParts[1];

                    item = this.workItemStore.GetWorkItem(itemLink.RelatedWorkItemId);
                    if (!itemLinkActivityCount.Equals(issueActivityCount.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                    {
                        hasChange = true;
                    }

                    this.logger.Debug("Found existing workitem: {Id}, {activityCount}", item.Id, itemLinkActivityCount);
                    this.logger.Debug("Activity count for issue: {Activity}", issueActivityCount);
                    break;
                }
            }

            if (item == null)
            {
                item = this.parentWorkItem.Project.WorkItemTypes["Task"].NewWorkItem();
                hasChange = true;
            }

            return new TfsWorkItem(item) { HasChange = hasChange, IssueId = issueId };
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
                var link = new WorkItemLink(linkType.ReverseEnd, this.parentWorkItem.Id) { Comment = tfsWorkItem.IssueId };
                item.Links.Add(link);
            }

            item.Validate();
            item.Save();
        }
    }
}