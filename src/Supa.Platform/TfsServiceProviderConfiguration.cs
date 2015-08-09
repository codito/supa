// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TfsServiceProviderConfiguration.cs" company="">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Supa.Platform
{
    using System.Collections.Generic;

    /// <summary>
    /// Configuration properties for <see cref="ITfsServiceProvider"/>.
    /// </summary>
    public class TfsServiceProviderConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TfsServiceProviderConfiguration"/> class.
        /// </summary>
        /// <param name="username">
        /// Username for the connection.
        /// </param>
        /// <param name="password">
        /// Password for the connection.
        /// </param>
        public TfsServiceProviderConfiguration(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the parent work item id.
        /// </summary>
        public int ParentWorkItemId { get; set; }

        public Dictionary<string, string> IssueToWorkItemFieldMap { get; } 
    }
}