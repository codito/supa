// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsRestServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.Common;

    /// <summary>
    /// Service provider for TeamFoundationServer on-premise (REST based).
    /// </summary>
    public class TfsRestServiceProvider : ITfsServiceProvider
    {
        private readonly Uri serviceUri;

        private WorkItem parentWorkItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsRestServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceUri">Endpoint for team foundation server.</param>
        public TfsRestServiceProvider(Uri serviceUri)
        {
            if (serviceUri == null)
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            this.serviceUri = serviceUri;
        }

        /// <inheritdoc/>
        public async Task ConfigureAsync(TfsServiceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var vssCredentials = new VssBasicCredential(configuration.Username, configuration.Password);
            var witClient = new WorkItemTrackingHttpClient(this.serviceUri, vssCredentials);

            this.parentWorkItem = await witClient.GetWorkItemAsync(configuration.ParentWorkItemId);
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
    }
}