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
