// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsServiceProviderSimulator.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform.TestDoubles
{
    using System;

    using Microsoft.TeamFoundation;

    /// <summary>
    /// The <c>tfs</c> service provider simulator.
    /// </summary>
    public class TfsServiceProviderSimulator : ITfsServiceProvider
    {
        private Uri serviceUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="TfsServiceProviderSimulator"/> class.
        /// </summary>
        /// <param name="serviceUri">
        /// The service uri.
        /// </param>
        public TfsServiceProviderSimulator(Uri serviceUri)
        {
            this.serviceUri = serviceUri;
        }

        /// <inheritdoc/>
        public void Configure(TfsServiceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration.Username.Equals("invalidUsername") || configuration.Password.Equals("invalidPassword"))
            {
                throw new TeamFoundationServerUnauthorizedException();
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