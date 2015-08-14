namespace Supa.Platform.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TfsRestServiceProviderTests : TfsServiceProviderTestsBase
    {
        private static string tfsServiceProviderUri;

        private static string tfsServiceProviderTestUser;

        private static string tfsServiceProviderTestPassword;

        public override Type UnauthorizedExceptionType => typeof(VssUnauthorizedException);

        public override Type WorkItemDoesNotExistExceptionType => typeof(VssServiceResponseException);

        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            TfsRestServiceProviderTests.SkipTestsIfEnvironmentIsNotSet();

            tfsServiceProviderUri = Environment.GetEnvironmentVariable("SupaTfsServiceProviderServiceUrl");
            tfsServiceProviderTestUser = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestUser");
            tfsServiceProviderTestPassword = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestPassword");
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

        public override int CreateWorkItem(string title)
        {
            throw new NotImplementedException();
        }

        public override void AddLinkToWorkItem(int parentWorkItemId, int childWorkItemId)
        {
            throw new NotImplementedException();
        }

        public override TfsServiceProviderConfiguration CreateDefaultConfiguration()
        {
            return new TfsServiceProviderConfiguration(tfsServiceProviderTestUser, tfsServiceProviderTestPassword)
                       {
                           ParentWorkItemId = 0
                       };
        }
    }
}