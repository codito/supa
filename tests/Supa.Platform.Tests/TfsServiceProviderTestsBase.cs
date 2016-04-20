namespace Supa.Platform.Tests
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform;
    using TestDoubles;

    [TestClass]
    public abstract class TfsServiceProviderTestsBase
    {
        private const string IssueId1 = "issueId1:5";
        private const string IssueId2 = "issueId2:10";
        private const string IssueTitle1 = "Workitem for Issue 1";
        private const string IssueTitle2 = "Workitem for Issue 2";
        private readonly int issueActivityCount1 = 5;

        private ITfsServiceProvider tfsServiceProvider;
        private TfsServiceProviderConfiguration tfsServiceProviderDefaultConfig;

        private int childWorkItem1;

        [TestInitialize]
        public void InitializeTest()
        {
            // Reset the connection state at start of test
            this.tfsServiceProvider = this.CreateTfsServiceProvider();
            this.tfsServiceProviderDefaultConfig = this.CreateDefaultConfiguration();
        }

        [TestMethod]
        public void TfServiceProviderThrowsIfConfigureIsCalledWithNullConfiguration()
        {
            Func<Task> action = async () => await this.tfsServiceProvider.ConfigureAsync(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        [Ignore] // toso: Remove this when we move to rest api
        public void TfsServiceProviderConfigureThrowsIfAuthenticationFails()
        {
            var invalidCredentialConfig = new TfsServiceProviderConfiguration("invalidUsername", "invalidPassword");
            invalidCredentialConfig.ParentWorkItemId = this.tfsServiceProviderDefaultConfig.ParentWorkItemId;
            invalidCredentialConfig.WorkItemType = "Task";

            Func<Task> action = async () => await this.tfsServiceProvider.ConfigureAsync(invalidCredentialConfig);

            action.ShouldThrow<TeamFoundationServerUnauthorizedException>();
        }

        [TestMethod]
        public void TfsServiceProviderConfigureThrowsIfUnableToRetrieveParentWorkItem()
        {
            var defaultConfig = this.CreateDefaultConfiguration();
            defaultConfig.ParentWorkItemId = 9999999; // invalid id

            Func<Task> action = async () => await this.tfsServiceProvider.ConfigureAsync(defaultConfig);

            action.ShouldThrow<DeniedOrNotExistException>();
        }

        [TestMethod]
        public void TfsServiceProviderConfigureSetsupParentWorkItemFromTfsServer()
        {
            Func<Task> action =
                async () => await this.tfsServiceProvider.ConfigureAsync(this.CreateDefaultConfiguration());

            action.ShouldNotThrow();
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemForIssueThrowsIfIssueIdIsNullOrEmpty()
        {
            Action action = () => this.tfsServiceProvider.GetWorkItemForIssue(null, 0);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemForIssueThrowsIfParentWorkItemIsNotAvailable()
        {
            Action action = () => this.tfsServiceProvider.GetWorkItemForIssue("dummyIssueId", 0);

            action.ShouldThrow<Exception>().And.Message.Should().Contain("ConfigureAsync");
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemShouldReturnExistingWorkItemForIssueId()
        {
            this.SetupParentWorkItemWithLinks();
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();

            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(IssueId1, this.issueActivityCount1);

            tfsWorkItem.HasChange.Should().BeFalse();
            tfsWorkItem.IssueSignature.Should().Be($"{IssueId1}:{this.issueActivityCount1}");
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemShouldReturnNewWorkItemIfActivityCountDoesNotMatch()
        {
            this.SetupParentWorkItemWithLinks();
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();

            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(IssueId1, this.issueActivityCount1 + 10);

            tfsWorkItem.HasChange.Should().BeTrue();
            tfsWorkItem.IssueSignature.Should().Be($"{IssueId1}:{this.issueActivityCount1+10}");
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemShouldSkipRelatedLinkForInvalidCommentStringInWorkitemLink()
        {
            var childWorkItem2 = this.CreateWorkItem(IssueTitle2);
            this.AddLinkToWorkItem(
                this.tfsServiceProviderDefaultConfig.ParentWorkItemId,
                childWorkItem2,
                "randomStringWithoutColon");
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();

            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(IssueId2, this.issueActivityCount1 + 10);

            tfsWorkItem.HasChange.Should().BeTrue();
            tfsWorkItem.IssueSignature.Should().Be($"{IssueId2}:{this.issueActivityCount1+10}");
        }

        [TestMethod]
        public void TfsServiceProviderGetWorkItemShouldReturnNewItemIfParentWorkItemHasNoLinks()
        {
            var parentWorkItemId = this.CreateWorkItem("TfsServiceProviderTests: Workitem 3");
            this.tfsServiceProviderDefaultConfig.ParentWorkItemId = parentWorkItemId;
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();

            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid3", 1);

            tfsWorkItem.HasChange.Should().BeTrue();
            tfsWorkItem.IssueSignature.Should().Be("issueid3:1");
        }

        [TestMethod]
        public void TfsServiceProviderSaveWorkItemShouldThrowForInvalidItemType()
        {
            var tfsWorkItem = new TestableTfsWorkItem<int>(20);

            Action action = () => this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);

            action.ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void TfsServiceProviderSaveWorkItemShouldUpdateLinkCommentIfActivityCountIsModified()
        {
            // Setup a existing workitem with activity count as 1
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid4", 1);
            tfsWorkItem.UpdateField("Title", "TfsServiceProviderTests: Workitem 4");
            this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);

            // Query for an updated activity count, save the item
            tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid4", 2);
            this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);
            tfsWorkItem.HasChange.Should().BeTrue();
            tfsWorkItem.IssueSignature.Should().Be("issueid4:2");

            // Query for the same activity count
            tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid4", 2);
            tfsWorkItem.HasChange.Should().BeFalse();
            tfsWorkItem.IssueSignature.Should().Be("issueid4:2");
        }

        [TestMethod]
        public void TfsServiceProviderSaveWorkItemShouldAddLinkIfWorkItemIsNew()
        {
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid5", 1);
            tfsWorkItem.HasChange.Should().BeTrue();

            tfsWorkItem.UpdateField("Title", "TfsServiceProviderTests: Workitem 5");
            this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);

            tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid5", 1);
            tfsWorkItem.HasChange.Should().BeFalse();
            tfsWorkItem.IssueSignature.Should().Be("issueid5:1");
        }

        [TestMethod]
        public void TfsServiceProviderSaveWorkItemShouldThrowIfWorkItemIsNotReadyToSave()
        {
            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue("issueid6", 1);
            tfsWorkItem.HasChange.Should().BeTrue();

            Action action = () => this.tfsServiceProvider.SaveWorkItem(tfsWorkItem);

            action.ShouldThrow<ValidationException>();
        }


        public abstract ITfsServiceProvider CreateTfsServiceProvider();

        public abstract int CreateWorkItem(string title);

        public abstract void AddLinkToWorkItem(int parentWorkItemId, int childWorkItemId, string comment);

        public abstract TfsServiceProviderConfiguration CreateDefaultConfiguration();

        protected static void SkipTestsIfEnvironmentIsNotSet()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SupaTfsServiceProviderServiceUrl")))
            {
                Assert.Inconclusive("TfsServiceProviderTests require SupaTfsServiceProviderServiceUrl environment variable.");
            }

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestUser")))
            {
                Assert.Inconclusive("TfsServiceProviderTests require SupaTfsServiceProviderTestUser environment variable.");
            }

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestPassword")))
            {
                Assert.Inconclusive("TfsServiceProviderTests require SupaTfsServiceProviderTestPassword environment variable.");
            }
        }

        private void SetupParentWorkItemWithLinks()
        {
            var parentItemId = this.tfsServiceProviderDefaultConfig.ParentWorkItemId;
            this.childWorkItem1 = this.CreateWorkItem(IssueTitle1);

            this.AddLinkToWorkItem(parentItemId, this.childWorkItem1, IssueId1);
        }
    }
}