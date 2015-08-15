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

        [TestInitialize]
        public void InitializeTest()
        {
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

        public abstract ITfsServiceProvider CreateTfsServiceProvider();

        public abstract int CreateWorkItem(string title);

        public abstract void AddLinkToWorkItem(int parentWorkItemId, int childWorkItemId);

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