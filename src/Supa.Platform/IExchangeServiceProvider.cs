// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExchangeServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Provides functionality to work with Exchange Server online.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides functionality to work with Exchange Server online.
    /// </summary>
    public interface IExchangeServiceProvider
    {
        /// <summary>
        /// Gets list of conversations from Exchange Server.
        /// </summary>
        /// <param name="folderName">
        /// Name of the MailFolder to query for conversations.
        /// </param>
        /// <returns>
        /// List of conversations.
        /// </returns>
        IEnumerable<MailThread> GetEmailThreads(string folderName);

        /// <summary>
        /// Adds an email mailFolder under Inbox.
        /// </summary>
        /// <param name="folderName">
        /// The mailFolder name.
        /// </param>
        /// <param name="createIfNotExist">
        /// Creates the mailFolder if not existing.
        /// </param>
        /// <returns>
        /// The <see cref="MailFolder"/>.
        /// </returns>
        MailFolder GetMailFolder(string folderName, bool createIfNotExist);

        /// <summary>
        /// Deletes the email folder.
        /// </summary>
        /// <param name="mailFolder">
        /// The <see cref="MailFolder"/> object for the folder to be deleted.
        /// </param>
        void DeleteMailFolder(MailFolder mailFolder);
    }
}