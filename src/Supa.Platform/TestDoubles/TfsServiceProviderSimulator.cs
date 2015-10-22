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
    using System.Globalization;
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
            bool isNew = false;
            if (string.IsNullOrEmpty(issueId))
            {
                throw new ArgumentNullException(nameof(issueId));
            }

            if (this.parentWorkItemId == -1)
            {
                throw new Exception("Please run ConfigureAsync first.");
            }

            var hasChange = false;
            var parentWorkItem = this.workItems.Single(w => w.Id == this.parentWorkItemId);
            string[] commentParts = null;
            var workItemLink = parentWorkItem.Links.FirstOrDefault(l =>
                {
                    if (l.Comment.StartsWith(issueId))
                    {
                        commentParts = l.Comment.Split(':');
                        return true;
                    }

                    return false;
                });
            InMemoryWorkItem workItem = null;
            if (workItemLink != null)
            {
                if (!commentParts[1].Equals(issueActivityCount.ToString(CultureInfo.InvariantCulture)))
                {
                    hasChange = true;
                }

                workItem = this.workItems.Single(w => w.Id == workItemLink.RelatedWorkItemId);
            }
            else
            {
                workItem = new InMemoryWorkItem();
                hasChange = true;
                isNew = true;
            }

            var issueSignature = $"{issueId}:{issueActivityCount}";
            var imtw= new InMemoryTfsWorkItem(workItem) { HasChange = hasChange, IssueSignature = issueSignature };
            imtw.IsNew = isNew;
            return imtw;
        }

        /// <inheritdoc/>
        public void SaveWorkItem(TfsWorkItem tfsWorkItem)
        {
            var workItem = tfsWorkItem.Item as InMemoryWorkItem;
            if (workItem == null)
            {
                throw new InvalidOperationException("Invalid TfsWorkItem type.");
            }

            if (this.workItems.Contains(workItem))
            {
                var parentWorkItem = this.workItems.Single(w => w.Id == this.parentWorkItemId);
                var workItemLink = parentWorkItem.Links.Single(l => l.RelatedWorkItemId == workItem.Id);

                workItemLink.Comment = tfsWorkItem.IssueSignature;
            }
            else
            {
                workItem.Validate();
                this.workItems.Add(workItem);
                this.AddLinkToWorkItem(this.parentWorkItemId, workItem.Id, tfsWorkItem.IssueSignature);
            }
        }

        #region Testability Methods

        /// <summary>
        /// Creates a work item in memory.
        /// </summary>
        /// <param name="title">Work item title.</param>
        public int CreateWorkItem(string title)
        {
            var workItem = new InMemoryWorkItem { Title = title };
            this.workItems.Add(workItem);

            return workItem.Id;
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

        public class InMemoryWorkItem
        {
            private static int workItemSeed = 1020;
            private readonly Dictionary<string, string> fieldsDictionary;

            public InMemoryWorkItem()
            {
                this.Id = workItemSeed++;
                this.Links = new List<InMemoryWorkItemLink>();
                this.fieldsDictionary = new Dictionary<string, string>();
            }

            public int Id { get; private set; }

            public string Title { get; set; }

            public List<InMemoryWorkItemLink> Links { get; set; }

            public string this[string fieldName]
            {
                get
                {
                    return this.fieldsDictionary[fieldName];
                }

                set
                {
                    this.fieldsDictionary[fieldName] = value;
                }
            }

            public void Validate()
            {
                try
                {
                    if (string.IsNullOrEmpty(this["Title"]))
                    {
                        throw new Exception("Invalid title");
                    }
                }
                catch (Exception ex)
                {
                    throw new ValidationException("Workitem is not ready to save.", ex);
                }
            }
        }

        public class InMemoryWorkItemLink
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