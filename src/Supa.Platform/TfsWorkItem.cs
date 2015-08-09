// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsWorkItem.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the TfsWorkItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Encapsulates a <c>Tfs</c> work item.
    /// </summary>
    public class TfsWorkItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TfsWorkItem"/> class.
        /// </summary>
        /// <param name="workItem">Actual <c>Tfs</c> work item.</param>
        public TfsWorkItem(object workItem) : this(workItem, typeof(WorkItem))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsWorkItem"/> class.
        /// </summary>
        /// <param name="workItem">Actual <c>Tfs</c> work item.</param>
        /// <param name="baseWorkItemType">Type of underlying work item.</param>
        /// <remarks>Used for testability.</remarks>
        protected TfsWorkItem(object workItem, Type baseWorkItemType)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (workItem.GetType() != baseWorkItemType)
            {
                throw new ArgumentException("Argument must be of WorkItem type.", nameof(workItem));
            }

            this.Item = workItem;
            this.WorkItemBaseType = baseWorkItemType;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="WorkItem"/> has change and should be saved.
        /// </summary>
        public bool HasChange { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="WorkItem"/> instance.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Gets or sets the issue id for the mail.
        /// </summary>
        public string IssueId { get; set; }

        /// <summary>
        /// Gets the base type for underlying work item object.
        /// </summary>
        /// <remarks>Used for testability.</remarks>
        public Type WorkItemBaseType { get; }
    }
}