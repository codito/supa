// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tfs2013ServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the Tfs2013ServiceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;

    /// <summary>
    /// Service provider for TeamFoundationServer 2013.
    /// </summary>
    public class Tfs2013ServiceProvider : ITfsServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tfs2013ServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceUri">
        ///     Endpoint for team foundation server.
        /// </param>
        public Tfs2013ServiceProvider(string serviceUri)
        {
            if (serviceUri == null)
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            throw new System.NotImplementedException();
        }
    }

    public interface ITfsServiceProvider
    {
    }
}