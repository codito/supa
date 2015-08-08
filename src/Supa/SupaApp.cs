// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupaApp.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using JsonConfig;

    /// <summary>
    /// <c>SupaApp</c> is the program entry point.
    /// </summary>
    public class SupaApp
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public object Configuration { get; private set; }

        /// <summary>
        /// Reads application configuration from given file path to <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="filePath">
        /// Configuration file path.
        /// </param>
        /// <returns>
        /// The <see cref="SupaApp"/>.
        /// </returns>
        public SupaApp ReadConfiguration(string filePath)
        {
            this.Configuration = filePath != null ? Config.ApplyJsonFromPath(filePath) : Config.User;

            return this;
        }
    }
}