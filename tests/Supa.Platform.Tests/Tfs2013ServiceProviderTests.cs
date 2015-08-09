namespace Supa.Platform.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform;

    [TestClass]
    public class Tfs2013ServiceProviderTests
    {
        private readonly TfsSoapServiceProvider tfsServiceProvider;

        public Tfs2013ServiceProviderTests()
        {
            this.tfsServiceProvider = new TfsSoapServiceProvider(new Uri("http://dummyUri"));
            //var tfsServiceProviderConfig = new TfsServiceProviderConfiguration("dummyUsername", "dummyPassword");
        }

        [TestMethod]
        public void TfsServiceProviderThrowsArgumentNullExceptionForNullServiceUri()
        {
            Action action = () => { var x = new TfsSoapServiceProvider(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        [TestMethod]
        public void TfServiceProviderThrowsIfConfigureIsCalledWithNullConfiguration()
        {
            this.tfsServiceProvider.Invoking(s => s.Configure(null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void TfsServiceProviderConfigureThrowsIfAuthenticationFails()
        {
            // var invalidCredentialConfig = new TfsServiceProviderConfiguration("invalidUsername", "truePassword");
            // test for actual implementation
        }

        [TestMethod]
        public void TfsServiceProviderConfigureThrowsIfUnableToRetrieveParentWorkItem()
        {
            // test for actual implementation
        }
    }
}