// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsWorkItemExtensions.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the TfsWorkItemExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Extension methods for <see cref="TfsWorkItem"/>.
    /// </summary>
    public static class TfsWorkItemExtensions
    {
        /// <summary>
        /// Updates a field in the underlying <see cref="WorkItem"/> for the <see cref="TfsWorkItem"/>.
        /// </summary>
        /// <param name="tfsWorkItem">The <see cref="TfsWorkItem"/> instance.</param>
        /// <param name="field">Name of the field.</param>
        /// <param name="value">Value for the field.</param>
        public static void UpdateField(this TfsWorkItem tfsWorkItem, string field, string value)
        {
            if (tfsWorkItem == null)
            {
                throw new ArgumentNullException(nameof(tfsWorkItem));
            }

            dynamic workItem = tfsWorkItem.Item;
            workItem[field] = value;
        }
    }
}