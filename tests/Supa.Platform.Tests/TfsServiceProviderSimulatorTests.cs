namespace Supa.Platform.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsServiceProviderSimulatorTests : TfsServiceProviderTestsBase
    {
        private static int parentWorkItemId;

        private static int workItemSeed;

        private TfsServiceProviderSimulator tfsServiceProviderSimulator;


        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            workItemSeed = 1020;
        }

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            this.tfsServiceProviderSimulator = new TfsServiceProviderSimulator(new Uri("http://dummyUri"));
            return this.tfsServiceProviderSimulator;
        }

        public override int CreateWorkItem(string title)
        {
            var workItemId = workItemSeed++;
            this.tfsServiceProviderSimulator.CreateWorkItem(workItemId, title);
            return workItemId;
        }

        public override void AddLinkToWorkItem(int parentId, int childId, string comment)
        {
            this.tfsServiceProviderSimulator.AddLinkToWorkItem(parentId, childId, comment);
        }

        public override TfsServiceProviderConfiguration CreateDefaultConfiguration()
        {
            parentWorkItemId = this.CreateWorkItem("TfsServiceProviderSimulator: Parent work item");
            return new TfsServiceProviderConfiguration("testUser", "testPassword")
                       {
                           ParentWorkItemId = parentWorkItemId
                       };
        }
    }
}