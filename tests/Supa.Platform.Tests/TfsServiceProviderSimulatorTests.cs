namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsServiceProviderSimulatorTests : TfsServiceProviderTestsBase
    {
        public override Type UnauthorizedExceptionType => typeof(VssUnauthorizedException);

        public override Type WorkItemDoesNotExistExceptionType => typeof(KeyNotFoundException);

        public override ITfsServiceProvider CreateTfsServiceProvider()
        {
            return new TfsServiceProviderSimulator(new Uri("http://dummyUri"));
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
            return new TfsServiceProviderConfiguration("testUser", "testPassword");
        }
    }
}