using System;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Builders;
#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class CrmEnvironmentBuilderTests
    {
        [TestInitialize]
        public void InitializeTestSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void CrmEnvironmentBuilder_WithChildEntities_IncidentAndAccount_Should_CreateAccountFirst()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var account = new Id<Account>(Guid.NewGuid());
            var incident = new Id<Incident>(Guid.NewGuid());
            var lead = new Id<Lead>(Guid.NewGuid());

            // The Account and Incident will be added as Account first, and Incident second. 
            // The Lead will force a reorder and the Account incident would normally get placed after the Incident
            var builder = new DLaBCrmEnvironmentBuilder().
                          WithChildEntities(account, incident).
                          WithEntities(lead);

            //
            // Act
            //
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, incident);
            Assert.AreEqual(account.EntityReference, service.GetEntity(incident).CustomerId);
            Assert.AreEqual(account.EntityReference, incident.Entity.CustomerId);
        }

        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void CrmEnvironmentBuilder_WithChildEntities_BiDirectionalRelationship_Should_PopulateBothIds()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var contact = new Id<Contact>(Guid.NewGuid());
            var account = new Id<Account>(Guid.NewGuid());

            // The Account and Incident will be added as Account first, and Incident second. 
            // The Lead will force a reorder and the Account incident would normally get placed after the Incident
            var builder = new DLaBCrmEnvironmentBuilder().
                          WithChildEntities(contact, account).
                          WithChildEntities(account, contact);

            //
            // Act
            //
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, contact);
            Assert.AreEqual(contact.EntityReference, service.GetEntity(account).PrimaryContactId);
            Assert.AreEqual(contact.EntityReference, account.Entity.PrimaryContactId);
            Assert.AreEqual(account.EntityReference, service.GetEntity(contact).ParentCustomerId);
            Assert.AreEqual(account.EntityReference, contact.Entity.ParentCustomerId);
        }

        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void CrmEnvironmentBuilder_WithChildEntities_IncidentAndAccountButAccountAddedIncidentFirst_Should_CreateAccountFirst()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var account = new Id<Account>(Guid.NewGuid());
            var incident = new Id<Incident>(Guid.NewGuid());

            // The Account and Incident will be added as Account first, and Incident second. 
            // The Lead will force an reorder and the Account incident would normally get placed after the Incident
            var builder = new DLaBCrmEnvironmentBuilder().
                          WithEntities(new Id<PhoneCall>(Guid.NewGuid()), incident, account).
                          WithChildEntities(account, incident);

            //
            // Act
            //
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, incident);
            Assert.AreEqual(account.EntityReference, service.GetEntity(incident).CustomerId);
            Assert.AreEqual(account.EntityReference, incident.Entity.CustomerId);
        }

        /// <summary>
        /// Incident's can't be created without a customer, so attempt to force the incident to be created first
        /// </summary>
        [TestMethod]
        public void CrmEnvironmentBuilder_WithChildEntities_ContactAndAccountAdded_Should_AddedViaCustomerId()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var account = new Id<Account>(Guid.NewGuid());
            var contact = new Id<Contact>(Guid.NewGuid());
            var accountIncident = new Id<Incident>(Guid.NewGuid());
            var contactIncident = new Id<Incident>(Guid.NewGuid());

            // The Account and Incident will be added as Account first, and Incident second. 
            // The Lead will force an reorder and the Account incident would normally get placed after the Incident
            var builder = new DLaBCrmEnvironmentBuilder()
                .WithChildEntities(contact, contactIncident)
                .WithChildEntities(account, accountIncident);


            //
            // Act
            //
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, accountIncident);
            AssertCrm.Exists(service, contact);
            AssertCrm.Exists(service, contactIncident);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_WithEntities_GivenIdStruct_Should_CreateAll()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            //
            // Act
            //
            var builder = new DLaBCrmEnvironmentBuilder().WithEntities<Ids>();
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, Ids.Value1);
            AssertCrm.Exists(service, Ids.Value2);
            AssertCrm.Exists(service, Ids.Nested.Value1);
            AssertCrm.Exists(service, Ids.Nested.Value2);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_ExceptEntities_GivenIdStruct_Should_CreateAllExceptExcluded()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            //
            // Act
            //
            var builder = new DLaBCrmEnvironmentBuilder().
                          WithEntities<Ids>().
                          ExceptEntities<Ids.Nested>();
            builder.Create(service);

            //
            // Assert
            //

            AssertCrm.Exists(service, Ids.Value1);
            AssertCrm.Exists(service, Ids.Value2);
            AssertCrm.NotExists(service, Ids.Nested.Value1.EntityReference);
            AssertCrm.NotExists(service, Ids.Nested.Value2.EntityReference);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_ForInactive_Should_CreateThenUpdate()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = new Id<Account>(Guid.NewGuid());
            id.Inject(new Account { StateCode = AccountState.Inactive, StatusCodeEnum = Account_StatusCode.Inactive });

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().WithEntities(id).Create(service);

            //
            // Assert
            //
            var account = service.GetEntity(id);
            Assert.AreEqual(AccountState.Inactive, account.StateCode);
            Assert.AreEqual(Account_StatusCode.Inactive, account.StatusCodeEnum);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_ForIsDisabled_Should_CreateThenUpdate()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = new Id<SystemUser>(Guid.NewGuid());
            id.Inject(new SystemUser { IsDisabled = true });

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().WithEntities(id).Create(service);

            //
            // Assert
            //
            var user = service.GetEntity(id);
            Assert.IsTrue(user.IsDisabled);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithSelfReferencingEntity_Should_CreateThenUpdate()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = Guid.NewGuid();
            var account = new Account
            {
                Id = id,
                ParentAccountId = new EntityReference(Account.EntityLogicalName, id)
            };

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().WithEntities(account).Create(service);

            //
            // Assert
            //

            AssertCrm.Exists(service, account);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithBuilderSelfReferencingEntity_Should_CreateThenUpdate()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = new Id<Lead>(Guid.NewGuid());

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().
                WithBuilder<MyLeadBuilder>(id, b => b.WithAttributeValue(Lead.Fields.MasterId, id.EntityReference)).
                Create(service);

            //
            // Assert
            //

            AssertCrm.Exists(service, id);
            var lead = service.GetEntity(id);
            Assert.AreEqual(lead.MasterId, id.EntityReference);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithCyclicReferencingEntities_Should_CreateThenUpdate()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            CyclicIds.Account.Entity = new Account
            {
                Id = CyclicIds.Account.EntityId,
                ParentAccountId = CyclicIds.Account,
                PrimaryContactId = CyclicIds.Contact,
                OriginatingLeadId = CyclicIds.Lead
            };

            CyclicIds.Contact.Entity = new Contact
            {
                Id = CyclicIds.Contact.EntityId,
                ParentCustomerId = CyclicIds.Account,
                OriginatingLeadId = CyclicIds.Lead
            };

            CyclicIds.Lead.Entity = new Lead
            {
                Id = CyclicIds.Lead.EntityId,
                CustomerId = CyclicIds.Account,
                ParentAccountId = CyclicIds.Account,
                ParentContactId = CyclicIds.Contact
            };


            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().WithEntities<CyclicIds>().Create(service);

            //
            // Assert
            //
            var account = service.GetEntity(CyclicIds.Account);
            var contact = service.GetEntity(CyclicIds.Contact);
            var lead = service.GetEntity(CyclicIds.Lead);
            Assert.AreEqual(account.ParentAccountId.Id, CyclicIds.Account.EntityId);
            Assert.AreEqual(account.PrimaryContactId.Id, CyclicIds.Contact.EntityId);
            Assert.AreEqual(account.OriginatingLeadId.Id, CyclicIds.Lead.EntityId);
            Assert.AreEqual(contact.ParentCustomerId.Id, CyclicIds.Account.EntityId);
            Assert.AreEqual(contact.OriginatingLeadId.Id, CyclicIds.Lead.EntityId);
            Assert.AreEqual(lead.CustomerId.Id, CyclicIds.Account.EntityId);
            Assert.AreEqual(lead.ParentAccountId.Id, CyclicIds.Account.EntityId);
            Assert.AreEqual(lead.ParentContactId.Id, CyclicIds.Contact.EntityId);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithBuilder_Should_UseBuilder()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = new Id<Lead>(Guid.NewGuid());
            var country = "Kerbal";

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().
                WithBuilder<MyLeadBuilder>(id, b => 
                    b.WithAddress1()
                     .WithAttributeValue(Lead.Fields.Address1_Country, Guid.NewGuid().ToString())
                     .WithPostCreateAttributeValue(Lead.Fields.Address1_Country, country)).Create(service);

            //
            // Assert
            //
            var lead = service.GetEntity(id);
            Assert.IsNotNull(lead.Address1_City);
            Assert.IsNotNull(lead.Address1_Line1);
            Assert.IsNotNull(lead.Address1_PostalCode);
            Assert.IsNotNull(lead.Address1_StateOrProvince);
            Assert.AreEqual(country, lead.Address1_Country);
            Assert.IsNull(lead.Address2_City);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithMyOtherBuilder_Should_UseBuilder()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var id = new Id<Lead>(Guid.NewGuid());
            var email = "test@test.com";

            //
            // Act
            //
            new DLaBCrmEnvironmentBuilder().
                WithBuilder<MyOtherBuilder>(id, b => b.WithEmail(email)).Create(service);

            //
            // Assert
            //
            var lead = service.GetEntity(id);
            Assert.AreEqual(email, lead.EMailAddress1);
        }


        [TestMethod]
        public void CrmEnvironmentBuilder_Create_WithMultipleChildEntities_Should_CreateEntities()
        { 

            //
            // Arrange
            //
            var account = new Id<Account>("2b9631fd-c402-490d-8276-0a0e0ff3ba2f");
            var contact = new Id<Contact>("2b9631fd-c402-490d-8276-0a0e0ff3ba2e");
            var lead = new Id<Lead>("2b9631fd-c402-490d-8276-0a0e0ff3ba2d");
            var opportunity = new Id<Opportunity>("2b9631fd-c402-490d-8276-0a0e0ff3ba2c");

            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            //
            // Act
            //
            new CrmEnvironmentBuilder()
                .WithChildEntities(lead, opportunity)
                .WithChildEntities(lead, contact)
                .WithChildEntities(account, contact)
                .Create(service);

            //
            // Assert
            //
            AssertCrm.Exists(service, lead);
            AssertCrm.Exists(service, contact);
            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, opportunity);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_Create_ConnectionAssociation_Should_CreateAssociationBeforeConnection()
        {
            var to = new Id<ConnectionRole>("D588F8F5-9276-471E-9A73-C62217C29FD1");
            var from = new Id<ConnectionRole>("29E71A5B-692D-4777-846D-FD1687D7DDB7");
            var association = new Id<ConnectionRoleAssociation>("C1B93E89-F070-4FE7-81B4-94BA123222CA");
            var connection = new Id<Connection>("AC56E429-452F-49F5-A463-894E8CA8E17C");
            connection.Inject(new Connection
            {
                Record1RoleId = to,
                Record2RoleId = from
            });
            association.Inject(new ConnectionRoleAssociation
            {
                ConnectionRoleId = to,
                AssociatedConnectionRoleId = from
            });
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            var containsFailure = false;
            try
            {
                new CrmEnvironmentBuilder()
                    .WithEntities(to, from, connection)
                    .Create(service);
            }
            catch (Exception ex)
            {
                containsFailure = ex.ToString().Contains("The connection roles are not related");
            }
            Assert.IsTrue(containsFailure, "CRM does not allow connection roles to be used on a connection without being associated.");
            service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            new CrmEnvironmentBuilder()
                .WithEntities(to, from, association, connection)
                .Create(service);
        }

        private class MyLeadBuilder : DLaBEntityBuilder<Lead, MyLeadBuilder>
        {
            public Lead Lead { get; set; }

            public MyLeadBuilder()
            {
                Lead = new Lead();
            }

            public MyLeadBuilder(Id id)
                : this()
            {
                Id = id;
            }

            #region Fluent Methods

            public MyLeadBuilder WithAddress1()
            {
                Lead.Address1_City = "Any Town";
                Lead.Address1_Line1 = "123 Any Street";
                Lead.Address1_PostalCode = "12345";
                Lead.Address1_StateOrProvince = "IN";
                return this;
            }

            #endregion // Fluent Methods


            protected override Lead BuildInternal()
            {
                return Lead;
            }
        }

        public abstract class MyGenericBuilder<TEntity, TBuilder> : DLaBEntityBuilder<TEntity, TBuilder>
            where TEntity : Entity
            where TBuilder : MyGenericBuilder<TEntity, TBuilder>
        {

        }

        private class MyOtherBuilder : MyGenericBuilder<Lead, MyOtherBuilder>
        {
            public Lead Lead { get; set; }

            public MyOtherBuilder()
            {
                Lead = new Lead();
            }

            public MyOtherBuilder(Id id) : this()
            {
                Id = id;
            }

            protected override Lead BuildInternal() { return Lead; }

            public MyOtherBuilder WithEmail(string email)
            {
                Lead.EMailAddress1 = email;
                return this;
            }
        }

        public struct CyclicIds
        {
            public static readonly Id<Account> Account = new Id<Account>(Guid.NewGuid());
            public static readonly Id<Contact> Contact = new Id<Contact>(Guid.NewGuid());
            public static readonly Id<Lead> Lead = new Id<Lead>(Guid.NewGuid());
        }

        public struct Ids
        {
            public struct Nested
            {
                // ReSharper disable MemberHidesStaticFromOuterClass
                public static readonly Id<Contact> Value1 = new Id<Contact>(Guid.NewGuid());
                public static readonly Id<Contact> Value2 = new Id<Contact>(Guid.NewGuid());
                // ReSharper restore MemberHidesStaticFromOuterClass
            }
            public static readonly Id<Contact> Value1 = new Id<Contact>(Guid.NewGuid());
            public static readonly Id<Contact> Value2 = new Id<Contact>(Guid.NewGuid());
        }
    }
}
