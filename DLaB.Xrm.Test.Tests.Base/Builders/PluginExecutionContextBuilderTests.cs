using System;
using System.Collections.Generic;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
using PluginExecutionContextBuilder = DataverseUnitTest.Builders.PluginExecutionContextBuilder;
#else
using PluginExecutionContextBuilder = DLaB.Xrm.Test.Builders.PluginExecutionContextBuilder;
#endif

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class PluginExecutionContextBuilderTests
    {
        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void PluginExecutionContextBuilder_WithFirstRegisteredEvent_PrivatePlugin_Should_BeAccessible()
        {
            var context = new PluginExecutionContextBuilder().WithFirstRegisteredEvent((IPlugin)new HiddenPlugin()).Build();
            Assert.AreEqual(PipelineStage.PreOperation, (PipelineStage)context.Stage);
            Assert.AreEqual(MessageType.Create, context.GetMessageType());
        }

        private class HiddenPlugin : IRegisteredEventsPlugin
        {
            public void Execute(IServiceProvider serviceProvider)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<RegisteredEvent> RegisteredEvents => new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create).Build();
        }

        [TestMethod]
        public void PluginExecutionContextBuilder_WithRegisteredEvent_PreValidation_Should_NotBeInTransaction()
        {
            var context = new PluginExecutionContextBuilder().WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreValidation, MessageType.Create)).Build();
            Assert.IsFalse(context.IsInTransaction);
        }

        [TestMethod]
        public void PluginExecutionContextBuilder_WithRegisteredEvent_PreOperation_Should_BeInTransaction()
        {
            var context = new PluginExecutionContextBuilder().WithRegisteredEvent(new RegisteredEvent(PipelineStage.PreOperation, MessageType.Create)).Build();
            Assert.IsTrue(context.IsInTransaction);
        }

        [TestMethod]
        public void PluginExecutionContextBuilder_WithRegisteredEvent_PostOperation_Should_BeInTransactionOnlyWhenSync()
        {
            var context = new PluginExecutionContextBuilder()
                .WithRegisteredEvent(new RegisteredEvent(PipelineStage.PostOperation, MessageType.Create))
                .WithMode((int)SdkMessageProcessingStep_Mode.Synchronous);
            Assert.IsTrue(context.Build().IsInTransaction);

            context.WithMode((int)SdkMessageProcessingStep_Mode.Asynchronous);
            Assert.IsFalse(context.Build().IsInTransaction);
        }
    }
}
