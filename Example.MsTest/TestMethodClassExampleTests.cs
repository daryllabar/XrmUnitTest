using System;
using System.Linq;
using DLaB.Xrm.Entities;
using Example.MsTestBase;
using Example.MsTestBase.Builders;
using Example.Plugin.Advanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm.Test;

namespace Example.MsTest
{
    [TestClass]
    public class TestMethodClassExampleTests
    {
        #region CreationOfEntity_Should_DefineName

        [TestMethod]
        public void TestMethodClassExample_CreationOfEntity_Should_DefineName()
        {
            new CreationOfEntity_Should_DefineName().Test();
        }

        private class CreationOfEntity_Should_DefineName : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("A8C3DD73-4796-4755-8C7D-186F1A3BE6A5");
                public static readonly Id<Lead> Lead = new Id<Lead>("08ABE111-D02D-46C7-B6FA-1FD0177CDAA4");
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange / Act
                //
                service.Create(Ids.Account);
                service.Create(Ids.Lead);

                //
                // Assert
                //
                Assert.IsNotNull(service.GetEntity(Ids.Account).Name, "Name should have been defaulted.");
                Assert.IsNotNull(service.GetEntity(Ids.Lead).FullName, "Full Name should have been defaulted.");

            }
        }

        #endregion CreationOfEntity_Should_DefineName

        #region CreationOfEntity_Should_RequireIdDefined

        [TestMethod]
        public void TestMethodClassExample_CreationOfEntity_Should_RequireIdDefined()
        {
            new CreationOfEntity_Should_RequireIdDefined().Test();
        }

        private class CreationOfEntity_Should_RequireIdDefined : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("C3F2F4E3-91A5-4555-A983-A9533212CD74");
                public static readonly Id<Lead> Lead = new Id<Lead>("83F924AA-D3D5-4953-9453-631221DE7581");
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                service = new OrganizationServiceBuilder(service)
                    .WithIdsDefaultedForCreate(Ids.Lead)
                    .Build();

                //
                // Act
                //
                service.Create(Ids.Account); // Id is populated, This is OK
                service.Create(new Lead());  // No Id populated, but WithIdsDefaultedForCreate, lists it as a value
                try
                {
                    service.Create(new Lead()); // Not valid since only one Lead Id has been pre-defined.
                }
                catch (Exception ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type lead, but no id exists in the NewEntityDefaultIds Collection for it.\r\nEither the entity's Id was not populated as a part of initialization, or a call is needs to be added to to OrganizationServiceBuilder.WithIdsDefaultedForCreate(id)"));
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");

            }
        }

        #endregion CreationOfEntity_Should_RequireIdDefined

        #region TraceMessageCreated_Should_BeAccessible

        [TestMethod]
        public void TestMethodClassExample_TraceMessageCreated_Should_BeAccessible()
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
