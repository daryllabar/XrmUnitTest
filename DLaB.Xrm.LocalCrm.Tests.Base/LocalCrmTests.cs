using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Common;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm.Tests
{

    [TestClass]
    public class LocalCrmTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_DeactivateActivate()
        {
            var service = GetService();
            var entityType = typeof (Entity);
            var entitiesMissingStatusCodeAttribute = new List<string>();
            var entitiesMissingStateCodeAttribute = new List<string>();

            foreach (var entity in from t in typeof (SystemUser).Assembly.GetTypes()
                where entityType.IsAssignableFrom(t) && new LateBoundActivePropertyInfo(EntityHelper.GetEntityLogicalName(t)).ActiveAttribute != ActiveAttributeType.None 
                select (Entity) Activator.CreateInstance(t))
            {
                if (entity.LogicalName == RecommendationCache.EntityLogicalName)
                {
                    entity.Id = service.Create(entity);
                }
                if (entity.LogicalName == Incident.EntityLogicalName)
                {
                    // Satisfy ErrorCodes.unManagedidsincidentparentaccountandparentcontactnotpresent
                    var customer = new Account();
                    customer.Id = service.Create(customer);
                    entity[Incident.Fields.CustomerId] = customer.ToEntityReference();
                }
                entity.Id = service.Create(entity);
                AssertCrm.IsActive(service, entity, "Entity " + entity.GetType().Name + " wasn't created active!");
                try
                {
                    if (entity.LogicalName == Incident.EntityLogicalName)
                    {
                        // Requires the Special Resolve Incident Request Message
                        continue;
                    }

                    service.SetState(entity.LogicalName, entity.Id, false);
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("does not contain an attribute with name statuscode"))
                    {
                        entitiesMissingStatusCodeAttribute.Add(entity.LogicalName);
                        continue;
                    }
                    if (ex.ToString().Contains("does not contain an attribute with name statecode"))
                    {
                        entitiesMissingStateCodeAttribute.Add(entity.LogicalName);
                        continue;
                    }

                    throw;
                }
                AssertCrm.IsNotActive(service, entity, "Entity " + entity.GetType().Name + " wasn't deactivated!");
                service.SetState(entity.LogicalName, entity.Id, true);
                AssertCrm.IsActive(service, entity, "Entity " + entity.GetType().Name + " wasn't activated!");
            }

            if (entitiesMissingStatusCodeAttribute.Any())
            {
                Assert.Fail("The following Entities do not contain an attribute with name statuscode " + entitiesMissingStatusCodeAttribute.ToCsv() + ". Check DLaB.Xrm.ActiveAttributeType for proper configuration.");
            }

            if (entitiesMissingStateCodeAttribute.Any())
            {
                Assert.Fail("The following Entities do not contain an attribute with name statecode " + entitiesMissingStateCodeAttribute.ToCsv() + ". Check DLaB.Xrm.ActiveAttributeType for proper configuration.");
            }
        }

        [TestMethod]
        public void LocalCrmTests_EmptyStringIsNull()
        {
            var service = GetService();
            var id = service.Create(new Lead {Address1_City = string.Empty});
            Assert.IsFalse(service.GetEntity<Lead>(id).Attributes.ContainsKey(Lead.Fields.Address1_City));
        }

        [TestMethod]
        public void LocalCrmTests_OwnerPopulated()
        {
            var id = Guid.NewGuid();
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(userId: id);
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(info);
            var accountId = service.Create(new Account());

            var account = service.GetEntity<Account>(accountId);

            // Retrieve
            Assert.IsNotNull(account.OwnerId);
            Assert.AreEqual(id, account.OwnerId.Id);
        }

        [TestMethod]
        public void LocalCrmTests_DefaultEntitiesCreated()
        {
            var service = GetService();

            var user = service.GetFirstOrDefault<SystemUser>();
            var bu = service.GetFirstOrDefault<BusinessUnit>();

            Assert.IsNotNull(user, "User was not created by default");
            Assert.IsNull(bu, "Business Unit was created without being specified");

            service = GetService(businessUnitId: Guid.NewGuid());

            user = service.GetFirstOrDefault<SystemUser>();
            bu = service.GetFirstOrDefault<BusinessUnit>();

            Assert.IsNotNull(user, "User was not created by default");
            Assert.IsNotNull(bu, "Business Unit was not created");
        }
    }
}