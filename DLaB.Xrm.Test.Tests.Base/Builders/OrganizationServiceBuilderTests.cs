using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmUnitTest.Test;
using Microsoft.Xrm.Sdk.Query;

#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class OrganizationServiceBuilderTests
    {
        [TestInitialize]
        public void InitializeTestSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

        [TestMethod]
        public void OrganizationServiceBuilder_IsReadonly_Should_NotAllowCUD()
        {

            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service).IsReadOnly().Build();
            void AssertFailureForRequest(string requestName, string expectedError, Action action)
            {
                var didNotFailMessage = requestName + " Should Fail!";
                try
                {
                    action();
                    Assert.Fail(didNotFailMessage);
                }
                catch (Exception ex)
                {
                    if (ex.Message == didNotFailMessage)
                    {
                        throw;
                    }

                    Assert.IsTrue(ex.Message.Contains(expectedError), ex.Message);
                }
            }

            var contact = new Contact();
            AssertFailureForRequest("Associate", "An attempt was made to Associate Entities to Entity contact (00000000-0000-0000-0000-000000000000) for relationship r1 with a ReadOnly Service", () => { service.Associate(contact, "r1", contact); });
            AssertFailureForRequest("Create", "An attempt was made to Create a(n) contact Entity with a ReadOnly Service", () => { service.Create(contact); });
            AssertFailureForRequest("Delete", "An attempt was made to Delete a(n) contact Entity with id 00000000-0000-0000-0000-000000000000, using a ReadOnly Service", () => { service.Delete(contact); });
            AssertFailureForRequest("Disassociate", "An attempt was made to Disassociate Entities to Entity contact (00000000-0000-0000-0000-000000000000) for relationship r1 with a ReadOnly Service", () => { service.Disassociate(contact, "r1", contact); });
            AssertFailureForRequest("Update", "An attempt was made to Update a(n) contact Entity with id 00000000-0000-0000-0000-000000000000, using a ReadOnly Service", () => { service.Update(contact); });
        }

        [TestMethod]
        public void OrganizationServiceBuilder_IsReadonly_Should_NotAllowCudInExecuteMultiple()
        {

            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service).IsReadOnly().Build();

            void AssertFailureForSingleRequestAsMultiple(OrganizationRequest request, string expectedError)
            {
                var didNotFailMessage = request.RequestName + " Should Fail!";
                try
                {
                    service.ExecuteMultiple(new OrganizationRequestCollection { request });
                    Assert.Fail(didNotFailMessage);
                }
                catch(Exception ex)
                {
                    if (ex.Message == didNotFailMessage)
                    {
                        throw;
                    }
                    if (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                    Assert.IsTrue(ex.Message.Contains(expectedError), ex.Message);
                }
            }

            AssertFailureForSingleRequestAsMultiple(new CreateRequest { Target = new Contact() }, "An attempt was made to Create a(n) contact Entity with a ReadOnly Service");
            AssertFailureForSingleRequestAsMultiple(new DeleteRequest { Target = new Contact().ToEntityReference() }, "An attempt was made to Delete a(n) contact Entity with id 00000000-0000-0000-0000-000000000000, using a ReadOnly Service");
            AssertFailureForSingleRequestAsMultiple(new UpdateRequest { Target = new Contact() }, "An attempt was made to Update a(n) contact Entity with id 00000000-0000-0000-0000-000000000000, using a ReadOnly Service");
            AssertFailureForSingleRequestAsMultiple(new AssociateRequest{ Target = new Contact().ToEntityReference(), Relationship = new Relationship{ SchemaName = "Schema"} }, "An attempt was made to Associate Entities to Entity contact (00000000-0000-0000-0000-000000000000) for relationship Schema with a ReadOnly Service");
            AssertFailureForSingleRequestAsMultiple(new DisassociateRequest { Target = new Contact().ToEntityReference(), Relationship = new Relationship { SchemaName = "Schema" } }, "An attempt was made to Disassociate Entities to Entity contact (00000000-0000-0000-0000-000000000000) for relationship Schema with a ReadOnly Service");
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithEntityNameDefaulted_Name_Should_BeDefaulted()
        {
            //
            // Arrange
            //
            const string notExistsEntityLogicalName = "notexists";
            const string customEntityLogicalName = "custom_entity";
            var entities = new List<Entity>();

            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                 .WithFakeCreate((s, e) =>
                 {
                     // Don't create fake entities
                     if (e.LogicalName == notExistsEntityLogicalName
                         || e.LogicalName == customEntityLogicalName)
                     {

                         entities.Add(e);
                         return Guid.NewGuid();
                     }

                     return s.Create(e);
                 })
                 .WithEntityNameDefaulted((e, info) => GetName(e.LogicalName, info.AttributeName), new NoCustomNames()).Build();


            //
            // Act
            //
            service.Create(new Entity(notExistsEntityLogicalName));
            service.Create(new Entity(customEntityLogicalName));
            service.Create(new Contact());
            service.Create(new Account());


            //
            // Assert
            //
            Assert.AreEqual(0, entities.Single(e => e.LogicalName == notExistsEntityLogicalName).Attributes.Count);
            Assert.AreEqual(GetName(customEntityLogicalName,"custom_name"), entities.Single(e => e.LogicalName == customEntityLogicalName)["custom_name"]);
            Assert.AreEqual(GetName(Contact.EntityLogicalName, " " + Contact.Fields.FullName), service.GetFirst<Contact>().FullName);
            Assert.AreEqual(GetName(Account.EntityLogicalName, Account.Fields.Name), service.GetFirst<Account>().Name);
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithExecute_FakedRetrieveMetadataChanges_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                .WithFakeExecute((s, r) =>
                {
                    if (r.RequestName == "RetrieveMetadataChanges")
                    {
                        return new RetrieveMetadataChangesResponse
                        {
                            Results = new ParameterCollection
                            {
                                { 
                                    nameof(RetrieveMetadataChangesResponse.EntityMetadata),
                                    new EntityMetadataCollection {
                                        new() { LogicalName = Account.EntityLogicalName }
                                    }
                                }
                            }
                        };
                    }

                    // Perform Normal Execute
                    return s.Execute(r);
                }).Build();

            var response = (RetrieveMetadataChangesResponse)service.Execute(new RetrieveMetadataChangesRequest());
            Assert.AreEqual(Account.EntityLogicalName, response.EntityMetadata[0].LogicalName);
        }

        private class NoCustomNames : IEntityHelperConfig
        {
            public string GetIrregularIdAttributeName(string logicalName)
            {
                return null;
            }

            public PrimaryFieldInfo GetIrregularPrimaryFieldInfo(string logicalName, PrimaryFieldInfo defaultInfo = null)
            {
                return null;
            }
        }

        private string GetName(string entityLogicalName, string attributeName)
        {
            return entityLogicalName + "|" + attributeName;
        }

        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void OrganizationServiceBuilder_WithIdsDefaultedForCreate_Ids_Should_BeDefaulted()
        {
            //
            // Arrange
            //
            var ids = new
            {
                Account = new
                {
                    A = new Id<Account>("E2D24D5D-428F-4FBC-AA8D-5235DC27651C"),
                    B = new Id<Account>("D901F79B-2730-47BE-821F-3485A4CA020D")
                },
                Contact = new
                {
                    A = new Id<Contact>("02C430B9-B5CC-413F-B697-1C813F194547"),
                    B = new Id<Contact>("95D9BF9A-C603-4D5C-A078-7B01A4C47BA2")
                }
            };

            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                    .WithIdsDefaultedForCreate(
                        ids.Account.A, 
                        ids.Account.B,
                        ids.Contact.A,
                        ids.Contact.B).Build();

            Assert.AreEqual(ids.Account.A.EntityId, service.Create(new Account()));
            Assert.AreEqual(ids.Contact.A.EntityId, service.Create(new Contact()));
            using (var context = new CrmContext(service))
            {
                var account = new Account{ Id = ids.Account.B};
                context.AddObject(account);
                context.SaveChanges();
                AssertCrm.Exists(service, ids.Account.B);
                var contact = new Contact();
                context.AddObject(contact);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    var inner = ex.InnerException;
                    Assert.AreEqual("An attempt was made to create an entity of type contact with the EntityState set to created which normally means it comes from an OrganizationServiceContext.SaveChanges call.\r\nEither set ignoreContextCreation to true on the WithIdsDefaultedForCreate call, or define the id before calling SaveChanges, and add the id with the WithIdsDefaultedForCreate method.", inner?.Message);
                }
            }
        }

        [TestMethod]
        [DataRow(true, false, DisplayName = "Primary Allowed but hierarchy update not, should not utilize default ids")]
        [DataRow(true, true, DisplayName = "Primary Allowed, should utilize default ids")]
        [DataRow(false, false, DisplayName = "Primary Not Allowed and hierarchy update not, should not utilize default ids")]
        [DataRow(false, true, DisplayName = "Primary Not Allowed, should not utilize default ids")]
        public void OrganizationServiceBuilder_WithIdsDefaultedForCreate(bool usePrimaryBuilderForNewEntityDefaultIds, bool allowRearrangeViaInsert)
        {
            var account = new Id<Account>("74FAF332-2BDC-4A16-87F8-51E26D03ECC7");

            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            // Lower level fake is defined, that should utilize the Ids defaulted logic.
            service = new OrganizationServiceBuilder(service)
                .WithFakeRetrieveMultiple((s,q) => {
                    s.Create(new Account());
                    return s.RetrieveMultiple(q);
                }).Build();

            ((FakeIOrganizationService)service).AllowRearrangeViaInsert = allowRearrangeViaInsert;
            
            service = new OrganizationServiceBuilder(service)
                .WithIdsDefaultedForCreate(account).Build(new OrganizationServiceBuilderBuildConfig
                {
                    UsePrimaryBuilderForNewEntityDefaultIds = usePrimaryBuilderForNewEntityDefaultIds
                });

            var accounts = service.RetrieveMultiple(new QueryExpression(Account.EntityLogicalName));
            if (usePrimaryBuilderForNewEntityDefaultIds && allowRearrangeViaInsert)
            {
                Assert.AreEqual(account.EntityId, accounts[0].Id);
            }
            else
            {
                Assert.AreNotEqual(account.EntityId, accounts[0].Id);
            }
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithFakeAction_FakedBookingStatusRequest_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                    .WithFakeAction(new msdyn_UpdateBookingsStatusResponse
                    {
                        BookingResult = "Success"
                    }).Build();

            var response = (msdyn_UpdateBookingsStatusResponse)service.Execute(new msdyn_UpdateBookingsStatusRequest());

            Assert.AreEqual("Success", response.BookingResult);
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithFakeAction_FakedConditionalBookingStatusRequest_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                    .WithFakeAction<msdyn_UpdateBookingsStatusRequest, msdyn_UpdateBookingsStatusResponse>((_, r) =>
                    {
                        return new msdyn_UpdateBookingsStatusResponse
                        {
                            BookingResult = r.BookingStatusChangeContext == "A"
                                ? "Apple" 
                                : "Banana"
                        };
                    }).Build();

            var response = (msdyn_UpdateBookingsStatusResponse)service.Execute(new msdyn_UpdateBookingsStatusRequest { BookingStatusChangeContext = "A"});
            Assert.AreEqual("Apple", response.BookingResult);

            response = (msdyn_UpdateBookingsStatusResponse)service.Execute(new msdyn_UpdateBookingsStatusRequest { BookingStatusChangeContext = "B" });
            Assert.AreEqual("Banana", response.BookingResult);
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithFakeRetrieve_FakedRetrieves_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = service.Create(new Account());
            service = new OrganizationServiceBuilder(service).WithFakeRetrieve(new Account {Name = "TEST"}).Build();
            var account = service.GetEntity<Account>(id);
            Assert.AreEqual("TEST", account.Name);
            Assert.AreEqual(Guid.Empty, account.Id);

            service = new OrganizationServiceBuilder(service).WithFakeRetrieve((_,_,i,_) => i == id, new Account{Name = "TEST2" }).Build();
            account = service.GetEntity<Account>(id);
            Assert.AreEqual("TEST2", account.Name);
            Assert.AreEqual(Guid.Empty, account.Id);
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithFakeRetrieveMultiple_FakedRetrieves_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service).WithFakeRetrieveMultiple((_,_)=> true, GetFakedAccount()).Build();
            AssertAccountNotQueried(service);
        }

        [TestMethod]
        public void OrganizationServiceBuilder_WithFakeRetrieveMultipleForEntity_FakedRetrieves_Should_BeFaked()
        {
            IOrganizationService service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var accounts = new List<Account> {GetFakedAccount()};
            AssertAccountNotQueried(new OrganizationServiceBuilder(service).WithFakeRetrieveMultipleForEntity(Account.EntityLogicalName, new EntityCollection(accounts.Cast<Entity>().ToArray())).Build());
            AssertAccountNotQueried(new OrganizationServiceBuilder(service).WithFakeRetrieveMultipleForEntity(accounts).Build());
            AssertAccountNotQueried(new OrganizationServiceBuilder(service).WithFakeRetrieveMultipleForEntity(accounts.First()).Build());
        }

        private Account GetFakedAccount()
        {
            return new Account {Name = "TEST"};
        }

        private void AssertAccountNotQueried(IOrganizationService service)
        {
            service.Create(new Account());
            var account = service.GetFirst<Account>();
            Assert.AreEqual("TEST", account.Name);
            Assert.AreEqual(Guid.Empty, account.Id);
        }
    }
}