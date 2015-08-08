namespace Supa.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Ploeh.AutoFixture;

    [TestClass]
    public class CsvSinkTests
    {
        private const string DummyFilePath = "dummyFilePath";

        // CsvSinkSerializeIncludesIdForAnIssue
        // CsvSinkSerializeIncludesTopicForAnIssue
        // CsvSinkSerializeIncludesDescriptionForAnIssue
        // CsvSinkSerializeIncludesCreatedByForAnIssue
        // CsvSinkSerializeIncludesCreatedDateForAnIssue
        // CsvSinkSerializeIncludesLastUpdatedDateForAnIssue
        // CsvSinkUpdateSavesTheLatestSnapshotOfIssuesToCsvFile

        public CsvSinkTests()
        {
            this.Fixture = new Fixture();
        }

        public Fixture Fixture { get; set; }

        [TestMethod]
        public void CsvSinkTakesACsvFileAsInput()
        {
            var csvSink = new CsvSink(DummyFilePath);

            csvSink.Should().NotBeNull();
        }

        [TestMethod]
        public void CsvSinkThrowsForANullOrEmptyCsvFilePath()
        {
            Action createNullCsvSink = () => new CsvSink(null);
            Action createEmptyCsvSink = () => new CsvSink(string.Empty);

            createNullCsvSink.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("csvFilePath");
            createEmptyCsvSink.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("csvFilePath");
        }

        [TestMethod]
        public void CsvSinkGetCsvHeaderShouldReturnAllPublicInstancePropertiesOfIssue()
        {
            var csvSink = GetCsvSink();

            var header = csvSink.GetCsvHeader();

            header.Should().Be("Id,Topic,Description,CreatedBy,CreatedDate,LastUpdatedDate,Activity");
        }

        [TestMethod]
        public void CsvSinkSerializeThrowsForNullIssuesList()
        {
            var csvSink = GetCsvSink();
            Action serialize = () => csvSink.Serialize(null);

            serialize.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("issues");
        }

        [TestMethod]
        public void CsvSinkSerializeReturnsHeaderForNoIssues()
        {
            var csvSink = GetCsvSink();
            var csv = csvSink.Serialize(new List<Issue>());

            csv.Should().NotBeNullOrEmpty();
            csv.Should().StartWith(csvSink.GetCsvHeader());
        }

        [TestMethod]
        public void CsvSinkSerializeReturnsTwoMoreLineThanNumberOfIssues()
        {
            var csvSink = GetCsvSink();
            var issue = this.Fixture.Create<Issue>();

            var csvLines = GetCsvLines(csvSink.Serialize(new List<Issue> { issue }));

            // Last line is always a blank line
            csvLines.Count().Should().Be(3);
            csvLines.Last().Should().BeNullOrEmpty();
        }

        [TestMethod]
        public void CsvSinkSerializeConvertsIssueToACommaSeparatedString()
        {
            var csvSink = GetCsvSink();
            var issue = this.Fixture.Create<Issue>();

            var csvLines = GetCsvLines(csvSink.Serialize(new List<Issue> { issue }));

            csvLines[1].Should().Be(issue.ToString());
        }

        private static CsvSink GetCsvSink()
        {
            return new CsvSink(DummyFilePath);
        }

        private static List<string> GetCsvLines(string csvText)
        {
            return new List<string>(csvText.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }
    }
}