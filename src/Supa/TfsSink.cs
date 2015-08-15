// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsSink.cs" company="">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using System;
    using System.Globalization;
    using System.IO;

    using Serilog;

    using Supa.Platform;

    /// <summary>
    /// The <c>tfs</c> sink.
    /// </summary>
    public class TfsSink
    {
        private readonly ILogger logger;

        private readonly string workItemTemplatePath;

        private readonly ITfsServiceProvider tfsServiceProvider;

        private readonly TfsServiceProviderConfiguration tfsServiceProviderConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSink"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// Service end point.
        /// </param>
        /// <param name="parentWorkItem">
        /// Parent work item id for all tasks.
        /// </param>
        /// <param name="issueTemplatePath">
        /// Path to a work item template.
        /// </param>
        public TfsSink(Uri serviceUri, int parentWorkItem, string issueTemplatePath)
            : this(new TfsSoapServiceProvider(serviceUri), parentWorkItem, issueTemplatePath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsSink"/> class.
        /// </summary>
        /// <param name="tfsServiceProvider">Service endpoint.</param>
        /// <param name="parentWorkItem">The parent work item.</param>
        /// <param name="issueTemplatePath">The issue template path.</param>
        protected TfsSink(ITfsServiceProvider tfsServiceProvider, int parentWorkItem, string issueTemplatePath)
        {
            this.logger = Log.Logger.ForContext<TfsSink>();

            this.tfsServiceProvider = tfsServiceProvider;
            this.workItemTemplatePath = issueTemplatePath;
            this.tfsServiceProviderConfiguration = new TfsServiceProviderConfiguration(null, null)
            {
                ParentWorkItemId = parentWorkItem
            };
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        public void Serialize(Issue issue)
        {
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderConfiguration);
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(issue.Id, issue.Activity);

            // Don't process if the issue is same as existing item
            if (tfsWorkItem.HasChange == false)
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

                tfsWorkItem.UpdateField(keyval[0].Trim(), value);
            }

            this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);
        }
    }
}