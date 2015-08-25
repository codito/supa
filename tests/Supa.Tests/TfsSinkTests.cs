namespace Supa.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Supa.Platform;
    using Supa.Platform.TestDoubles;

    [TestClass]
    public class TfsSinkTests
    {
        private readonly TfsServiceProviderSimulator tfsServiceSimulator;

        private readonly int parentWorkItemId;

        private TestableTfsSink testableTfsSink;

        public TfsSinkTests()
        {
            this.tfsServiceSimulator = new TfsServiceProviderSimulator(new Uri("http://dummyUri"));
            this.parentWorkItemId = this.tfsServiceSimulator.CreateWorkItem("Parent Work Item");
        }

        [TestInitialize]
        public void InitializeTest()
        {
            this.testableTfsSink = new TestableTfsSink(
                this.tfsServiceSimulator,
                new NetworkCredential(),
                this.parentWorkItemId,
                "Task",
                new Dictionary<string, object>());
        }

        [TestMethod]
        public void TfsSinkShouldThrowIfTfsUrlIsNullOrEmpty()
        {
            Action createTfsSink = () => new TfsSink(null, new NetworkCredential(), 0, "Task", new Dictionary<string, object>());

            createTfsSink.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("serviceUri");
        }

        [TestMethod]
        public void TfsSinkShouldThrowIfCredentialIsNull()
        {
            Action action = () => new TfsSink(new Uri("http://dummyUri"), null, 0, "Task", new Dictionary<string, object>());

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("credential");
        }

        [TestMethod]
        public void TfsSinkShouldThrowIfFieldMapIsNull()
        {
            Action action = () => new TfsSink(new Uri("http://dummyUri"), new NetworkCredential(), 0, "Task", null);

            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("fieldMap");
        }

        [TestMethod]
        public void TfsSinkConfigureShouldConfigureTheTfsServiceProvider()
        {
            Action action = () => this.testableTfsSink.Configure();

            action.ShouldNotThrow();
        }

        [TestMethod]
        public void TfsSinkConfigureShouldThrowIfParentWorkItemDoesNotExist()
        {
            var tfsSink = new TestableTfsSink(
                this.tfsServiceSimulator,
                new NetworkCredential(),
                -205,
                "Task",
                new Dictionary<string, object>());
            Action action = () => tfsSink.Configure();

            action.ShouldThrow<Exception>();
        }

        [TestMethod]
        public void TfsSinkUpdateWorkItemShouldNotUpdateWorkItemIfIssueAlreadyExists()
        {
            this.testableTfsSink.Configure();
            var workItem = this.tfsServiceSimulator.CreateWorkItem("Workitem: Issue 1");
            this.tfsServiceSimulator.AddLinkToWorkItem(this.parentWorkItemId, workItem, "issue1:1");

            var issue = new Issue { Id = "issue1", Activity = 1 };

            this.testableTfsSink.UpdateWorkItem(issue).Should().BeFalse();
        }

        [TestMethod]
        public void TfsSinkUpdateWorkItemShouldUpdateFieldsIfWorkItemForIssueDoesNotExist()
        {
            var fieldMap = new Dictionary<string, object>
                               {
                                   { "Title", "{{Topic}}" },
                                   { "Description", "{{Description}}" },
                                   { "IdField", "{{Id}}" },
                                   { "ActivityCount", "{{Activity}}" }
                               };
            var tfsSink = new TestableTfsSink(this.tfsServiceSimulator, new NetworkCredential(), this.parentWorkItemId, "Task", fieldMap);
            var issue = new Issue { Id = "issue2", Topic = "issue1Topic", Activity = 1, Description = "descr" };
            tfsSink.Configure();

            tfsSink.UpdateWorkItem(issue).Should().BeTrue();

            var workItem = this.tfsServiceSimulator.GetWorkItemForIssue("issue2", 1).Item as TfsServiceProviderSimulator.InMemoryWorkItem;
            Assert.IsNotNull(workItem);
            workItem["Title"].Should().Be("issue1Topic");
            workItem["Description"].Should().Be("descr");
            workItem["IdField"].Should().Be("issue2");
            workItem["ActivityCount"].Should().Be("1");
        }

        [TestMethod]
        public void TfsSinkUpdateWorkItemShouldUpdateFieldsEvenIfWorkItemTypeIsEmpty()
        {
            var fieldMap = new Dictionary<string, object>
                               {
                                   { "Title", "{{Topic}}" },
                                   { "Description", "{{Description}}" },
                                   { "IdField", "{{Id}}" },
                                   { "ActivityCount", "{{Activity}}" }
                               };
            var tfsSink = new TestableTfsSink(this.tfsServiceSimulator, new NetworkCredential(), this.parentWorkItemId, "", fieldMap);
            var issue = new Issue { Id = "issue2", Topic = "issue1Topic", Activity = 1, Description = "descr" };
            tfsSink.Configure();

            tfsSink.UpdateWorkItem(issue).Should().BeTrue();

            var workItem = this.tfsServiceSimulator.GetWorkItemForIssue("issue2", 1).Item as TfsServiceProviderSimulator.InMemoryWorkItem;
            Assert.IsNotNull(workItem);
            workItem["Title"].Should().Be("issue1Topic");
            workItem["Description"].Should().Be("descr");
            workItem["IdField"].Should().Be("issue2");
            workItem["ActivityCount"].Should().Be("1");
        }

        [TestMethod]
        public void TfsSinkUpdateWorkItemShouldUpdateFieldsEvenIfWorkItemTypeIsNull()
        {
            var fieldMap = new Dictionary<string, object>
                               {
                                   { "Title", "{{Topic}}" },
                                   { "Description", "{{Description}}" },
                                   { "IdField", "{{Id}}" },
                                   { "ActivityCount", "{{Activity}}" }
                               };
            var tfsSink = new TestableTfsSink(this.tfsServiceSimulator, new NetworkCredential(), this.parentWorkItemId, null, fieldMap);
            var issue = new Issue { Id = "issue2", Topic = "issue1Topic", Activity = 1, Description = "descr" };
            tfsSink.Configure();

            tfsSink.UpdateWorkItem(issue).Should().BeTrue();

            var workItem = this.tfsServiceSimulator.GetWorkItemForIssue("issue2", 1).Item as TfsServiceProviderSimulator.InMemoryWorkItem;
            Assert.IsNotNull(workItem);
            workItem["Title"].Should().Be("issue1Topic");
            workItem["Description"].Should().Be("descr");
            workItem["IdField"].Should().Be("issue2");
            workItem["ActivityCount"].Should().Be("1");
        }

        [TestMethod]
        public void TfsSinkUpdateWorkItemShouldUpdateFieldsIfTemplateTextDoesNotMatch()
        {
            var fieldMap = new Dictionary<string, object>
                               {
                                   { "Title", "Dummy Title" }, // Title is a mandatory field for simulator
                                   { "Foo", "{{Topic1}}" },
                                   { "Description", "{{deScriptIon}}" }
                               };
            var tfsSink = new TestableTfsSink(this.tfsServiceSimulator, new NetworkCredential(), this.parentWorkItemId, "Task", fieldMap);
            var issue = new Issue { Id = "issue2", Topic = "issue1Topic", Activity = 1 };
            tfsSink.Configure();

            tfsSink.UpdateWorkItem(issue).Should().BeTrue();

            var workItem = this.tfsServiceSimulator.GetWorkItemForIssue("issue2", 1).Item as TfsServiceProviderSimulator.InMemoryWorkItem;
            Assert.IsNotNull(workItem);
            workItem["Title"].Should().Be("Dummy Title");
            workItem["Foo"].Should().Be("{{Topic1}}");
            workItem["Description"].Should().Be("{{deScriptIon}}");
        }
    }

    public class TestableTfsSink : TfsSink
    {
        public TestableTfsSink(ITfsServiceProvider serviceProvider, NetworkCredential credential, int parentWorkItemId, string workItemType, Dictionary<string, object> fieldMap)
            : base(serviceProvider, credential, parentWorkItemId, workItemType, fieldMap)
        {
        }
    }
}