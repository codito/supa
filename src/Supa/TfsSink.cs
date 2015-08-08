// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsSink.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Supa
{
    using System;
    using System.Globalization;
    using System.IO;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    using Serilog;

    /// <summary>
    /// The <c>tfs</c> sink.
    /// </summary>
    public class TfsSink
    {
        private readonly WorkItemStore workItemStore;

        private readonly WorkItem parentWorkItem;

        private readonly ILogger logger;

        private readonly string workItemTemplatePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSink"/> class.
        /// </summary>
        /// <param name="serviceUri">Service end point.</param>
        /// <param name="parentWorkItem">Parent work item id for all tasks.</param>
        /// <param name="issueTemplatePath">Path to a work item template.</param>
        public TfsSink(Uri serviceUri, int parentWorkItem, string issueTemplatePath)
        {
            var tfsUrl = serviceUri;

            this.logger = Log.Logger.ForContext<TfsSink>();

            var tfsProjectCollection = new TfsTeamProjectCollection(tfsUrl);
            this.workItemStore = new WorkItemStore(tfsProjectCollection);
            this.parentWorkItem = this.workItemStore.GetWorkItem(parentWorkItem);
            this.workItemTemplatePath = issueTemplatePath;
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        public void Serialize(Issue issue)
        {
            WorkItem item = null;
            var shouldSave = false;
            foreach (var link in this.parentWorkItem.Links)
            {
                var itemLink = link as RelatedLink;
                if (itemLink != null && itemLink.Comment.StartsWith(issue.Id))
                {
                    item = this.workItemStore.GetWorkItem(itemLink.RelatedWorkItemId);
                    var activityCount = item["Custom 05"].ToString();
                    if (!activityCount.Equals(issue.Activity.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSave = true;
                    }

                    this.logger.Debug("Found existing workitem: {Id}, {activityCount}", item.Id, activityCount);
                    this.logger.Debug("Activity count for issue: {Activity}", issue.Activity);
                    break;
                }
            }

            if (item == null)
            {
                item = this.parentWorkItem.Project.WorkItemTypes["Task"].NewWorkItem();
                shouldSave = true;
            }

            // Don't process if the issue is same as existing item
            if (shouldSave == false)
            {
                this.logger.Information("Skip creating tfs workitem.");
                return;
            }

            var template = File.ReadAllText(this.workItemTemplatePath);

            foreach (var line in template.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                var keyval = line.Split(new[] { '=' }, 2);
                if (string.IsNullOrEmpty(keyval[0]))
                {
                    continue;
                }

                var value = keyval[1].Trim();
                switch (value)
                {
                    case "{{Id}}":
                        value = issue.Id;
                        break;
                    case "{{Topic}}":
                        value = issue.Topic;
                        break;
                    case "{{Description}}":
                        value = issue.Description.Replace(Environment.NewLine, "<br/>");
                        break;
                    case "{{SourceName}}":
                        value = "vstestsup";
                        break;
                    case "{{Activity}}":
                        value = issue.Activity.ToString(CultureInfo.InvariantCulture);
                        break;
                }

                item[keyval[0].Trim()] = value;
            }

            if (item.IsNew)
            {
                var linkType = this.workItemStore.WorkItemLinkTypes["System.LinkTypes.Hierarchy"];
                var link = new WorkItemLink(linkType.ReverseEnd, this.parentWorkItem.Id) { Comment = issue.Id };
                item.Links.Add(link);
            }

            item.Validate();
            item.Save();
        }
    }
}