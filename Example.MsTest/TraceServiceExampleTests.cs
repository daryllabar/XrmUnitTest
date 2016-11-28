using System.Linq;
using DLaB.Xrm.Entities;
using Example.MsTestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Example.MsTestBase.Builders;
using Example.Plugin.Advanced;

namespace Example.MsTest
{
    [TestClass]
    public class TraceServiceExampleTests
    {

        #region TraceMessageCreated_Should_BeAccessible

        [TestMethod]
        public void TraceServiceExample_TraceMessageCreated_Should_BeAccessible()
        {
            new TraceMessageCreated_Should_BeAccessible().Test();
        }

        private class TraceMessageCreated_Should_BeAccessible : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var plugin = new SyncContactToAccount();
                var context = new PluginExecutionContextBuilder()
                    .WithTarget(new Contact())
                    .WithFirstRegisteredEvent(plugin)
                    .Build();
                var provider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(provider);

                //
                // Assert
                //
                Assert.IsTrue(Logs.Any(l => l.Trace == SyncContactToAccount.AddressNotUpdatedMessage));

            }
        }

        #endregion TraceMessageCreated_Should_BeAccessible

    }
}
