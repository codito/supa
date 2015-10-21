// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsSink.cs" company="">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using Serilog;

    using Supa.Platform;

    /// <summary>
    /// The <c>tfs</c> sink.
    /// </summary>
    public class TfsSink
    {
        private readonly ILogger logger;

        private readonly ITfsServiceProvider tfsServiceProvider;

        private readonly IDictionary<string, object> fieldMap;

        private readonly TfsServiceProviderConfiguration tfsServiceProviderConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSink"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// Service end point.
        /// </param>
        /// <param name="credential">
        /// The credential.
        /// </param>
        /// <param name="parentWorkItem">
        /// Parent work item id for all tasks.
        /// </param>
        /// <param name="fieldMap">Map of work item fields to mail properties.</param>
        public TfsSink(Uri serviceUri, NetworkCredential credential, int parentWorkItem, string workItemType, IDictionary<string, object> fieldMap)
            : this(new TfsSoapServiceProvider(serviceUri), credential, parentWorkItem, workItemType, fieldMap)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSink"/> class.
        /// </summary>
        /// <param name="tfsServiceProvider">
        /// Service endpoint.
        /// </param>
        /// <param name="credential">
        /// The credential.
        /// </param>
        /// <param name="parentWorkItem">
        /// Parent work item id for all tasks.
        /// </param>
        /// <param name="fieldMap">Map of work item fields to mail properties.</param>
        protected TfsSink(ITfsServiceProvider tfsServiceProvider, NetworkCredential credential, int parentWorkItem, string workItemType, IDictionary<string, object> fieldMap)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            if (fieldMap == null)
            {
                throw new ArgumentNullException(nameof(fieldMap));
            }

            this.logger = Log.Logger.ForContext<TfsSink>();

            this.tfsServiceProvider = tfsServiceProvider;
            this.fieldMap = fieldMap;
            this.tfsServiceProviderConfiguration = new TfsServiceProviderConfiguration(credential.UserName, credential.Password)
            {
                ParentWorkItemId = parentWorkItem,
                WorkItemType = string.IsNullOrEmpty(workItemType) ? "Task" : workItemType
            };
        }

        /// <summary>
        /// Configures the service provider connection and validates the user settings.
        /// </summary>
        public void Configure()
        {
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderConfiguration).Wait();
        }

        /// <summary>
        /// Create or update a <c>Tfs</c> work item for a given <see cref="Issue"/>.
        /// </summary>
        /// <param name="issue">
        /// An issue instance.
        /// </param>
        /// <returns>
        /// True if the <c>Tfs</c> update is done, false is no update is required.
        /// </returns>
        public bool UpdateWorkItem(Issue issue)
        {
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(issue.Id, issue.Activity);

            // Don't process if the issue is same as existing item
            if (tfsWorkItem.HasChange == false)
            {
                this.logger.Information("Skip creating tfs workitem.");
                return false;
            }

            foreach (var keyval in this.fieldMap)
            {
                // Based on work item template, few fields may be mandatory. If user configures
                // field map with default values for these fields, update them.
                var issueField = keyval.Value.ToString();
                switch (keyval.Value.ToString())
                {
                    case "{{Id}}":
                        issueField = issue.Id;
                        break;
                    case "{{Topic}}":
                        issueField = issue.Topic;
                        break;
                    case "{{Description}}":
                        issueField = issue.Description.Replace(Environment.NewLine, "<br/>");
                        break;
                    case "{{Activity}}":
                        issueField = issue.Activity.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        if (!tfsWorkItem.IsNew)
                        {
                            continue;
                        }
                        break;
                }

                tfsWorkItem.UpdateField(keyval.Key, issueField);
            }

            this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);
            this.logger.Information("Updated tfs work item.");
            return true;
        }
    }
}