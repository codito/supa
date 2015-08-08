// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mail.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the Mail type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;

    /// <summary>
    /// Represents a single email message.
    /// </summary>
    public class Mail
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the from.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        public string Recipients { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the date time received.
        /// </summary>
        public DateTime DateTimeReceived { get; set; }
    }
}