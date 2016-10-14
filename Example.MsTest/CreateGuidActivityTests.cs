using System;
using System.Linq;
using DLaB.Xrm.Test.Builders;
using Example.MsTestBase;
using Example.Plugin.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Example.MsTest
{
    [TestClass]
    public class CreateGuidActivityTests
    {
        [TestMethod]
        public void CreateGuidActivity_Request_Should_CreateGuid()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var workflow = new CreateGuidActivity();

            //
            // Act
            //
            var results = new WorkflowInvokerBuilder(workflow).
                // WithService(service).
                InvokeWorkflow();
            //
            // Assert
            //
            Guid tmp;
            Assert.IsTrue(Guid.TryParse(results.First().Value.ToString(), out tmp));
        }
    }
}