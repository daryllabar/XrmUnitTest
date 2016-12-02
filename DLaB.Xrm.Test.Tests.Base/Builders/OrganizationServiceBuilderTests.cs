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

        private string GetName(string entityLogicalName, string attributeName)
        {
            return entityLogicalName + "|" + attributeName;
        }
    }
}