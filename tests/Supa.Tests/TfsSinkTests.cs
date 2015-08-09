namespace Supa.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsSinkTests
    {
        //[TestMethod]
        //public void TfsSinkShouldThrowIfTfsUrlIsNullOrEmpty()
        //{
        //    Action createTfsSink = () => new TfsSink(null);

        //    createTfsSink.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("tfsUrl");
        //}

        // TfsSinkShouldThrowIfWorkItemTemplateIsNullOrEmpty
        // TfsSinkShouldThrowIfWorkItemTemplateDoesNotExist

        // TfsSinkSerializeShouldReturnAWorkItemObject
        // TfsSinkValidateShouldThrowIfWorkItemIsCompatible
        // TfsSinkValidateShouldReturnTrueIfWorkItemIsValid
        // TfsSinkCommitShouldSaveWorkItem
    }

    public class TestableTfsSink : TfsSink
    {
        public TestableTfsSink(Uri serviceUri, int parentWorkItem, string issueTemplatePath)
            : base(new TfsServiceProviderSimulator(serviceUri), parentWorkItem, issueTemplatePath)
        {
        }
    }
}