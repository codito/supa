namespace Supa.Platform.Tests
{
    using System;
    using System.Net;

    using FluentAssertions;

    using Microsoft.TeamFoundation;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TfsSoapServiceProviderTests : TfsServiceProviderTestsBase
    {
        private static string tfsServiceProviderUri;

        private static string tfsServiceProviderTestUser;

        private static string tfsServiceProviderTestPassword;

        private static int parentWorkItemId;

        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            TfsSoapServiceProviderTests.SkipTestsIfEnvironmentIsNotSet();

            tfsServiceProviderUri = Environment.GetEnvironmentVariable("SupaTfsServiceProviderServiceUrl");
            tfsServiceProviderTestUser = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestUser");
            tfsServiceProviderTestPassword = Environment.GetEnvironmentVariable("SupaTfsServiceProviderTestPassword");

            // Create a parent work item
            parentWorkItemId = TfsSoapServiceProviderTests.CreateWorkItemInternal("TfsSoapServiceProviderTests: Parent Work Item");
        }

        [TestMethod]
        public void TfsServiceProviderThrowsArgumentNullExceptionForNullServiceUri()
        {
            Action action = () => { var x = new TfsSoapServiceProvider(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        public override Type UnauthorizedExceptionType => typeof(TeamFoundationServerUnauthorizedException);

        public override Type WorkItemDoesNotExistExceptionType => typeof(DeniedOrNotExistException);

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsSoapServiceProvider(new Uri(tfsServiceProviderUri));
        }

        public override int CreateWorkItem(string title)
        {
            return CreateWorkItemInternal(title);
        }

        public override void AddLinkToWorkItem(int parentId, int childWorkItemId)
        {
            throw new NotImplementedException();
        }

        public override TfsServiceProviderConfiguration CreateDefaultConfiguration()
        {
            return new TfsServiceProviderConfiguration(tfsServiceProviderTestUser, tfsServiceProviderTestPassword)
                       {
                           ParentWorkItemId = parentWorkItemId
                       };
        }

        private static int CreateWorkItemInternal(string title)
        {
            var networkCredential = new NetworkCredential(tfsServiceProviderTestUser, tfsServiceProviderTestPassword);
            using (var tfsProjectCollection = new TfsTeamProjectCollection(
                    new Uri(tfsServiceProviderUri),
                    networkCredential))
            {
                tfsProjectCollection.Authenticate();

                var workItemStore = new WorkItemStore(tfsProjectCollection);
                var workItem = workItemStore.Projects[0].WorkItemTypes["Task"].NewWorkItem();
                workItem.Title = title;

                workItem.Validate();
                workItem.Save();

                return workItem.Id;
            }
        }
    }
}