// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExchangeServiceProvider.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    using Microsoft.Exchange.WebServices.Data;

    using Serilog;
    using System.Collections.ObjectModel;

    /// <inheritdoc/>
    public class ExchangeServiceProvider : IExchangeServiceProvider
    {
        private static ExchangeService exchangeService;

        private readonly ILogger logger;

        private readonly Stopwatch watch;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceUrl">
        /// Url for the exchange service provider. E.g. <code>https://outlook.office365.com/EWS/Exchange.asmx</code>
        /// </param>
        /// <param name="credential">
        /// Credentials to login into the service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <see cref="serviceUrl"/> or <see cref="credential"/> are null.
        /// </exception>
        public ExchangeServiceProvider(Uri serviceUrl, NetworkCredential credential)
        {
            this.logger = Log.Logger.ForContext<ExchangeServiceProvider>();

            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            if (credential == null)
            {
                throw new ArgumentNullException("credential");
            }

            this.watch = new Stopwatch();
            if (exchangeService == null)
            {
                this.logger.Debug("Create exchange service reference.");
                this.watch.Start();
                exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
                                      {
                                          Credentials = credential,
                                          TraceEnabled = false,
                                          TraceFlags = TraceFlags.All,
                                          Url = serviceUrl
                                      };
                this.watch.Stop();
                this.logger.Debug("Time taken for exchange discover operation: {ElapsedTime}", this.watch.Elapsed);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<MailThread> GetEmailThreads(string folderName)
        {
            this.logger.Verbose("GetEmailThreads invoked with {folderName}.", folderName);
            var folder = exchangeService.FindFolders(
                WellKnownFolderName.Inbox,
                new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderName),
                new FolderView(1)).FirstOrDefault();
            if (folder == null)
            {
                throw new InvalidOperationException(string.Format("MailFolder {0} doesn't exist.", folderName));
            }

            // Create the view of conversations returned in the response. This view will return the first five
            // conversations, starting with an offset of 0 from the beginning of the results set.
            var view = new ConversationIndexedItemView(Int32.MaxValue, 0, OffsetBasePoint.Beginning);

            // Search the Inbox for conversations and return a results set with the specified view.
            // This results in a call to EWS. 
            this.watch.Restart();
            var conversations = exchangeService.FindConversation(view, folder.Id);
            this.watch.Stop();
            this.logger.Debug("Time taken to find conversations in {folderName} = {ElapsedTime}.", folderName, this.watch.Elapsed);

            // Examine properties on each conversation returned in the response.
            foreach (var conversation in conversations)
            {
                var thread = new MailThread { Id = conversation.Id.ToString(), Topic = conversation.Topic, };
                this.logger.Debug("Created {MailThread}", thread);

                this.logger.Verbose(string.Format("Finding emails contained in the thread. Topic: {0} ", conversation.Topic));
                this.watch.Restart();
                var items = GetConversationItemsSingleConversation(exchangeService, folder.Id, conversation);
                this.watch.Stop();
                this.logger.Debug("Time taken to find emails in {thread} = {ElapsedTime}", thread, this.watch.Elapsed);

                this.logger.Verbose("Convert each message to mail object.");
                this.watch.Restart();
                foreach (var item in items)
                {
                    var mail = item as EmailMessage;
                    if (mail == null)
                    {
                        this.logger.Verbose("Non email item found in {folderName}: {Item}.", folderName, item);
                        continue;
                    }

                    mail.Load(new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

                    thread.Mails.Add(new Mail
                                         {
                                             Id = mail.Id.ToString(),
                                             Body = mail.Body.Text.Trim(),
                                             DateTimeReceived = mail.DateTimeReceived,
                                             From = mail.Sender.Address,
                                             Recipients = string.Join(",", mail.ToRecipients.Select(to => to.Address)),
                                             Subject = mail.Subject
                                         });
                }

                this.watch.Stop();
                this.logger.Debug("Time taken to load properties for emails = {ElapsedTime}", this.watch.Elapsed);
                yield return thread;
            }
        }

        /// <inheritdoc/>
        public MailFolder GetMailFolder(string folderName, bool createIfNotExist)
        {
            this.logger.Debug("GetMailFolder called with {folderName} and {createIfNotExist}", folderName, createIfNotExist);
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            this.watch.Restart();
            var folder = exchangeService.FindFolders(
                WellKnownFolderName.Inbox,
                new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderName),
                new FolderView(1)).FirstOrDefault();
            this.watch.Stop();
            this.logger.Debug("Time taken to find {folderName} = {ElapsedTime}", folderName, this.watch.Elapsed);

            if (folder == null)
            {
                if (createIfNotExist == false)
                {
                    return null;
                }

                folder = new Folder(exchangeService) { DisplayName = folderName };
                folder.Save(WellKnownFolderName.Inbox);
                this.logger.Verbose("Created {folderName} with {Id}.", folderName, folder.Id);
            }

            this.logger.Debug("Got {folderName} with {Id}.", folderName, folder.Id);
            return new MailFolder { Id = folder.Id, Name = folderName };
        }

        /// <inheritdoc/>
        public void DeleteMailFolder(MailFolder mailFolder)
        {
            if (mailFolder == null)
            {
                throw new ArgumentNullException("mailFolder");
            }

            Folder folder = null;
            var folderId = mailFolder.Id as FolderId;
            if (folderId != null)
            {
                folder = Folder.Bind(exchangeService, mailFolder.Id as FolderId);
            }

            if (folder == null)
            {
                throw new InvalidOperationException(string.Format("Folder {0} does not exist.", mailFolder.Name));
            }

            folder.Delete(DeleteMode.HardDelete);
        }

        private static IEnumerable<Item> GetConversationItemsSingleConversation(ExchangeService service, FolderId folderId, Conversation conversation)
        {

            var itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties)
                                      {
                                          RequestedBodyType = BodyType.Text
                                      };
            var itemview = new ItemView(100) { PropertySet = itempropertyset };
            var conversationFilter = new SearchFilter.IsEqualTo(EmailMessageSchema.ConversationTopic, conversation.Topic);
            var items = service.FindItems(folderId, conversationFilter, itemview);
            return items;
        }
    }
}