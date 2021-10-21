using System;
using System.Collections.Generic;
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
            var b = new PluginExecutionContextBuilder().WithFirstRegisteredEvent((IPlugin)new HiddenPlugin());
        }

        private class HiddenPlugin : IRegisteredEventsPlugin
        {
            public void Execute(IServiceProvider serviceProvider)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<RegisteredEvent> RegisteredEvents => new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create).Build();
        }
    }
}
