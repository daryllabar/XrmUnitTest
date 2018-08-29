using System;
using System.Linq;
using DLaB.Xrm.Test.Builders;
using Xyz.Xrm.Test;
using Xyz.Xrm.Plugin.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xyz.Xrm.Plugin.Tests
{
    /// <summary>
    /// Workflow Example.  Utilizes the WorkflowInvokerBuilder to Invoke the workflow
    /// </summary>
    [TestClass]
    public class WorkflowActivityExampleTests
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
            Assert.IsTrue(Guid.TryParse(results.First().Value.ToString(), out Guid tmp));
        }
    }
}