using System;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Tests
{
    [TestClass]
    public class ExtensionsIPluginExecutionContextTests
    {
        [TestMethod]
        public void Extensions_IPluginExecutionContext_GetTarget()
        {
            // Currently I believe this to be a issue with CRM, where the Parent Plugin Context contain the actual Target, for the post operation.
            // https://social.microsoft.com/Forums/en-US/19435dde-31ad-419d-826c-3f4e89ce6370/when-does-the-parent-plugin-context-contain-the-actual-target?forum=crm
            //
            // Update Finally understood, changings this to return what it should
            // Arrange
            //
            var target = new Lead
            {
                Id = Guid.NewGuid(),
                StateCode = LeadState.Open,
                StatusCodeEnum = Lead_StatusCode.CannotContact,
                FirstName = "Test"
            };

            var parentContext = new PluginExecutionContextBuilder().
                WithTarget(target).Build();
            var pluginContext = new PluginExecutionContextBuilder().
                WithParentContext(parentContext).WithTarget(new Lead
                {
                    Id = target.Id,
                    StateCode = LeadState.Open,
                    StatusCodeEnum = Lead_StatusCode.CannotContact,
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = new EntityReference(SystemUser.EntityLogicalName, Guid.NewGuid()),
                    ModifiedOnBehalfBy = new EntityReference(SystemUser.EntityLogicalName, Guid.NewGuid())
                }).
                WithRegisteredEvent(new RegisteredEvent(PipelineStage.PostOperation, MessageType.Update)).Build();

            //
            // Act
            //
            var pluginTarget = pluginContext.GetTarget<Lead>();

            //
            // Assert
            //
            Assert.AreNotEqual(target.FirstName, pluginTarget.FirstName);

        }
    }
}
