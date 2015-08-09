namespace Supa.Platform.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsRestServiceProviderTests : TfsServiceProviderTestsBase
    {
        private static string tfsServiceProviderUri;

        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            TfsRestServiceProviderTests.SkipTestsIfEnvironmentIsNotSet();

            tfsServiceProviderUri = Environment.GetEnvironmentVariable("SupaTfsServiceProviderServiceUrl");
            var tfsServiceProviderTestUser = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestUser");
            var tfsServiceProviderTestPassword = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestPassword");
        }

        [TestMethod]
        public void TfsServiceProviderThrowsArgumentNullExceptionForNullServiceUri()
        {
            Action action = () => { var x = new TfsRestServiceProvider(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsRestServiceProvider(new Uri(tfsServiceProviderUri));
        }
    }

    [TestClass]
    public class TfsServiceProviderSimulatorTests : TfsServiceProviderTestsBase
    {
        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsServiceProviderSimulator(new Uri("http://dummyUri"));
        }
    }

    [TestClass]
    public class TfsSoapServiceProviderTests : TfsServiceProviderTestsBase
    {
        private static string tfsServiceProviderUri;

        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            TfsSoapServiceProviderTests.SkipTestsIfEnvironmentIsNotSet();

            tfsServiceProviderUri = Environment.GetEnvironmentVariable("SupaTfsServiceProviderServiceUrl");
            var tfsServiceProviderTestUser = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestUser");
            var tfsServiceProviderTestPassword = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestPassword");
        }

        [TestMethod]
        public void TfsServiceProviderThrowsArgumentNullExceptionForNullServiceUri()
        {
            Action action = () => { var x = new TfsSoapServiceProvider(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsSoapServiceProvider(new Uri(tfsServiceProviderUri));
        }
    }
}