// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExchangeServiceProviderSimulator.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the ExchangeServiceProviderSimulator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform.TestDoubles
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Simulates an <see cref="ExchangeServiceProvider"/> with an in-memory representation.
    /// </summary>
    public class ExchangeServiceProviderSimulator : IExchangeServiceProvider
    {
        private readonly List<MailFolder> folders;

        /// <inheritdoc/>
        public ExchangeServiceProviderSimulator(Uri serviceUrl, NetworkCredential credential)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            if (credential == null)
            {
                throw new ArgumentNullException("credential");
            }

            this.folders = new List<MailFolder>();
        }

        #region IExchangeProvider implementation

        /// <inheritdoc/>
        public IEnumerable<MailThread> GetEmailThreads(string folderName)
        {
            var folder = this.GetMailFolder(folderName, false);
            if (folder == null)
            {
                throw new InvalidOperationException();
            }

            return folder.EmailThreads;
        }

        /// <inheritdoc/>
        public MailFolder GetMailFolder(string folderName, bool createIfNotExist)
        {
            if (folderName == null)
            {
                throw new ArgumentNullException("folderName");
            }

            var folder = this.folders.FirstOrDefault(f => f.Name.Equals(folderName));
            if (folder == null && createIfNotExist)
            {
                folder = new MailFolder { Id = 1, Name = folderName };
                this.folders.Add(folder);
            }

            return folder;
        }

        /// <inheritdoc/>
        public void DeleteMailFolder(MailFolder mailFolder)
        {
            if (mailFolder == null)
            {
                throw new ArgumentNullException("mailFolder");
            }

            if (!this.folders.Contains(mailFolder))
            {
                throw new InvalidOperationException(string.Format("Folder {0} does not exist.", mailFolder.Name));
            }

            this.folders.Remove(mailFolder);
        }

        #endregion

        #region Testable members

        public IEnumerable<MailFolder> GetMailFolders()
        {
            return this.folders;
        }

        public void AddConversation(string folderName, int id, string topic, string sender, string recipients, string content)
        {
            var folder = this.folders.Single(f => f.Name.Equals(folderName));
            var mail = new Mail
                           {
                               Id = "1",
                               Body = content,
                               DateTimeReceived = DateTime.Now,
                               From = sender,
                               Recipients = recipients,
                               Subject = topic
                           };
            var mailThread = new MailThread
                                 {
                                     Id = id.ToString(CultureInfo.InvariantCulture),
                                     Topic = topic
                                 };
            mailThread.Mails.Add(mail);
            folder.EmailThreads.Add(mailThread);
        }

        public void AddEmailToConversation(string conversationSubject, string body)
        {
            foreach (var folder in this.folders)
            {
                var mailThread = folder.EmailThreads.First(t => t.Topic.Equals(conversationSubject));
                mailThread.Mails.Add(new Mail
                                         {
                                             Subject = conversationSubject,
                                             Body = body
                                         });
            }
        }

        public void DeleteConversation(string folderName, string subject)
        {
            var folder = this.folders.Single(f => f.Name.Equals(folderName));
            var thread = folder.EmailThreads.Single(t => t.Topic.Equals(subject));
            folder.EmailThreads.Remove(thread);
        }

        public void DeleteMailFromMailThread(string folderName, string conversationSubject)
        {
            var folder = this.folders.Single(f => f.Name.Equals(folderName));
            var thread = folder.EmailThreads.Single(t => t.Topic.Equals(conversationSubject));
            var lastEmail = thread.Mails.Last();
            thread.Mails.Remove(lastEmail);
        }

        #endregion
    }
}