// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExchangeServiceProviderTests.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Supa.Platform.Tests
{
    using System;
    using System.Linq;
    using System.Net;

    using FluentAssertions;

    using Microsoft.Exchange.WebServices.Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExchangeServiceProviderTests : ExchangeServiceProviderTestsBase
    {
        // private static readonly Uri ServiceUrl = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
        private static ExchangeServiceProvider exchangeServiceProvider;

        private static ExchangeService exchangeService;

        private static MailFolder testFolder;

        [ClassInitialize]
        public static void InitializeProvider(TestContext testContext)
        {
            var serviceEndpoint = Environment.GetEnvironmentVariable("SupaExchangeServiceProviderServiceUrl", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(serviceEndpoint))
            {
                Assert.Inconclusive("SupaExchangeServiceProviderServiceUrl is not set. Please set it to an exchange endpoint. E.g. https://outlook.office365.com/EWS/Exchange.asmx");
            }

            if (string.IsNullOrEmpty(TestUser))
            {
                Assert.Inconclusive("SupaExchangeServiceProviderTestUser is not set. Please set it to the username for exchange endpoint.");
            }

            if (string.IsNullOrEmpty(TestUserPassword))
            {
                Assert.Inconclusive("SupaExchangeServiceProviderTestPassword is not set. Please set it to the password for exchange endpoint.");
            }

            var serviceUrl = new Uri(serviceEndpoint);
            var credential = new NetworkCredential(TestUser, TestUserPassword);
            exchangeServiceProvider = new ExchangeServiceProvider(serviceUrl, credential);

            exchangeService = new ExchangeService(ExchangeVersion.Exchange2013)
                                  {
                                      Credentials = credential, 
                                      TraceEnabled = false, 
                                      TraceFlags = TraceFlags.All, 
                                      Url = serviceUrl
                                  };

            // Create default test folders
            testFolder = exchangeServiceProvider.GetMailFolder(TestFolder, true);

            // Create a test conversation with two emails
            var testClass = new ExchangeServiceProviderTests();
            testClass.CreateMailThreadInTestFolder(
                subject: FirstConversationSubject, 
                body: FirstConversationContent, 
                recipients: TestUser);

            // AddMailToMailThread(
            // conversationSubject: ExchangeServiceProviderTestsBase.FirstConversationSubject,
            // messageContent: ExchangeServiceProviderTestsBase.FirstConversationSecondEmailContent);
        }

        [ClassCleanup]
        public static void CleanupProvider()
        {
            exchangeServiceProvider.DeleteMailFolder(testFolder);
        }

        protected static void MoveMessageToTestFolder(string subject)
        {
            EmailMessage message = null;
            while (message == null)
            {
                message =
                    exchangeService.FindItems(
                        WellKnownFolderName.Inbox, 
                        new SearchFilter.ContainsSubstring(ItemSchema.Subject, subject), 
                        new ItemView(1)).FirstOrDefault() as EmailMessage;
            }

            message.Move(testFolder.Id as FolderId);
        }

        protected override void ValidateInboxContainsFolder(string folderName)
        {
            var folder = exchangeService.FindFolders(
                WellKnownFolderName.Inbox, 
                new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderName), 
                new FolderView(1)).FirstOrDefault();

            folder.Should().NotBeNull();
        }

        protected override void CreateMailThreadInTestFolder(string subject, string body, string recipients)
        {
            var message = new EmailMessage(exchangeService)
                              {
                                  Subject = subject, 
                                  Body = body
                              };
            message.ToRecipients.Add(recipients);
            message.Send();
            MoveMessageToTestFolder(subject);
        }

        protected override void AddMailToMailThread(string conversationSubject, string messageContent)
        {
            var message = exchangeService.FindItems(
                testFolder.Id as FolderId, 
                new SearchFilter.IsEqualTo(ItemSchema.Subject, conversationSubject), 
                new ItemView(1)).Single() as EmailMessage;

            Assert.IsTrue(message != null, "message != null");
            message.Reply(messageContent, false);
            MoveMessageToTestFolder(conversationSubject);
        }

        protected override void DeleteMailThreadInTestFolder(string subject)
        {
            var message = exchangeService.FindItems(
                testFolder.Id as FolderId, 
                new SearchFilter.IsEqualTo(ItemSchema.Subject, subject), 
                new ItemView(1)).Single() as EmailMessage;

            Assert.IsTrue(message != null, "message != null");
            message.Delete(DeleteMode.HardDelete);
        }

        protected override void DeleteLastMailFromMailThread(string conversationSubject)
        {
            var message = exchangeService.FindItems(
                testFolder.Id as FolderId, 
                new SearchFilter.ContainsSubstring(ItemSchema.Subject, conversationSubject), 
                new ItemView(2)).First() as EmailMessage;

            Assert.IsTrue(message != null, "message != null");
            message.Delete(DeleteMode.HardDelete);
        }

        protected override IExchangeServiceProvider GetDefaultExchangeServiceProvider()
        {
            return exchangeServiceProvider;
        }

        protected override IExchangeServiceProvider CreateExchangeServiceProvider(Uri serviceUrl, NetworkCredential credential)
        {
            return new ExchangeServiceProvider(serviceUrl, credential);
        }
    }
}
