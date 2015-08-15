// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsServiceProviderSimulator.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// The <c>tfs</c> service provider simulator.
    /// </summary>
    public class TfsServiceProviderSimulator : ITfsServiceProvider
    {
        private readonly List<InMemoryWorkItem> workItems;
        private int parentWorkItemId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsServiceProviderSimulator"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// The service uri.
        /// </param>
        public TfsServiceProviderSimulator(Uri serviceUri)
        {
            if (serviceUri == null)
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            this.workItems = new List<InMemoryWorkItem>();
            this.parentWorkItemId = -1;
        }

        /// <inheritdoc/>
        public async Task ConfigureAsync(TfsServiceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration.Username.Equals("invalidUsername") || configuration.Password.Equals("invalidPassword"))
            {
                throw new TeamFoundationServerUnauthorizedException();
            }

            await Task.Run(
                () =>
                    {
                        if (this.workItems.Any(w => w.Id == configuration.ParentWorkItemId))
                        {
                            this.parentWorkItemId = configuration.ParentWorkItemId;
                        }
                        else
                        {
                            throw new DeniedOrNotExistException();
                        }
                    });
        }

        /// <inheritdoc/>
        public TfsWorkItem GetWorkItemForIssue(string issueId, int issueActivityCount)
        {
            if (string.IsNullOrEmpty(issueId))
            {
                throw new ArgumentNullException(nameof(issueId));
            }

            if (this.parentWorkItemId == -1)
            {
                throw new Exception("Please run ConfigureAsync first.");
            }

            var parentWorkItem = this.workItems.Single(w => w.Id == this.parentWorkItemId);
            var workItemLink = parentWorkItem.Links.FirstOrDefault(l => l.Comment.StartsWith(issueId));
            InMemoryWorkItem workItem = null;
            if (workItemLink != null)
            {
                workItem = this.workItems.Single(w => w.Id == workItemLink.RelatedWorkItemId);
            }

            return new InMemoryTfsWorkItem(workItem) { HasChange = false, IssueId = issueId };
        }

        /// <inheritdoc/>
        public void SaveWorkItem(TfsWorkItem tfsWorkItem)
        {
            throw new NotImplementedException();
        }

        #region Testability Methods

        /// <summary>
        /// Creates a work item in memory.
        /// </summary>
        /// <param name="id">Work item id.</param>
        /// <param name="title">Work item title.</param>
        public void CreateWorkItem(int id, string title)
        {
            this.workItems.Add(new InMemoryWorkItem { Id = id, Title = title });
        }

        /// <summary>
        /// Creates a link between parent and child work item with a comment.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="childId">The child id.</param>
        /// <param name="comment">A comment.</param>
        public void AddLinkToWorkItem(int parentId, int childId, string comment)
        {
            var workItemLink = new InMemoryWorkItemLink
                                   {
                                       RelatedWorkItemId = childId,
                                       Comment = comment
                                   };
            this.workItems.Single(w => w.Id == parentId).Links.Add(workItemLink);
        }

        private class InMemoryWorkItem
        {
            public InMemoryWorkItem()
            {
                this.Links = new List<InMemoryWorkItemLink>();
            }

            public int Id { get; set; }

            public string Title { get; set; }

            public List<InMemoryWorkItemLink> Links { get; set; } 
        }

        private class InMemoryWorkItemLink
        {
            public int RelatedWorkItemId { get; set; }

            public string Comment { get; set; }
        }

        private class InMemoryTfsWorkItem : TfsWorkItem
        {
            public InMemoryTfsWorkItem(InMemoryWorkItem item)
                : base(item, typeof(InMemoryWorkItem))
            {
            }
        }
        #endregion
    }
}