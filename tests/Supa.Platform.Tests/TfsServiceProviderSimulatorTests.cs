namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsServiceProviderSimulatorTests : TfsServiceProviderTestsBase
    {
        private static TfsServiceProviderSimulator tfsServiceProviderSimulator;

        private static int parentWorkItemId;

        private static int workItemSeed;

        [ClassInitialize]
        public static void InitializeTestSuite(TestContext testContext)
        {
            workItemSeed = 1020;
            tfsServiceProviderSimulator = new TfsServiceProviderSimulator(new Uri("http://dummyUri"));
            parentWorkItemId = CreateWorkItemInternal("TfsServiceProviderSimulator: Parent work item");
        }

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return tfsServiceProviderSimulator;
        }

        public override int CreateWorkItem(string title)
        {
            return CreateWorkItemInternal(title);
        }

        public override void AddLinkToWorkItem(int parentId, int childId)
        {
            throw new NotImplementedException();
        }

        public override TfsServiceProviderConfiguration CreateDefaultConfiguration()
        {
            return new TfsServiceProviderConfiguration("testUser", "testPassword")
                       {
                           ParentWorkItemId = parentWorkItemId
                       };
        }

        private static int CreateWorkItemInternal(string title)
        {
            var workItemId = workItemSeed++;
            tfsServiceProviderSimulator.CreateWorkItem(workItemId, title);
            return workItemId;
        }
    }
}