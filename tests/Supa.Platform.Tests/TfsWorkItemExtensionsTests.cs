namespace Supa.Platform.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform.Tests.TestDoubles;

    [TestClass]
    public class TfsWorkItemExtensionsTests
    {
        [TestMethod]
        public void TfsWorkItemExtensionsUpdateFieldShouldThrowIfTfsWorkItemIsNull()
        {
            var tfsWorkItem = (TfsWorkItem)null;

            // ReSharper disable once ExpressionIsAlwaysNull
            tfsWorkItem.Invoking(t => TfsWorkItemExtensions.UpdateField(t, "dummyField", "dummyValue")).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void TfsWorkItemExtensionsUpdateFieldShouldThrowIfWorkItemDoesNotHaveTheField()
        {
            var dummyWorkItem = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            var tfsWorkItem = new TestableTfsWorkItem<ReadOnlyDictionary<string, string>>(dummyWorkItem);

            tfsWorkItem.Invoking(t => t.UpdateField("nonExistentField", "dummyValue")).ShouldThrow<Exception>();
        }

        [TestMethod]
        public void TfsWorkItemExtensionsUpdateFieldShouldUpdateTheFieldOfTheWorkItem()
        {
            var dummyWorkItem = new Dictionary<string, string> { { "dummyField", "value1" } };
            var tfsWorkItem = new TestableTfsWorkItem<Dictionary<string, string>>(dummyWorkItem);
            
            tfsWorkItem.UpdateField("dummyField", "value2");

            dummyWorkItem["dummyField"].Should().Be("value2");
        }
    }
}