namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using FluentAssertions;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TfsSoapServiceProviderTests : TfsServiceProviderTestsBase
    {
        private static readonly Dictionary<int, string> WorkItemsDictionary = new Dictionary<int, string>();
            
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
            const string ParentWorkItemTitle = "TfsSoapServiceProviderTests: Parent Work Item";
            parentWorkItemId = TfsSoapServiceProviderTests.CreateWorkItemInternal(ParentWorkItemTitle);
        }

        [ClassCleanup]
        public static void CleanupTestSuite()
        {
            DeleteWorkItemInternal(WorkItemsDictionary.Keys);
        }

        [TestMethod]
        public void TfsServiceProviderThrowsArgumentNullExceptionForNullServiceUri()
        {
            // ReSharper disable once UnusedVariable
            Action action = () => { var x = new TfsSoapServiceProvider(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsSoapServiceProvider(new Uri(tfsServiceProviderUri));
        }

        public override int CreateWorkItem(string title)
        {
            return CreateWorkItemInternal(title);
        }

        public override void AddLinkToWorkItem(int parentId, int childWorkItemId, string comment)
        {
            using (var tfsProjectCollection = GetProjectCollection())
            {
                var workItemStore = new WorkItemStore(tfsProjectCollection);
                var parentWorkItem = workItemStore.GetWorkItem(parentId);
                var linked = false;

                // Update if there's an existing link already
                foreach (var link in parentWorkItem.Links)
                {
                    var relatedLink = link as RelatedLink;
                    if (relatedLink != null && relatedLink.RelatedWorkItemId == childWorkItemId)
                    {
                        relatedLink.Comment = comment;
                        linked = true;
                    }
                }

                if (!linked)
                {
                    parentWorkItem.Links.Add(new RelatedLink(childWorkItemId) { Comment = comment });
                }

                parentWorkItem.Validate();
                parentWorkItem.Save();
            }
        }

        public override TfsServiceProviderConfiguration CreateDefaultConfiguration()
        {
            return new TfsServiceProviderConfiguration(tfsServiceProviderTestUser, tfsServiceProviderTestPassword)
                       {
                           ParentWorkItemId = parentWorkItemId,
                           WorkItemType = "Task"
                       };
        }

        private static int CreateWorkItemInternal(string title)
        {
            // Get work item from cache, saves some time for us!
            if (WorkItemsDictionary.Any(w => w.Value.Equals(title)))
            {
                return WorkItemsDictionary.Single(w => w.Value.Equals(title)).Key;
            }

            using (var tfsProjectCollection = GetProjectCollection())
            {
                var workItemStore = new WorkItemStore(tfsProjectCollection);
                var workItem = workItemStore.Projects[0].WorkItemTypes["Task"].NewWorkItem();
                workItem.Title = title;

                workItem.Validate();
                workItem.Save();

                WorkItemsDictionary.Add(workItem.Id, workItem.Title);

                return workItem.Id;
            }
        }

        private static void DeleteWorkItemInternal(IEnumerable<int> workItems)
        {
            using (var tfsProjectCollection = GetProjectCollection())
            {
                tfsProjectCollection.Authenticate();

                var workItemStore = new WorkItemStore(tfsProjectCollection);

                workItemStore.DestroyWorkItems(workItems);
            }
        }

        private static TfsTeamProjectCollection GetProjectCollection()
        {
            var networkCredential = new NetworkCredential(tfsServiceProviderTestUser, tfsServiceProviderTestPassword);
            var tfsClientCredentials = new TfsClientCredentials(new BasicAuthCredential(networkCredential))
                                           {
                                               AllowInteractive = false
                                           };

            var tfsProjectCollection = new TfsTeamProjectCollection(
                new Uri(tfsServiceProviderUri),
                tfsClientCredentials);
            tfsProjectCollection.Authenticate();

            return tfsProjectCollection;
        }
    }
}