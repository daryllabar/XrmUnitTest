using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
using Example.MsTestBase;
using Example.Plugin;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

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

        [TestMethod]
        public void CreateGuidActivity_Request_Should_CreateGuid_FakeXrmEasy()
        {
            //
            // Arrange
            //
            var fakedContext = new XrmFakedContext();
            //
            // Act
            //
            var results = fakedContext.ExecuteCodeActivity<CreateGuidActivity>(new Dictionary<string, object>());
            //
            // Assert
            //
            Guid tmp;
            Assert.IsTrue(Guid.TryParse(results.First().Value.ToString(), out tmp));
        }
    }
}
