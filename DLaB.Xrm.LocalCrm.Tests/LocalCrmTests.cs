using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm.Tests
{

    [TestClass]
    public class LocalCrmTests
    {
        private static IOrganizationService GetService(bool createUnique = true, Guid? businessUnitId = null)
        {
            var info = createUnique
                ? LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString(), userBusinessUnit: businessUnitId)
                : LocalCrmDatabaseInfo.Create<CrmContext>(userBusinessUnit: businessUnitId);
            return new LocalCrmDatabaseOrganizationService(info);
        }

        [TestMethod]
        public void LocalCrmTests_RetrieveById()
        {
            var service = GetService();
            var id1 = service.Create(new Contact());
            var id2 = service.Create(new Contact());
            Assert.AreNotEqual(id1, id2, "Shouldn't have created duplicate Ids!");
            Assert.AreEqual(2, service.GetEntities<Contact>().Count, "Two and only two contacts should exist!");
            Assert.AreEqual(id1, service.GetEntity<Contact>(id1).Id, "Failed looking up Contact 1 by Id");
            Assert.AreEqual(id2, service.GetEntity<Contact>(id2).Id, "Failed looking up Contact 2 by Id");
        }

        [TestMethod]
        public void LocalCrmTests_RetrieveOuterJoinedColumn()
        {
            var service = GetService();
            var contact = new Contact {FirstName = "Joe"};
            contact.Id = service.Create(contact);

            // Create 2 opportunities
            service.Create(new Opportunity {CustomerId = contact.ToEntityReference()});
            service.Create(new Opportunity());

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, Contact.Fields.ContactId, JoinOperator.LeftOuter, c => new {c.FirstName});

            var entities = service.GetEntities(qe);
            Assert.AreEqual(2, entities.Count, "Two opportunities should have been returned!");
            Assert.AreEqual(contact.FirstName, entities.First(o => o.ParentContactId != null).GetAliasedEntity<Contact>().FirstName, "First Name wasn't returned!");
            Assert.IsNull(entities.First(o => o.ParentContactId == null).GetAliasedEntity<Contact>().FirstName, "Second Opportunity some how has a contact!");
        }

        [TestMethod]
        public void LocalCrmTests_RetrieveFilterOnOuterJoinedColumn()
        {
            var service = GetService();
            var contact1 = new Contact {FirstName = "Joe"};
            contact1.Id = service.Create(contact1);
            var contact2 = new Contact {FirstName = "Jim"};
            contact2.Id = service.Create(contact2);
            var contact3 = new Contact {FirstName = "Jake"};
            contact3.Id = service.Create(contact3);

            service.Create(new Opportunity {CustomerId = contact1.ToEntityReference()});
            service.Create(new Opportunity {CustomerId = contact2.ToEntityReference()});
            service.Create(new Opportunity {CustomerId = contact3.ToEntityReference()});
            service.Create(new Opportunity());

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, Contact.Fields.ContactId, JoinOperator.LeftOuter, c => new {c.FirstName}).EntityAlias = "MyAlias";
            qe.Criteria.AddCondition("MyAlias", Contact.Fields.FirstName, ConditionOperator.Equal, "Joe");
            var entities = service.GetEntities(qe);

            Assert.AreEqual(1, entities.Count, "Only Joe opportunities should have been returned!");
            Assert.AreEqual(contact1.FirstName, entities[0].GetAliasedEntity<Contact>().FirstName);

            qe.Criteria.AddCondition("MyAlias", Contact.Fields.FirstName, ConditionOperator.Equal, "Jim");
            qe.Criteria.FilterOperator = LogicalOperator.Or;
            entities = service.GetEntities(qe);

            Assert.AreEqual(2, entities.Count, "Joe and Jim opportunities should have been returned!");
        }

        [TestMethod]
        public void LocalCrmTests_ColumnSetLookups()
        {
            var service = GetService();
            const string firstName = "Joe";
            const string lastName = "Plumber";
            var contact = new Contact {FirstName = firstName, LastName = lastName};
            contact.Id = service.Create(contact);
            var cs = new ColumnSet("firstname");
            Assert.AreEqual(firstName, service.GetEntity<Contact>(contact.Id, cs).FirstName, "Failed to retrieve first name correctly");
            Assert.IsNull(service.GetEntity<Contact>(contact.Id, cs).LastName, "Last name was not requested, but was returned");
            Assert.AreEqual(firstName + " " + lastName, service.GetEntity<Contact>(contact.Id).FullName, "Full Name not populated correctly");
        }

        [TestMethod]
        public void LocalCrmTests_BasicCrud()
        {
            var c1 = new Contact {Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Plumber"};
            var c2 = new Contact {Id = Guid.NewGuid(), FirstName = "Bill", LastName = "Carpenter"};
            var opp = new Opportunity {Id = Guid.NewGuid(), CustomerId = c1.ToEntityReference() };

            var service = GetService();
            service.Create(c1);
            service.Create(c2);
            service.Create(opp);

            Assert.IsNotNull(service.GetFirstOrDefault<Opportunity>(Opportunity.Fields.ParentContactId, c1.Id), "Failed Simple Lookup by Attribute Entity Reference");
            Assert.AreEqual(1, service.GetEntitiesById<Contact>(c1.Id).Count, "Failed Simple Where In Lookup by Id");

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, "contactid", c => new {c.FirstName});

            var otherC = service.GetFirstOrDefault(qe);

            Assert.IsNotNull(otherC, "Failed Simple Lookup with Linked Entity on Entity Reference");
            Assert.AreEqual(c1.FirstName, otherC.GetAliasedEntity<Contact>().FirstName, "Failed Simple Lookup retrieving Linked Entity columns");
        }

        [TestMethod]
        public void LocalCrmTests_AndOrConstraints()
        {
            var service = GetService();
            service.Create(new Contact {FirstName = "Abraham", LastName = "Lincoln"});
            service.Create(new Contact {FirstName = "George W", LastName = "Bush"});
            service.Create(new Contact {FirstName = "George H W", LastName = "Bush"});

            Assert.AreEqual(1, service.GetEntities<Contact>(
                "lastname", "Bush",
                "firstname", "George W").Count, "Failed And for two constraints");

            Assert.AreEqual(2, service.GetEntities<Contact>(
                "lastname", "Bush",
                LogicalOperator.Or,
                "firstname", "George W").Count, "Failed Or for two Or's");

            Assert.AreEqual(1, service.GetEntities<Contact>(
                "lastname", "Lincoln",
                LogicalOperator.Or,
                "firstname", "Tad").Count, "Failed or for two Or's with only one item to be found");

            var qe = QueryExpressionFactory.Create<Contact>();
            qe.Criteria.Conditions.Clear();
            qe.Criteria.FilterOperator = LogicalOperator.Or;
            qe.Criteria.AddFilter(new FilterExpression(LogicalOperator.And));
            qe.Criteria.AddFilter(new FilterExpression(LogicalOperator.And));
            qe.Criteria.Filters[0].WhereIn("lastname", "Lincoln");
            qe.Criteria.Filters[1].WhereIn("firstname", "George W");

            Assert.AreEqual(2, service.GetEntities(qe).Count, "Failed Nested Filter Or'd");

            qe.Criteria.FilterOperator = LogicalOperator.And;
            qe.Criteria.Filters[0].Conditions.Clear();
            qe.Criteria.Filters[0].WhereIn("lastname", "Bush");
            Assert.AreEqual(1, service.GetEntities(qe).Count, "Failed Nested Filter And'd");
        }

        [TestMethod]
        public void LocalCrmTests_LateBoundCrud()
        {
            var contact = new Contact();
            var lateContact = new Entity {LogicalName = Contact.EntityLogicalName};

            var service = GetService();
            contact.Id = service.Create(contact);
            lateContact.Id = service.Create(lateContact);

            Assert.IsNotNull(service.Retrieve(contact.LogicalName, contact.Id, new ColumnSet()), "Failed Create or Read");
            Assert.IsNotNull(service.Retrieve(lateContact.LogicalName, lateContact.Id, new ColumnSet()), "Failed Create or Read");
            Assert.AreEqual(2, service.GetEntities<Contact>().Count, "Failed Create or Read");

            contact.FirstName = "Early";
            lateContact["firstname"] = "Late";

            service.Update(contact);
            service.Update(lateContact);

            Assert.IsNotNull(service.Retrieve(contact.LogicalName, contact.Id, new ColumnSet("firstname")), "Failed Update or Read");
            Assert.IsNotNull(service.Retrieve(lateContact.LogicalName, lateContact.Id, new ColumnSet("firstname")), "Failed Update or Read");

            service.Delete(contact);
            service.Delete(lateContact);

            Assert.AreEqual(0, service.GetEntities<Contact>().Count, "Failed Delete or Read");
        }

        [TestMethod]
        public void LocalCrmTests_AdvancedCrud()
        {
            // Verify that linked items exist upon create
            var service = GetService();
            var contact = new Contact {Id = Guid.NewGuid()};
            var opp = new Opportunity {ParentContactId = contact.ToEntityReference()};

            try
            {
                service.Create(opp);
                Assert.Fail("Opportunity Creation should have failed since the Contact Doesn't exist");
            }
            catch (System.ServiceModel.FaultException<OrganizationServiceFault> ex)
            {
                Assert.IsTrue(ex.Message.Contains($"With Id = {contact.Id} Does Not Exist"), "Exception type is different than expected");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception)
            {
                Assert.Fail("Exception type is different than expected");
            }

            service.Create(contact);
            AssertCrm.Exists(service, contact);
            opp.Id = service.Create(opp);
            AssertCrm.Exists(service, opp);
        }

        [TestMethod]
        public void LocalCrmTests_DeactivateActivate()
        {
            var service = GetService();
            var entityType = typeof (Entity);
            var entitiesMissingStatusCodeAttribute = new List<string>();

            foreach (var entity in from t in typeof (SystemUser).Assembly.GetTypes()
                where entityType.IsAssignableFrom(t) && new LateBoundActivePropertyInfo(EntityHelper.GetEntityLogicalName(t)).ActiveAttribute != ActiveAttributeType.None
                select (Entity) Activator.CreateInstance(t))
            {
                entity.Id = service.Create(entity);
                AssertCrm.IsActive(service, entity, "Entity " + entity.GetType().Name + " wasn't created active!");
                try
                {
                    service.SetState(entity.LogicalName, entity.Id, false);
                }
                catch (Exception ex)
                {
                    if(ex.ToString().Contains("does not contain an attribute with name statuscode"))
                    {
                        entitiesMissingStatusCodeAttribute.Add(entity.LogicalName);
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
                Assert.Fail("The following Entities do not contain an attribute with name statuscode " + String.Join(", ", entitiesMissingStatusCodeAttribute) + ". Check DLaB.Xrm.ActiveAttributeType for proper configuration.");
            }
        }

        [TestMethod]
        public void LocalCrmTests_OrderBy()
        {
            var service = GetService();
            service.Create(new Contact {FirstName = "Chuck", LastName = "Adams"});
            service.Create(new Contact {FirstName = "Anna", LastName = "Adams"});
            service.Create(new Contact {FirstName = "Bill", LastName = "Adams"});

            var qe = new QueryExpression(Contact.EntityLogicalName) {ColumnSet = new ColumnSet(true)};

            // Ascending Order Test
            qe.AddOrder("firstname", OrderType.Ascending);
            var results = service.GetEntities<Contact>(qe);

            Assert.AreEqual("Anna", results[0].FirstName, "Ascending Ordering failed.  \"Anna\" should have been returned first");
            Assert.AreEqual("Bill", results[1].FirstName, "Ascending Ordering failed.  \"Bill\" should have been returned second");
            Assert.AreEqual("Chuck", results[2].FirstName, "Ascending Ordering failed.  \"Chuck\" should have been returned third");

            // Descending Order Test
            qe.Orders[0].OrderType = OrderType.Descending;
            results = service.GetEntities<Contact>(qe);

            Assert.AreEqual("Chuck", results[0].FirstName, "Descending Ordering failed.  \"Chuck\" should have been returned first");
            Assert.AreEqual("Bill", results[1].FirstName, "Descending Ordering failed.  \"Bill\" should have been returned second");
            Assert.AreEqual("Anna", results[2].FirstName, "Descending Ordering failed.  \"Anna\" should have been returned third");

            // Add Dups
            System.Threading.Thread.Sleep(1000); // Sleep to ensure that the creation date is different
            service.Create(new Contact {FirstName = "Chuck", LastName = "Bell"});
            service.Create(new Contact {FirstName = "Anna", LastName = "Bell"});
            service.Create(new Contact {FirstName = "Anna", LastName = "Carter"});
            service.Create(new Contact {FirstName = "Bill", LastName = "Bell"});
            service.Create(new Contact {FirstName = "Bill", LastName = "Carter"});
            service.Create(new Contact {FirstName = "Chuck", LastName = "Carter"});

            // Order By Descending First Then By Ascending Last Test
            qe.AddOrder("lastname", OrderType.Ascending);
            results = service.GetEntities<Contact>(qe);

            Assert.AreEqual(9, results.Count, "9 Contacts have been created");
            Assert.AreEqual("Chuck Adams", results[0].FullName, "Descending then Ascending Ordering failed.  \"Chuck Adams\" should have been returned first");
            Assert.AreEqual("Chuck Bell", results[1].FullName, "Descending then Ascending Ordering failed.  \"Chuck Bell\" should have been returned second");
            Assert.AreEqual("Chuck Carter", results[2].FullName, "Descending then Ascending Ordering failed.  \"Chuck Carter\" should have been returned third");
            Assert.AreEqual("Bill Adams", results[3].FullName, "Descending then Ascending Ordering failed.  \"Bill Adams\" should have been returned forth");
            Assert.AreEqual("Bill Bell", results[4].FullName, "Descending then Ascending Ordering failed.  \"Bill Bell\" should have been returned fifth");
            Assert.AreEqual("Bill Carter", results[5].FullName, "Descending then Ascending Ordering failed.  \"Bill Carter\" should have been returned sixth");
            Assert.AreEqual("Anna Adams", results[6].FullName, "Descending then Ascending Ordering failed.  \"Anna Adams\" should have been returned seventh");
            Assert.AreEqual("Anna Bell", results[7].FullName, "Descending then Ascending Ordering failed.  \"Anna Bell\" should have been returned eighth");
            Assert.AreEqual("Anna Carter", results[8].FullName, "Descending then Ascending Ordering failed.  \"Anna Carter\" should have been returned nineth");

            // Order By Ascending First Then By Descending Last Test
            qe.Orders.Clear();
            qe.AddOrder("firstname", OrderType.Ascending);
            qe.AddOrder("lastname", OrderType.Descending);
            results = service.GetEntities<Contact>(qe);

            Assert.AreEqual("Anna Carter", results[0].FullName, "Ascending then Descending Ordering failed.  \"Anna Carter\" should have been returned first");
            Assert.AreEqual("Anna Bell", results[1].FullName, "Ascending then Descending Ordering failed.  \"Anna Bell\" should have been returned second");
            Assert.AreEqual("Anna Adams", results[2].FullName, "Ascending then Descending Ordering failed.  \"Anna Adams\" should have been returned thrid");
            Assert.AreEqual("Bill Carter", results[3].FullName, "Ascending then Descending Ordering failed.  \"Bill Carter\" should have been returned forth");
            Assert.AreEqual("Bill Bell", results[4].FullName, "Ascending then Descending Ordering failed.  \"Bill Bell\" should have been returned fifth");
            Assert.AreEqual("Bill Adams", results[5].FullName, "Ascending then Descending Ordering failed.  \"Bill Adams\" should have been returned sixth");
            Assert.AreEqual("Chuck Carter", results[6].FullName, "Ascending then Descending Ordering failed.  \"Chuck Carter\" should have been returned seventh");
            Assert.AreEqual("Chuck Bell", results[7].FullName, "Ascending then Descending Ordering failed.  \"Chuck Bell\" should have been returned eighth");
            Assert.AreEqual("Chuck Adams", results[8].FullName, "Ascending then Descending Ordering failed.  \"Chuck Adams\" should have been returned nineth");

            // Order by Date Ascending
            qe.Orders.Clear();
            qe.AddOrder("createdon", OrderType.Ascending);
            results = service.GetEntities<Contact>(qe);

            Assert.AreEqual("Adams", results[0].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned first");
            Assert.AreEqual("Adams", results[1].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned first");
            Assert.AreEqual("Adams", results[2].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned first");


            // Order by Date Descending
            qe.Orders.Clear();
            qe.AddOrder("createdon", OrderType.Descending);
            results = service.GetEntities<Contact>(qe);

            Assert.AreEqual("Adams", results[8].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned last");
            Assert.AreEqual("Adams", results[7].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned last");
            Assert.AreEqual("Adams", results[6].LastName, "Ascending Date Ordering failed.  \"Adams\" should have been returned last");
        }

        [TestMethod]
        public void LocalCrmTests_EmptyStringIsNull()
        {
            var service = GetService();
            var id = service.Create(new Lead {Address1_City = String.Empty});
            Assert.IsFalse(service.GetEntity<Lead>(id).Attributes.ContainsKey(Lead.Fields.Address1_City));
        }

        [TestMethod]
        public void LocalCrmTests_FormattedValuePopulated()
        {
            var service = GetService();
            var id = service.Create(new Lead { BudgetStatusEnum = budgetstatus.CanBuy });

            // Retrieve
            var entity = service.GetEntity<Lead>(id);
            Assert.AreEqual(budgetstatus.CanBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));

            // RetrieveMultiple
            entity = service.GetEntitiesById<Lead>(id).Single();
            Assert.AreEqual(budgetstatus.CanBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
        }

        [TestMethod]
        public void LocalCrmTests_AliasedFormattedValuePopulated()
        {
            var service = GetService();
            var id = service.Create(new Lead { BudgetStatusEnum = budgetstatus.MayBuy });
            service.Create(new Account {OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, id)});

            var qe = QueryExpressionFactory.Create<Account>();
            qe.AddLink<Lead>(Account.Fields.OriginatingLeadId, Lead.Fields.LeadId, l => new {l.BudgetStatus});

            // Retrieve
            var entity = service.GetFirst(qe);
            Assert.AreEqual(budgetstatus.MayBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
            Assert.AreEqual(budgetstatus.MayBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
        }

        [TestMethod]
        public void LocalCrmTests_FormattedValuesIgnored()
        {
            var service = GetService();

            var contact = new Contact { AccountRoleCodeEnum = contact_accountrolecode.DecisionMaker };
            contact.FormattedValues.Add(Contact.Fields.AccountRoleCode, "Verify Formatted Values Are Ignored");
            service.Create(contact);

            contact = service.GetFirstOrDefault<Contact>();
            Assert.AreEqual(contact_accountrolecode.DecisionMaker, contact.AccountRoleCodeEnum);
            Assert.AreEqual(contact_accountrolecode.DecisionMaker.ToString(), contact.FormattedValues[Contact.Fields.AccountRoleCode]);
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
            Assert.AreEqual(id,account.OwnerId.Id);
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