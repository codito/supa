namespace Supa.Platform.Tests
{
    using System;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform;

    [TestClass]
    public abstract class TfsServiceProviderTestsBase
    {
        private ITfsServiceProvider tfsServiceProvider;

        private TfsServiceProviderConfiguration tfsServiceProviderDefaultConfig;

        private const string IssueId1 = "issueId1";
        private const string IssueId2 = "issueId2";
        private const string IssueTitle1 = "Workitem for Issue 1";
        private const string IssueTitle2 = "Workitem for Issue 2";
        private int issueActivityCount1 = 5;
        private int issueActivityCount2 = 10;

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
        public void TfsServiceProviderConfigureThrowsIfAuthenticationFails()
        {
            var invalidCredentialConfig = new TfsServiceProviderConfiguration("invalidUsername", "invalidPassword");
            invalidCredentialConfig.ParentWorkItemId = this.tfsServiceProviderDefaultConfig.ParentWorkItemId;

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
            var parentItemId = this.tfsServiceProviderDefaultConfig.ParentWorkItemId;
            var childItemForIssue1 = this.CreateWorkItem(IssueTitle1);
            var childItemForIssue2 = this.CreateWorkItem(IssueTitle2);
            this.AddLinkToWorkItem(parentItemId, childItemForIssue1, IssueId1);

            this.tfsServiceProvider.ConfigureAsync(this.tfsServiceProviderDefaultConfig).Wait();
            var tfsWorkItem = this.tfsServiceProvider.GetWorkItemForIssue(IssueId1, this.issueActivityCount1);

            tfsWorkItem.HasChange.Should().BeFalse();
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
    }
}