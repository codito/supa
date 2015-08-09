namespace Supa.Platform.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.TeamFoundation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform;

    [TestClass]
    public abstract class TfsServiceProviderTestsBase
    {
        private ITfsServiceProvider tfsServiceProvider;

        [TestInitialize]
        public void InitializeTest()
        {
            this.tfsServiceProvider = this.CreateTfsServiceProvider();
        }

        [TestMethod]
        public void TfServiceProviderThrowsIfConfigureIsCalledWithNullConfiguration()
        {
            Action action = () => this.tfsServiceProvider.Configure(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void TfsServiceProviderConfigureThrowsIfAuthenticationFails()
        {
            var invalidCredentialConfig = new TfsServiceProviderConfiguration("invalidUsername", "invalidPassword");

            Action action = () => this.tfsServiceProvider.Configure(invalidCredentialConfig);

            action.ShouldThrow<TeamFoundationServerUnauthorizedException>();
        }

        [TestMethod]
        public void TfsServiceProviderConfigureThrowsIfUnableToRetrieveParentWorkItem()
        {
            // test for actual implementation
        }

        public abstract ITfsServiceProvider CreateTfsServiceProvider();

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