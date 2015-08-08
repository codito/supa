// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MailThread.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a mail conversation thread.
    /// </summary>
    public class MailThread
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailThread"/> class.
        /// </summary>
        public MailThread()
        {
            this.Mails = new List<Mail>();
        }

        /// <summary>
        /// Gets or sets the mail conversation id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the mail conversation topic.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the mails that comprise the thread.
        /// </summary>
        public List<Mail> Mails { get; set; }
    }
}