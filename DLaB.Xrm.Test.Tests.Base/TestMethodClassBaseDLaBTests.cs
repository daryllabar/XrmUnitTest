using System;
using System.Collections.Generic;
using DLaB.Common;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Ioc;
using DLaB.Xrm.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Assumptions;
using XrmUnitTest.Test.Builders;
#if NET
using DataverseUnitTest;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class TestMethodClassBaseDLaBTests
    {
        #region GivenEntityIds_Should_CleanupTestsBeforeTestRun

        [TestMethod]
        public void TestMethodClassBaseDLaB_GivenEntityIds_Should_CleanupTestsBeforeTestRun()
        {
            new GivenEntityIds_Should_CleanupTestsBeforeTestRun().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class GivenEntityIds_Should_CleanupTestsBeforeTestRun : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id Contact = new Id<Contact>("8072dbf6-f966-43ec-83af-5b55b067be80");
            }

            protected override void CleanupDataPreInitialization(IOrganizationService service)
            {
                service.Create(Ids.Contact.Entity);
                base.CleanupDataPreInitialization(service);
            }

            protected override void Test(IOrganizationService service)
            {
                // Test happens CleanupTestData 
                AssertCrm.NotExists(Ids.Contact, "Contact Should have been deleted up by the CleanupDataPreInitialization");
            }
        }

        #endregion GivenEntityIds_Should_CleanupTestsBeforeTestRun

        #region TestService_Should_ForceIdsToBeDefined

        [TestMethod]
        public void TestMethodClassBaseDLaB_TestService_Should_ForceIdsToBeDefined()
        {
            new TestService_Should_ForceIdsToBeDefined().Test();
        }

        private class TestService_Should_ForceIdsToBeDefined : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact();

                //
                // Act
                //
                try
                {
                    service.Create(contact);
                }
                catch (AssertFailedException ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type contact without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate."), "Test Service should have had AssertIdNonEmptyOnCreate.");
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");
            }
        }

        #endregion TestService_Should_ForceIdsToBeDefined

        #region ExecuteMultiple_Should_ForceIdsToBeDefined

        [TestMethod]
        public void TestService_ExecuteMultiple_Should_ForceIdsToBeDefined()
        {
            new ExecuteMultiple_Should_ForceIdsToBeDefined().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class ExecuteMultiple_Should_ForceIdsToBeDefined : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact();

                //
                // Act
                //
                try
                {
                    service.Execute(new ExecuteMultipleRequest
                    {
                        Settings = new ExecuteMultipleSettings(),
                        Requests = new OrganizationRequestCollection { new UpsertRequest { Target = contact } }
                    });
                }
                catch (AssertFailedException ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type contact without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate."), "Test Service should have had AssertIdNonEmptyOnCreate.");
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");
            }
        }

        #endregion ExecuteMultiple_Should_ForceIdsToBeDefined

        #region ExecuteMultiple_Should_UseDefinedIds

        [TestMethod]
        public void TestService_ExecuteMultiple_Should_UseDefinedIds()
        {
            new ExecuteMultiple_Should_UseDefinedIds().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class ExecuteMultiple_Should_UseDefinedIds : TestMethodClassBase
        {
            private struct Ids
            {
                public struct Contacts
                {
                    public static readonly Id<Contact> A = new Id<Contact>("D19092FA-694E-4AC6-A876-F1842CD11B43");
                    public static readonly Id<Contact> B = new Id<Contact>("934C45FE-577F-4772-B916-2F2E9C8304AA");
                }
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                          .WithIdsDefaultedForCreate(Ids.Contacts.A, Ids.Contacts.B)
                          .Build();

                service.Execute(new ExecuteMultipleRequest
                {
                    Settings = new ExecuteMultipleSettings(),
                    Requests = new OrganizationRequestCollection
                    {
                        new UpsertRequest {Target = new Contact()},
                        new CreateRequest {Target = new Contact()}
                    }
                });

                AssertCrm.Exists(Ids.Contacts.A);
                AssertCrm.Exists(Ids.Contacts.B);
            }
        }

        #endregion ExecuteMultiple_Should_UseDefinedIds

        #region ExecuteTransaction_Should_ForceIdsToBeDefined

        [TestMethod]
        public void TestService_ExecuteTransaction_Should_ForceIdsToBeDefined()
        {
            new ExecuteTransaction_Should_ForceIdsToBeDefined().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class ExecuteTransaction_Should_ForceIdsToBeDefined : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact();

                //
                // Act
                //
                try
                {
                    service.Execute(new ExecuteTransactionRequest
                    {
                        Requests = new OrganizationRequestCollection { new UpsertRequest { Target = contact } }
                    });
                }
                catch (AssertFailedException ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type contact without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate."), "Test Service should have had AssertIdNonEmptyOnCreate.");
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");
            }
        }

        #endregion ExecuteTransaction_Should_ForceIdsToBeDefined

        #region ExecuteTransaction_Should_UseDefinedIds

        [TestMethod]
        public void TestService_ExecuteTransaction_Should_UseDefinedIds()
        {
            new ExecuteTransaction_Should_UseDefinedIds().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class ExecuteTransaction_Should_UseDefinedIds : TestMethodClassBase
        {
            private struct Ids
            {
                public struct Contacts
                {
                    public static readonly Id<Contact> A = new Id<Contact>("48C344C5-4AB5-476D-B562-845A00F533C5");
                    public static readonly Id<Contact> B = new Id<Contact>("762E5722-A5A4-48F0-8FAC-ED4A82D5259B");
                }
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                          .WithIdsDefaultedForCreate(Ids.Contacts.A, Ids.Contacts.B)
                          .Build();

                service.Execute(new ExecuteTransactionRequest
                {
                    Requests = new OrganizationRequestCollection
                    {
                        new UpsertRequest {Target = new Contact()},
                        new CreateRequest {Target = new Contact()}
                    }
                });

                AssertCrm.Exists(Ids.Contacts.A);
                AssertCrm.Exists(Ids.Contacts.B);
            }
        }

        #endregion ExecuteTransaction_Should_UseDefinedIds


        #region Upsert_Should_ForceIdsToBeDefined

        [TestMethod]
        public void TestService_Upsert_Should_ForceIdsToBeDefined()
        {
            new Upsert_Should_ForceIdsToBeDefined().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class Upsert_Should_ForceIdsToBeDefined : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact();

                //
                // Act
                //
                try
                {
                    service.Upsert(contact);
                }
                catch (AssertFailedException ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type contact without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate."), "Test Service should have had AssertIdNonEmptyOnCreate.");
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");
            }
        }

        #endregion Upsert_Should_ForceIdsToBeDefined

        #region Upsert_Should_UseDefinedIds

        [TestMethod]
        public void TestService_Upsert_Should_UseDefinedIds()
        {
            new Upsert_Should_UseDefinedIds().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class Upsert_Should_UseDefinedIds : TestMethodClassBase
        {
            private struct Ids
            {
                public struct Contacts
                {
                    public static readonly Id<Contact> A = new Id<Contact>("FE11D2D4-D2D7-4848-88D6-E72DFB98BAEB");
                    public static readonly Id<Contact> B = new Id<Contact>("E50EAF7D-5FC0-46FC-AAB4-FAD3DFF062AB");
                }
            }

            protected override void Test(IOrganizationService service)
            {
                service = new OrganizationServiceBuilder(service)
                          .WithIdsDefaultedForCreate(Ids.Contacts.A, Ids.Contacts.B)
                          .Build();
                var result = service.Upsert(new Contact());
                Assert.IsTrue(result.RecordCreated);
                Assert.AreEqual(Ids.Contacts.A.EntityId, result.Target.Id);

                var contact = new Contact();
                contact.KeyAttributes.Add(Contact.Fields.FirstName, "MyKey");
                result = service.Upsert(contact);
                Assert.IsTrue(result.RecordCreated);
                Assert.AreEqual(Ids.Contacts.B.EntityId, result.Target.Id);

                result = service.Upsert(contact);
                Assert.IsFalse(result.RecordCreated);
            }
        }

        #endregion Upsert_Should_UseDefinedIds

        #region AssumptionParentFirst_Should_LoadAssumptions

        [TestMethod]
        public void TestMethodClassBaseDLaB_AssumptionParentFirst_Should_LoadAssumptions()
        {
            new AssumptionParentFirst_Should_LoadAssumptions().Test();
            // Test for failure when the same test is ran twice, and then another test uses a cached values.
            new AssumptionParentFirst_Should_LoadAssumptions().Test();
        }

        [AccountDefault]
        [ContactDefault]
        private class AssumptionParentFirst_Should_LoadAssumptions : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                var account = service.GetEntity<Account>(AssumedEntities.Get<AccountDefault>().Id);
                Assert.AreEqual("Default Account", account.Name);
                var contact = service.GetEntity<Contact>(AssumedEntities.Get<ContactDefault>().Id);
                Assert.AreEqual("Default Contact", contact.FirstName + " " + contact.LastName);
            }
        }

        #endregion AssumptionParentFirst_Should_LoadAssumptions

        #region AssumptionChildFirst_Should_LoadAssumptions

        [TestMethod]
        public void TestMethodClassBaseDLaB_AssumptionChildFirst_Should_LoadAssumptions()
        {
            new AssumptionChildFirst_Should_LoadAssumptions().Test();
        }

        [ContactDefault]
        [AccountDefault]
        private class AssumptionChildFirst_Should_LoadAssumptions : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                var account = service.GetEntity<Account>(AssumedEntities.Get<AccountDefault>().Id);
                Assert.AreEqual("Default Account", account.Name);
                var contact = service.GetEntity<Contact>(AssumedEntities.Get<ContactDefault>().Id);
                Assert.AreEqual("Default Contact", contact.FirstName + " " + contact.LastName);
            }
        }

#endregion AssumptionChildFirst_Should_LoadAssumptions

        #region UnnamedAssociation_Should_BeCreated

        [TestMethod]
        public void TestMethodClassBaseDLaB_UnnamedAssociation_Should_BeCreated()
        {
            new UnnamedAssociation_Should_BeCreated().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class UnnamedAssociation_Should_BeCreated : TestMethodClassBase
        {

            protected override void Test(IOrganizationService service)
            {
                    var id1 = service.Create(new ConnectionRole{Id = Guid.NewGuid()});
                    var id2 = service.Create(new ConnectionRole{Id = Guid.NewGuid()});
                    service.Associate(ConnectionRole.EntityLogicalName,
                        id1,
                        new Relationship("connectionroleassociation_association"),
                        new EntityReferenceCollection(new List<EntityReference> { new EntityReference(ConnectionRole.EntityLogicalName, id2) })
                    );
                    var m2m = service.GetFirstOrDefault<ConnectionRoleAssociation>();
                    Assert.IsNotNull(m2m);
                    Assert.AreEqual(id1, m2m.ConnectionRoleId.GetValueOrDefault());
                    Assert.AreEqual(id2, m2m.AssociatedConnectionRoleId.GetValueOrDefault());

            }
        }

#endregion UnnamedAssociation_Should_BeCreated

        #region CustomAction_Should_InitPlugin

        [TestMethod]
        public void CustomAction_Should_InitPlugin()
        {
            new CustomAction_Should_InitPluginClass().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CustomAction_Should_InitPluginClass : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {

                //
                // Arrange
                //
                var plugin = new InitPluginTester(null, null);
                var context = new PluginExecutionContextBuilder().WithFirstRegisteredEvent(plugin).Build();
                var serviceProvider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(serviceProvider);

                //
                // Assert
                //
                
            }
        }

        public class InitPluginTester: DLaBPluginBase
        {
            public MessageType MyCustomAction = new MessageType("MyCustomAction");
            public InitPluginTester(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
            {
            }

            protected override void ExecuteInternal(IExtendedPluginContext context)
            {
                // nothing
            }

            protected override IEnumerable<RegisteredEvent> CreateEvents()
            {
                return new RegisteredEventBuilder(PipelineStage.PreOperation, MyCustomAction)
                       .And(PipelineStage.PostOperation, MessageType.Update, MessageType.Create)
                       .ForEntities<Contact>().Build();
            }
        }

#endregion CustomAction_Should_InitPlugin
    }
}
