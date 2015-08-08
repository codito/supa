namespace Supa.Platform.Tests
{
    using System;
    using System.Linq;
    using System.Net;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class ExchangeServiceProviderTestsBase
    {
        public const string TestFolder = "SupaTestFolder";

        public const string FirstConversationSubject = "First conversation";

        public const string FirstConversationContent = "Sample content for first conversation";

        public const string FirstConversationSecondEmailContent =
            "Sample content for second email in first conversation";

        public static readonly string TestUser =
            Environment.GetEnvironmentVariable("SupaExchangeServiceProviderTestUser", EnvironmentVariableTarget.User);

        public static readonly string TestUserPassword =
            Environment.GetEnvironmentVariable("SupaExchangeServiceProviderTestPassword", EnvironmentVariableTarget.User);

        private const string SecondConversationSubject = "Second conversation";

        private const string SecondConversationContent = "Sample content for second conversation";

        [TestMethod]
        public void ExchangeServiceProviderShouldThrowForNullServiceUrl()
        {
            Action action = () => this.CreateExchangeServiceProvider(null, CredentialCache.DefaultNetworkCredentials);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUrl");
        }

        [TestMethod]
        public void ExchangeServiceProviderShouldThrowIfNetworkCredentialIsNull()
        {
            Action action = () => this.CreateExchangeServiceProvider(new Uri("http://dummyUri"), null);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("credential");
        }

        #region DeleteMailFolder Scenarios

        [TestMethod]
        public void DeleteMailFolderShouldThrowIfMailFolderIsNull()
        {
            Action action = () => this.GetDefaultExchangeServiceProvider().DeleteMailFolder(null);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("mailFolder");
        }

        [TestMethod]
        public void DeleteMailFolderShouldThrowIfFolderDoesNotExist()
        {
            var nonExistentFolder = new MailFolder { Id = 102, Name = "nonExistentFolder" };

            Action action = () => this.GetDefaultExchangeServiceProvider().DeleteMailFolder(nonExistentFolder);

            action.ShouldThrow<InvalidOperationException>().And.Message.Should().Be("Folder nonExistentFolder does not exist.");
        }

        [TestMethod]
        public void DeleteMailFolderShouldDeleteFolderIfExist()
        {
            var exchangeServiceProvider = this.GetDefaultExchangeServiceProvider();
            var mailFolder = exchangeServiceProvider.GetMailFolder("dummyFolder12", createIfNotExist: true);

            exchangeServiceProvider.DeleteMailFolder(mailFolder);

            exchangeServiceProvider.GetMailFolder("dummyFolder12", createIfNotExist: false).Should().BeNull();
        }

        #endregion

        #region GetMailFolder Scenarios

        [TestMethod]
        public void GetMailFolderShouldThrowIfFolderNameIsNullOrEmpty()
        {
            Action action = () => this.GetDefaultExchangeServiceProvider().GetMailFolder(null, false);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("folderName");
        }

        [TestMethod]
        public void GetMailFolderShouldCreateFolderIfCreateIfNotExistIsTrue()
        {
            const string NonExistentFolder = "nonExistentFolderThatWeWillCreate";

            var mailFolder = this.GetDefaultExchangeServiceProvider().GetMailFolder(NonExistentFolder, true);

            mailFolder.Name.Should().Be(NonExistentFolder);
            this.ValidateInboxContainsFolder(NonExistentFolder);
            this.GetDefaultExchangeServiceProvider().DeleteMailFolder(mailFolder);
        }

        // GetMailFolderShouldFindFolderInInbox
        // GetMailFolderShouldReturnNullIfFolderNotExistAndCreateIfNotExistIsFalse

        #endregion

        #region GetConversations Scenarios
        // TODO temporarily disabled
        //[TestMethod]
        public void GetConversationsShouldThrowIfFolderDoesNotExist()
        {
            Action action = () => this.GetDefaultExchangeServiceProvider().GetEmailThreads("nonExistentFolder");

            action.ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void GetConversationsShouldReturnAllItemsInAFolder()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);

            items.Count().Should().Be(1);
        }

        [TestMethod]
        public void GetConversationsShouldQueryForNewMailThreadsInAFolder()
        {
            var itemsCount = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder).Count();
            this.CreateMailThreadInTestFolder(SecondConversationSubject, SecondConversationContent, TestUser);

            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);

            itemsCount.Should().Be(1);
            items.Count().Should().Be(2);

            this.DeleteMailThreadInTestFolder(SecondConversationSubject);

            items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            items.Count().Should().Be(1);
        }
        #endregion

        #region MailThread Scenarios
        [TestMethod]
        public void GetConversationsShouldReturnMailThreadWithIdAsConversationId()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstConversation = items.Single();

            firstConversation.Id.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GetConversationShouldReturnMailThreadWithTopicAsConversationSubject()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstConversation = items.Single();

            firstConversation.Topic.Should().Be(FirstConversationSubject);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailThreadWithMailForConversationWithOneEmail()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstConversation = items.Single();

            firstConversation.Mails.Should().NotBeNull();
            firstConversation.Mails.Count.Should().Be(1);
        }

        [TestMethod]
        public void GetConversationShouldAddNewItemsToMailThread()
        {
            var firstConversation = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder).Single();
            var initialMailCount = firstConversation.Mails.Count;
            this.AddMailToMailThread(FirstConversationSubject, FirstConversationSecondEmailContent);

            firstConversation = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder).Single();
            firstConversation.Mails.Count.Should().Be(initialMailCount + 1);

            this.DeleteLastMailFromMailThread(FirstConversationSubject);
        }

        #endregion

        #region Mail Scenarios

        [TestMethod]
        public void GetConversationShouldReturnMailWithId()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.Id.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithSubject()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.Subject.Should().Be(FirstConversationSubject);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithSubjectSameAsConversationTopic()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder).ToList();
            var firstMail = items.Single().Mails.Single();

            firstMail.Subject.Should().Be(items.Single().Topic);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithFromAsTestUser()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.From.Should().Be(TestUser);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithRecipientsAsTestUser()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.Recipients.Should().Be(TestUser);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithBodyAsMailContent()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.Body.Should().Be(FirstConversationContent);
        }

        [TestMethod]
        public void GetConversationShouldReturnMailWithDateTimeReceivedSet()
        {
            var items = this.GetDefaultExchangeServiceProvider().GetEmailThreads(TestFolder);
            var firstMail = items.Single().Mails.Single();

            firstMail.DateTimeReceived.Should().BeCloseTo(DateTime.Now, 20 * 60 * 1000);
        }
        #endregion

        protected abstract IExchangeServiceProvider GetDefaultExchangeServiceProvider();

        protected abstract IExchangeServiceProvider CreateExchangeServiceProvider(Uri serviceUrl, NetworkCredential credential);

        protected abstract void ValidateInboxContainsFolder(string folderName);

        protected abstract void CreateMailThreadInTestFolder(string subject, string body, string recipients);

        protected abstract void AddMailToMailThread(string conversationSubject, string body);

        protected abstract void DeleteMailThreadInTestFolder(string subject);

        protected abstract void DeleteLastMailFromMailThread(string conversationSubject);
    }
}