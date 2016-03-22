using System;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class CrmEnvironmentBuilderTests
    {
        [TestInitialize]
        public void IntializeTestSettings()
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
            // The Lead will force an reorder and the Account incident would normally get placed after the Incident
            var builder = new CrmEnvironmentBuilder().
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
            var builder = new CrmEnvironmentBuilder().WithEntities<Ids>();
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
            var builder = new CrmEnvironmentBuilder().
                WithEntities<Ids>().
                ExceptEntities<Ids.Nested>();
            builder.Create(service);

            //
            // Assert
            //

            AssertCrm.Exists(service, Ids.Value1);
            AssertCrm.Exists(service, Ids.Value2);
            AssertCrm.NotExists(service, Ids.Nested.Value1);
            AssertCrm.NotExists(service, Ids.Nested.Value2);
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
            new CrmEnvironmentBuilder().WithEntities(account).Create(service);

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
            new CrmEnvironmentBuilder().
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
            new CrmEnvironmentBuilder().WithEntities<CyclicIds>().Create(service);

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

            //
            // Act
            //
            new CrmEnvironmentBuilder().
                WithBuilder<MyLeadBuilder>(id, b => b.WithAddress1()).Create(service);

            //
            // Assert
            //
            var lead = service.GetEntity(id);
            Assert.IsNotNull(lead.Address1_City);
            Assert.IsNotNull(lead.Address1_Line1);
            Assert.IsNotNull(lead.Address1_PostalCode);
            Assert.IsNotNull(lead.Address1_StateOrProvince);
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
            new CrmEnvironmentBuilder().
                WithBuilder<MyOtherBuilder>(id, b => b.WithEmail(email)).Create(service);

            //
            // Assert
            //
            var lead = service.GetEntity(id);
            Assert.AreEqual(email, lead.EMailAddress1);
        }

        public class MyLeadBuilder : EntityBuilder<Lead>
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

        public abstract class MyGenericBuilder<TEntity> : EntityBuilder<TEntity> where TEntity : Entity
        {
            
        }

        public class MyOtherBuilder : MyGenericBuilder<Lead>
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
                public static readonly Id Value1 = new Id<Contact>(Guid.NewGuid());
                public static readonly Id Value2 = new Id<Contact>(Guid.NewGuid());
                // ReSharper restore MemberHidesStaticFromOuterClass
            }
            public static readonly Id Value1 = new Id<Contact>(Guid.NewGuid());
            public static readonly Id Value2 = new Id<Contact>(Guid.NewGuid());
        }

    }
}
