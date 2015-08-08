namespace Supa.Tests
{
    using System;
    using System.Linq;
    using System.Net;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;
    using Supa.Tests.TestDoubles;

    [TestClass]
    public class ExchangeSourceTests
    {
        private const string DefaultFolderName = "SupaTestFolder";

        private const string DefaultConversationContent = "Some content";

        private const string DefaultConversationReceipant = "recipient@microsoft.com";

        private const int DefaultConversationId = 1;

        private const string DefaultConversationTopic = "Some conversation topic";

        private const string DefaultConversationSender = "sender@microsoft.com";

        private const string DefaultConversationSecondEmailContent = "Content for second email";

        private readonly NetworkCredential defaultNetworkCredential = new NetworkCredential();

        public ExchangeSourceTests()
        {
            this.ExchangeServiceServiceProviderSimulator = new ExchangeServiceProviderSimulator(new Uri("http://dummyUri"), this.defaultNetworkCredential);
            this.ExchangeServiceServiceProviderSimulator.GetMailFolder(DefaultFolderName, true);
            this.ExchangeServiceServiceProviderSimulator.AddConversation(
                folderName: DefaultFolderName,
                id: DefaultConversationId,
                topic: DefaultConversationTopic,
                sender: DefaultConversationSender,
                recipients: DefaultConversationReceipant,
                content: DefaultConversationContent);

            this.TestableExchangeSource = new TestableExchangeSource(DefaultFolderName, this.ExchangeServiceServiceProviderSimulator);
        }

        private TestableExchangeSource TestableExchangeSource { get; set; }

        private ExchangeServiceProviderSimulator ExchangeServiceServiceProviderSimulator { get; set; }

        [TestMethod]
        public void ExchangeSourceShouldTakeFolderAsInput()
        {
            Action action = () => new TestableExchangeSource(DefaultFolderName, this.ExchangeServiceServiceProviderSimulator);
            action.ShouldNotThrow();
        }

        [TestMethod]
        public void ExchangeSourceShouldThrowIfFolderIsNullOrEmpty()
        {
            Action action = () => new TestableExchangeSource(string.Empty, this.ExchangeServiceServiceProviderSimulator);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("folderName");
        }

        [TestMethod]
        public void ExchangeSourceShouldThrowIfCredentialIsNull()
        {
            Action action = () => new ExchangeSource(new Uri("http://dummyUri"), null, DefaultFolderName);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("credential");
        }

        [TestMethod]
        public void ExchangeSourceShouldThrowIfServiceUrlIsNull()
        {
            Action action = () => new ExchangeSource(null, this.defaultNetworkCredential, DefaultFolderName);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ExchangeSourceShouldDisplayAllThreadsInFolder()
        {
            var items = this.TestableExchangeSource.Issues;

            items.Count().Should().Be(1);
        }

        [TestMethod]
        public void ExchangeSourceShouldDisplayNewThreadsInFolder()
        {
            var dummyTopic = "dummyTopic";
            this.ExchangeServiceServiceProviderSimulator.AddConversation(
                folderName: DefaultFolderName,
                id: 2,
                topic: dummyTopic,
                sender: "dummySender@microsoft.com",
                recipients: "dummyReceipant@microsoft.com",
                content: "dummyContent");

            var items = this.TestableExchangeSource.Issues;

            items.Count().Should().Be(2);

            this.ExchangeServiceServiceProviderSimulator.DeleteConversation(
                folderName: DefaultFolderName,
                subject: dummyTopic);
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheIdAsTheConversationId()
        {
            var items = this.TestableExchangeSource.Issues;

            items.Single().Id.Should().Be(DefaultConversationId.ToString());
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheIdAsTheConversationIdForMultilpleEmails()
        {
            this.ExchangeServiceServiceProviderSimulator.AddEmailToConversation(
                DefaultConversationTopic,
                DefaultConversationSecondEmailContent);

            var items = this.TestableExchangeSource.Issues;

            items.Single().Id.Should().Be(DefaultConversationId.ToString());

            this.ExchangeServiceServiceProviderSimulator.DeleteMailFromMailThread(
                DefaultFolderName,
                DefaultConversationTopic);
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheTopicAsConversationSubject()
        {
            var items = this.TestableExchangeSource.Issues;

            items.Single().Topic.Should().Be(DefaultConversationTopic);
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheDescriptionAsContentOfFirstEmailInConversation()
        {
            this.ExchangeServiceServiceProviderSimulator.AddEmailToConversation(
                DefaultConversationTopic,
                DefaultConversationSecondEmailContent);

            var items = this.TestableExchangeSource.Issues;

            items.Single().Topic.Should().Be(DefaultConversationTopic);

            this.ExchangeServiceServiceProviderSimulator.DeleteMailFromMailThread(
                DefaultFolderName,
                DefaultConversationTopic);
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheActivityCountAsNumberOfEmailsInConversationWithOneEmail()
        {
            var items = this.TestableExchangeSource.Issues;

            items.Single().Activity.Should().Be(1);
        }

        [TestMethod]
        public void ExchangeSourceIssuesHasTheActivityCountAsNumberOfEmailsInConversationWithSeveralEmails()
        {
            this.ExchangeServiceServiceProviderSimulator.AddEmailToConversation(
                DefaultConversationTopic,
                DefaultConversationSecondEmailContent);

            var items = this.TestableExchangeSource.Issues;

            items.Single().Activity.Should().Be(2);

            this.ExchangeServiceServiceProviderSimulator.DeleteMailFromMailThread(
                DefaultFolderName,
                DefaultConversationTopic);
        }

        // Outlook source should not change the state of threads in search folder
        // Outlook source should report IssueSource as Internal for emails from the same domain as the user
        // Outlook source should report IssueSource as External for emails from a different domain as the user
        // Outlook source should report Mail address of sender in SourceDetail
        // Outlook source should report CreatedDate as the sent date of the first email in conversation
        // Outlook source should report LastUpdatedDate as the sent date of the latest email in conversation
        // ExchangeSourceQueriesWithCurrentUserCredentialWithoutUsernameAndPassword
        // ExchangeSourceQueriesWithUsernameAndPasswordForAProvidedUser
        // ExchangeSourceQueriesWithEmailAddressForAProvidedUser
    }
}