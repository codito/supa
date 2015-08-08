// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MailFolder.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System.Collections.Generic;

    using Microsoft.Exchange.WebServices.Data;

    /// <summary>
    /// Represents a folder in exchange mailbox.
    /// </summary>
    public class MailFolder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailFolder"/> class.
        /// </summary>
        public MailFolder()
        {
            this.EmailThreads = new List<MailThread>();
        }

        /// <summary>
        /// Gets or sets the folder id. This is <see cref="FolderId"/> object for Exchange.
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the email threads in the mail folder.
        /// </summary>
        public List<MailThread> EmailThreads { get; private set; }
    }
}