namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class ExchangeServiceProviderSimulatorTests : ExchangeServiceProviderTestsBase
    {
        private static readonly Uri ServiceUrl = new Uri("http://dummyUri");

        private static ExchangeServiceProviderSimulator exchangeServiceProvider;

        [ClassInitialize]
        public static void InitializeProvider(TestContext testContext)
        {
            exchangeServiceProvider = new ExchangeServiceProviderSimulator(ServiceUrl, new NetworkCredential());

            // Create default test folders
            exchangeServiceProvider.GetMailFolder(ExchangeServiceProviderTestsBase.TestFolder, true);

            // Create a test conversation with two emails
            var testclass = new ExchangeServiceProviderSimulatorTests();
            testclass.CreateMailThreadInTestFolder(
                subject: ExchangeServiceProviderTestsBase.FirstConversationSubject,
                body: ExchangeServiceProviderTestsBase.FirstConversationContent,
                recipients: ExchangeServiceProviderTestsBase.TestUser);

            //AddMailToMailThread(
            //    conversationSubject: FirstConversationSubject,
            //    body: FirstConversationSecondEmailContent);
        }

        protected override IExchangeServiceProvider GetDefaultExchangeServiceProvider()
        {
            return exchangeServiceProvider;
        }

        protected override IExchangeServiceProvider CreateExchangeServiceProvider(Uri serviceUrl, NetworkCredential credential)
        {
            return new ExchangeServiceProviderSimulator(serviceUrl, credential);
        }

        protected override void ValidateInboxContainsFolder(string folderName)
        {
            var mailFolder = exchangeServiceProvider.GetMailFolders().SingleOrDefault(m => m.Name.Equals(folderName, StringComparison.CurrentCultureIgnoreCase));

            mailFolder.Should().NotBeNull();
        }

        protected override void CreateMailThreadInTestFolder(string subject, string body, string recipients)
        {
            exchangeServiceProvider.AddConversation(
                ExchangeServiceProviderTestsBase.TestFolder,
                1,
                subject,
                ExchangeServiceProviderTestsBase.TestUser,
                recipients,
                body);
        }

        protected override void AddMailToMailThread(string conversationSubject, string body)
        {
            exchangeServiceProvider.AddEmailToConversation(conversationSubject, body);
        }

        protected override void DeleteMailThreadInTestFolder(string subject)
        {
            exchangeServiceProvider.DeleteConversation(ExchangeServiceProviderTestsBase.TestFolder, subject);
        }

        protected override void DeleteLastMailFromMailThread(string conversationSubject)
        {
            exchangeServiceProvider.DeleteMailFromMailThread(
                ExchangeServiceProviderTestsBase.TestFolder,
                conversationSubject);
        }
    }
}