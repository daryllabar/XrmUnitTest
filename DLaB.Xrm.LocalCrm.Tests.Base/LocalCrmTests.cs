﻿using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Common;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{

    [TestClass]
    public class LocalCrmTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_ParentCustomerIdUsesAccountId()
        {
            var service = GetService();
            var account = new Account();
            account.Id = service.Create(account);
            var contact = new Contact
            {
                ParentCustomerId = account.ToEntityReference(),
            };
            service.Create(contact);

            var query = QueryExpressionFactory.Create<Contact>();
            query.LinkEntities.Add(new LinkEntity(Contact.EntityLogicalName, Account.EntityLogicalName, Contact.Fields.AccountId, Account.Fields.AccountId, JoinOperator.Inner));
            var contacts = service.GetEntities(query).ToList();

            var qe = QueryExpressionFactory.Create<Account>();
            qe.AddLink<Contact>(Account.Fields.Id, Contact.Fields.AccountId);

            var accounts = service.GetEntities(qe);

            Assert.AreEqual(1, contacts.Count);
            Assert.AreEqual(1, accounts.Count);
        }

        [TestMethod]
        public void LocalCrmTests_Something()
        {
            var service = GetService();
            var email = new Entity(Email.EntityLogicalName);
            var activityParty  = new Entity(ActivityParty.EntityLogicalName);
            activityParty.Attributes.Add(ActivityParty.Fields.ParticipationTypeMask, new OptionSetValue(2));

            email[Email.Fields.To] = new EntityCollection ( new List<Entity> { activityParty });
            email[Email.Fields.From] = new EntityCollection(
            new List<Entity> {
                new (ActivityParty.EntityLogicalName) {
                    [ActivityParty.Fields.PartyId] = new EntityReference(SystemUser.EntityLogicalName, service.GetCurrentlyExecutingUserInfo().UserId),
                    [ActivityParty.Fields.ParticipationTypeMask ] = new OptionSetValue(1)
                }
            });

            try
            {
                service.Create(email);
                Assert.Fail("Error expected!");
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ex.Message, "Activity Party PartyId and AddressUsed were null");
            }

            activityParty[ActivityParty.Fields.PartyId] = new EntityReference(SystemUser.EntityLogicalName, service.GetCurrentlyExecutingUserInfo().UserId);
            email[Email.Fields.To] = new EntityCollection(new List<Entity> { activityParty });
            service.Create(email);

            activityParty[ActivityParty.Fields.PartyId] = null;
            activityParty[ActivityParty.Fields.AddressUsed] = "A@A.com"; 
            email[Email.Fields.To] = new EntityCollection(new List<Entity> { activityParty });
            service.Create(email);
        }

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
                if (entity.LogicalName == Incident.EntityLogicalName)
                {
                    // Satisfy ErrorCodes.unManagedidsincidentparentaccountandparentcontactnotpresent
                    var customer = new Account();
                    customer.Id = service.Create(customer);
                    entity[Incident.Fields.CustomerId] = customer.ToEntityReference();
                }
                else if (entity.LogicalName == Connection.EntityLogicalName)
                {
                    // Satisfy ErrorCodes.BothConnectionSidesAreNeeded
                    var account = new Account();
                    account.Id = service.Create(account);
                    entity[Connection.Fields.Record1Id] = account.ToEntityReference();
                    account.Id = service.Create(new Account());
                    entity[Connection.Fields.Record2Id] = account.ToEntityReference();
                }

                entity.Id = service.Create(entity);

                try
                {
                    AssertCrm.IsActive(service, entity, "Entity " + entity.GetType().Name + " wasn't created active!");
                    if (entity.LogicalName == Incident.EntityLogicalName)
                    {
                        // Requires the Special Resolve Incident Request Message
                        continue;
                    }

                    service.SetState(entity.LogicalName, entity.Id, false);
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                    if (error.Contains("does not contain an attribute with name statuscode")
                        || error.Contains("The property \"statuscode\" was not found in the entity type"))
                    {
                        entitiesMissingStatusCodeAttribute.Add(entity.LogicalName);
                        continue;
                    }
                    if (error.Contains("does not contain an attribute with name statecode")
                        || error.Contains("The property \"statecode\" was not found in the entity type"))
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
        public void LocalCrmTests_NullColumnSetAllowed()
        {
            var service = GetService();
            var id = service.Create(new Contact());
            var qe = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = null,
            };

            Assert.AreEqual(1, service.GetFirst(qe).Attributes.Count);
            Assert.AreEqual(id, service.GetFirst(qe).Id);
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
        public void LocalCrmTests_OwningBuPopulated()
        {
            var id = Guid.NewGuid();
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(userId: id, userBusinessUnit: Guid.NewGuid());
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(info);
            var buId = service.Create(new BusinessUnit());
            var userId = service.Create(new SystemUser
            {
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, buId)
            });
            var user = service.GetEntity<SystemUser>(userId);
            Assert.AreEqual(user.BusinessUnitId.Id, buId);

            var accountId = service.Create(new Account
            {
                OwnerId = new EntityReference(SystemUser.EntityLogicalName, userId)
            });

            // Test Create BU Logic
            var account = service.GetEntity<Account>(accountId);
            Assert.IsNotNull(account.OwningBusinessUnit);
            Assert.AreEqual(buId, account.OwningBusinessUnit.Id);

            // Test Update BU Logic
            service.Update(new Account
            {
                Id = accountId,
                OwnerId = new EntityReference(SystemUser.EntityLogicalName, id)
            });
            account = service.GetEntity<Account>(accountId);
            Assert.IsNotNull(account.OwningBusinessUnit);
            Assert.AreNotEqual(buId, account.OwningBusinessUnit.Id);
        }

        [TestMethod]
        public void LocalCrmTests_DefaultEntitiesCreated()
        {
            var service = GetService();

            var user = service.GetFirstOrDefault<SystemUser>();
            var bu = service.GetFirstOrDefault<BusinessUnit>();

            Assert.IsNotNull(user, "User was not created by default");
            Assert.IsNotNull(bu, "Business Unit was not created by default");
        }

        [TestMethod]
        public void LocalCrmTests_RetrievesAreCloned()
        {
            var service = GetService();
            var contact = new Contact();
            contact.Id = service.Create(contact);
            var contact1 = service.GetEntity<Contact>(contact.Id);
            var contact2 = service.GetEntity<Contact>(contact.Id);
            Assert.AreNotSame(contact1,contact2, "Each retrieve should create a new instance");

            var phoneCall = new PhoneCall();
            SetActivityParty(phoneCall, PhoneCall.Fields.From, new ActivityParty { PartyId = contact.ToEntityReference() });
            phoneCall.Id = service.Create(phoneCall);
            var phoneCall1 = service.GetEntity<PhoneCall>(phoneCall.Id);
            var phoneCall2 = service.GetEntity<PhoneCall>(phoneCall.Id);
            Assert.AreNotSame(phoneCall1[PhoneCall.Fields.From], phoneCall2[PhoneCall.Fields.From], "Each retrieve should create a new instance");

        }

        [TestMethod]
        public void LocalCrmTests_NonPrimitiveDataTypesAreCloned()
        {
            var service = GetService();
            var contact = new Contact();
            contact.Id = service.Create(contact);
            var phoneCall = new PhoneCall();
            phoneCall.Id = service.Create(phoneCall);

            SetActivityParty(phoneCall, PhoneCall.Fields.From, new ActivityParty { PartyId = contact.ToEntityReference()});
            phoneCall.CreatedBy = contact.ToEntityReference();

            service.Update(phoneCall);

            phoneCall.CreatedBy.Name = "Test";
            Assert.AreEqual(1, service.GetFirst<PhoneCall>().From.Count());

            var from = phoneCall.GetAttributeValue<EntityCollection>(PhoneCall.Fields.From);
            from.Entities.Clear();

            Assert.IsNull(service.GetFirst<PhoneCall>().CreatedBy.Name);
            Assert.AreEqual(1, service.GetFirst<PhoneCall>().From.Count());
        }

        private static void SetActivityParty(Entity entity, string fieldName, ActivityParty party)
        {
            var partyList = entity.GetAttributeValue<EntityCollection>(fieldName);
            if (partyList == null)
            {
                partyList = new EntityCollection();
                entity[fieldName] = partyList;
            }
            else
            {
                partyList.Entities.Clear();
            }
            partyList.Entities.Add(party);
        }
    }
}