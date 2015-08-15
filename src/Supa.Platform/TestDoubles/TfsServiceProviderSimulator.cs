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
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.VisualStudio.Services.Common;

    /// <summary>
    /// The <c>tfs</c> service provider simulator.
    /// </summary>
    public class TfsServiceProviderSimulator : ITfsServiceProvider
    {

        private readonly Dictionary<int, string> workItems;
        private readonly Dictionary<int, List<int>> workItemLinks;
        private Uri serviceUri;
        private int parentWorkItemId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsServiceProviderSimulator"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// The service uri.
        /// </param>
        public TfsServiceProviderSimulator(Uri serviceUri)
        {
            this.serviceUri = serviceUri;
            this.workItems = new Dictionary<int, string>();
            this.workItemLinks = new Dictionary<int, List<int>>();
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
                        if (this.workItems.ContainsKey(configuration.ParentWorkItemId))
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
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void SaveWorkItem(TfsWorkItem tfsWorkItem)
        {
            throw new System.NotImplementedException();
        }

        #region Testability Methods

        public void CreateWorkItem(int id, string title)
        {
            this.workItems.Add(id, title);
            this.workItemLinks.Add(id, new List<int>());
        }

        public void AddLinkToWorkItem(int parentId, int childId)
        {
            this.workItemLinks[parentId].Add(childId);
        }
        #endregion
    }
}