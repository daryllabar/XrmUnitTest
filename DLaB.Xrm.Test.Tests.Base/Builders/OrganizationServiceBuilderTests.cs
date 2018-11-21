using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Test.Builders;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class OrganizationServiceBuilderTests
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
        public void OrganizationServiceBuilder_WithEntityNameDefaulted_Name_Should_BeDefaulted()
        {
            //
            // Arrange
            //
            const string notExistsEntityLogicalName = "notexists";
            const string customEntityLogicalName = "custom_entity";
            var entities = new List<Entity>();

            IOrganizationService service =
                LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service = new OrganizationServiceBuilder(service)
                .WithFakeCreate((s, e) =>
                 {
                     // Don't create fake entities
                     if (e.LogicalName == notExistsEntityLogicalName)
                     {

                         entities.Add(e);
                         return Guid.NewGuid();
                     }
                     if (e.LogicalName == customEntityLogicalName)
                     {
                         entities.Add(e);
                         return Guid.NewGuid();
                     }

                     return s.Create(e);
                 })
                 .WithEntityNameDefaulted((e, info) => GetName(e.LogicalName, info.AttributeName))
                 .Build();


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
                    Assert.AreEqual("An attempt was made to create an entity of type contact with the EntityState set to created which normally means it comes from an OrganizationServiceContext.SaveChanges call.\r\nEither set ignoreContextCreation to true on the WithIdsDefaultedForCreate call, or define the id before calling SaveChanges, and add the id with the WithIdsDefaultedForCreate method.", inner.Message);
                }
            }
        }

        private string GetName(string entityLogicalName, string attributeName)
        {
            return entityLogicalName + "|" + attributeName;
        }
    }
}