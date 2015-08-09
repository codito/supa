namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.Tests.TestDoubles;

    [TestClass]
    public class TfsWorkItemTests
    {
        [TestMethod]
        public void TfsWorkItemCtorShouldThrowIfWorkItemIsNull()
        {
            // ReSharper disable once UnusedVariable
            Action action = () => { var x = new TfsWorkItem(null); };

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("workItem");
        }

        [TestMethod]
        public void TfsWorkItemCtorShouldThrowIfWorkItemIsInvalid()
        {
            // ReSharper disable once UnusedVariable
            Action action = () => { var x = new TfsWorkItem(23); };

            action.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("workItem");
        }

        [TestMethod]
        public void TfsWorkItemCtorShouldSetTheUnderlyingWorkItem()
        {
            var dummyWorkItem = new Dictionary<string, string> { { "dummyField", "value1" } };
            var tfsWorkItem = new TestableTfsWorkItem<Dictionary<string, string>>(dummyWorkItem);

            tfsWorkItem.Item.Should().Be(dummyWorkItem);
            tfsWorkItem.WorkItemBaseType.Should().Be<Dictionary<string, string>>();
        }
    }
}