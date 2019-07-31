using System;
using System.Collections.Generic;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Builders;
using PluginExecutionContextBuilder = DLaB.Xrm.Test.Builders.PluginExecutionContextBuilder;

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
