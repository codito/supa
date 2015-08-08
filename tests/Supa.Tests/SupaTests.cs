// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupaTests.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa.Tests
{
    using System;

    using FluentAssertions;

    using JsonConfig;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SupaTests
    {
        private readonly SupaApp supaApp;

        public SupaTests()
        {
            this.supaApp = new SupaApp();
        }

        #region ReadConfiguration scenarios

        [TestMethod]
        public void SupaAppReadConfigurationShouldNotThrowForNullFilePath()
        {
            Action action = () => new SupaApp().ReadConfiguration(null);

            action.ShouldNotThrow();
        }

        [TestMethod]
        public void SupaAppReadConfigurationShouldReturnSupaAppInstance()
        {
            this.supaApp.ReadConfiguration(null).Should().BeSameAs(this.supaApp);
        }

        [TestMethod]
        public void SupaAppReadConfigurationShouldSetConfigurationAsUserConfigIfFilePathIsNull()
        {
            this.supaApp.ReadConfiguration(null);

            this.supaApp.Configuration.Should().BeSameAs(Config.User);
        }

        [TestMethod]
        public void SupaAppReadConfigurationReadsConfigurationFromFilePath()
        {
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, "{ StoreOwner: \"John Doe\", Fruits: [\"apple\", \"banana\"]}");

            dynamic result = this.supaApp.ReadConfiguration(tempFile).Configuration;

            this.supaApp.Configuration.Should().NotBeNull();
            Assert.AreEqual("John Doe", result.StoreOwner);
            CollectionAssert.AreEquivalent(new[] { "apple", "banana" }, result.Fruits);
        }

        [TestMethod]
        public void SupaAppReadConfigurationShouldSetFieldsAsNullIfNoSettingsFileIsAvailable()
        {
            dynamic result = this.supaApp.ReadConfiguration(null).Configuration;

            Assert.IsTrue(string.IsNullOrEmpty(result.StoreOwner));
            Assert.IsTrue(string.IsNullOrEmpty(result.Fruits));
        }

        #endregion
    }
}