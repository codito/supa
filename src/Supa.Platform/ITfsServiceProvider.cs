// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITfsServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the ITfsServiceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Abstraction for <c>Tfs</c> services.
    /// </summary>
    public interface ITfsServiceProvider
    {
        /// <summary>
        /// Configures and authenticates the <c>Tfs</c> connection.
        /// </summary>
        /// <param name="configuration">An instance of configuration</param>
        /// <returns>A Task</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="configuration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if invalid credentials are presented.</exception>
        Task ConfigureAsync(TfsServiceProviderConfiguration configuration);

        /// <summary>
        /// Gets a work item for an issue id. Creates a new work item if none matching exists.
        /// </summary>
        /// <param name="issueId">Unique identifier for an <c>supa</c> issue.</param>
        /// <param name="issueActivityCount">Count of activity in the issue discussion.</param>
        /// <returns>
        /// The <see cref="TfsWorkItem"/>.
        /// </returns>
        TfsWorkItem GetWorkItemForIssue(string issueId, int issueActivityCount);

        /// <summary>
        /// Validate and save the work item.
        /// </summary>
        /// <param name="tfsWorkItem">Instance of <see cref="TfsWorkItem"/></param>
        void SaveWorkItem(TfsWorkItem tfsWorkItem);
    }
}