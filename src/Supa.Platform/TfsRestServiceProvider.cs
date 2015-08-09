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

    using Microsoft.TeamFoundation.Client;

    /// <summary>
    /// Service provider for TeamFoundationServer on-premise (REST based).
    /// </summary>
    public class TfsRestServiceProvider : ITfsServiceProvider
    {
        private readonly Uri serviceUri;

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
        public void Configure(TfsServiceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var networkCredential = new NetworkCredential(configuration.Username, configuration.Password);
            var tfsClientCredentials = new TfsClientCredentials(new BasicAuthCredential(networkCredential)) { AllowInteractive = false };
            using (var tpc = new TfsTeamProjectCollection(this.serviceUri, tfsClientCredentials))
            {
                tpc.Authenticate();
                Console.WriteLine(tpc.InstanceId);
            }
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