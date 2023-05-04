using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DLaB.Xrm.CrmSdk;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using XrmUnitTest.Test;

#if NET
using OrganizationServiceBuilder = DataverseUnitTest.Builders.OrganizationServiceBuilder;
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using OrganizationServiceBuilder = DLaB.Xrm.Test.Builders.OrganizationServiceBuilder;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class CrudTests : BaseTestClass
    {
        [TestMethod]
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        public void LocalCrmTests_Crud_ActivityPartyConstraints()
        {
            // Verify that linked items exist upon create
            var service = GetService();
            try
            {
                service.Create(new ActivityParty());
                Assert.Fail("Direct creation of ActivityParty should not be allowed!");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
            }

            try
            {
                service.Update(new ActivityParty {Id = Guid.NewGuid()});
                Assert.Fail("Direct update of ActivityParty should not be allowed!");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
            }

            try
            {
                service.Delete(new ActivityParty {Id = Guid.NewGuid()});
                Assert.Fail("Direct deletion of ActivityParty should not be allowed!");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
            }
        }

        [TestMethod]
        public void LocalCrmTests_Crud_ActivityPartyAutoCreation()
        {
            var service = GetService();
            var contact = new Contact();
            contact.Id = service.Create(contact);
            var account = new Account();
            account.Id = service.Create(account);
            var party = new ActivityParty
            {
                PartyId = account.ToEntityReference()
            };
            var parties = new EntityCollection {EntityName = account.LogicalName};
            parties.Entities.Add(party);
            var phoneCall = new PhoneCall
            {
                [PhoneCall.Fields.To] = parties,
                RegardingObjectId = contact.ToEntityReference()
            };
            service.Create(phoneCall);
                
            var qe = QueryExpressionFactory.Create<PhoneCall>();
            var apLink = qe.AddLink<ActivityParty>(PhoneCall.Fields.ActivityId);
            apLink.Columns.AddColumn(ActivityParty.Fields.ParticipationTypeMask);
            //apLink.AddLink(account.LogicalName, ActivityParty.Fields.PartyId, Account.Fields.Id);
            var values = service.GetEntities(qe)
                                .Select(p => p.GetAliasedEntity<ActivityParty>().ParticipationTypeMaskEnum)
                                .ToList();
            Assert.AreEqual(1, values.Count(m => m == ActivityParty_ParticipationTypeMask.ToRecipient), "The to activity party should have been created.");
            Assert.AreEqual(1, values.Count(m => m == ActivityParty_ParticipationTypeMask.Regarding), "The regarding activity party should have been created.");
        }


        [TestMethod]
        public void LocalCrmTests_Crud_Advanced()
        {
            // Verify that linked items exist upon create
            var service = GetService();
            var contact = new Contact { Id = Guid.NewGuid() };
            var opp = new Opportunity { ParentContactId = contact.ToEntityReference() };

            AssertOrganizationServiceFaultException("Opportunity Creation should have failed since the Contact Doesn't exist",
                                                    $"With Id = {contact.Id} Does Not Exist",
                                                    () => service.Create(opp));

            service.Create(contact);
            AssertCrm.Exists(service, contact);
            opp.Id = service.Create(opp);
            AssertCrm.Exists(service, opp);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_AlternateKey()
        {
            var service = GetService();
            var contact1 = new Contact { EMailAddress1 = "test1@test.com" };
            contact1.Id = service.Create(contact1);
            var contact2 = new Contact { EMailAddress1 = "test2@test.com" };
            contact2.Id = service.Create(contact2);
            var accountId = service.Create(new Account { PrimaryContactId = new EntityReference(Contact.EntityLogicalName, Contact.Fields.EMailAddress1, contact1.EMailAddress1) });

            Assert.AreEqual(contact1.Id, service.GetEntity<Account>(accountId).PrimaryContactId.Id, "Failed to create an account with a relationship to primary contact defined by alternate key");

            service.Update(new Account { Id = accountId, PrimaryContactId = new EntityReference(Contact.EntityLogicalName, Contact.Fields.EMailAddress1, contact2.EMailAddress1) });

            Assert.AreEqual(contact2.Id, service.GetEntity<Account>(accountId).PrimaryContactId.Id, "Failed to update an account with a relationship to primary contact defined by alternate key");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_AndOrConstraints()
        {
            var service = GetService();
            service.Create(new Contact { FirstName = "Abraham", LastName = "Lincoln" });
            service.Create(new Contact { FirstName = "George W", LastName = "Bush" });
            service.Create(new Contact { FirstName = "George H W", LastName = "Bush" });

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
        public void LocalCrmTests_Crud_Basic()
        {
            var c1 = new Contact { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Plumber" };
            var c2 = new Contact { Id = Guid.NewGuid(), FirstName = "Bill", LastName = "Carpenter" };
            var opp = new Opportunity { Id = Guid.NewGuid(), CustomerId = c1.ToEntityReference() };

            var service = GetService();
            service.Create(c1);
            service.Create(c2);
            service.Create(opp);

            Assert.IsNotNull(service.GetFirstOrDefault<Opportunity>(Opportunity.Fields.ParentContactId, c1.Id), "Failed Simple Lookup by Attribute Entity Reference");
            Assert.AreEqual(1, service.GetEntitiesById<Contact>(c1.Id).Count, "Failed Simple Where In Lookup by Id");

            // *** WIKI Start - DLaB.Xrm ***

            var qe = QueryExpressionFactory.Create<Opportunity>();
            // This AddLink will lookup the logical name of the Contact Entity when adding the link,
            //     and it will only return the FirstName attribute
            // If the joining attributes happened to be contactid for both, this could have just been written as :
            // qe.AddLink<Contact>(Contact.Fields.ContactId, c => new { c.FirstName });
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, Contact.Fields.ContactId, c => new { c.FirstName });
            var contact = service.GetFirstOrDefault(qe);

            // *** WIKI End - DLaB.Xrm ***

            Assert.IsNotNull(contact, "Failed Simple Lookup with Linked Entity on Entity Reference");
            Assert.AreEqual(c1.FirstName, contact.GetAliasedEntity<Contact>().FirstName, "Failed Simple Lookup retrieving Linked Entity columns");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_CustomerId()
        {
            var contact = new Id<Contact>(Guid.NewGuid());
            var account = new Id<Account>(Guid.NewGuid());
            var opp = new Opportunity { Id = Guid.NewGuid(), CustomerId = contact};
            var service = GetService();

            service.Create(contact);
            service.Create(account);
            service.Create(opp);

            Assert.AreEqual(contact.EntityId, service.GetFirstOrDefault<Opportunity>().ParentContactId.Id, "Contact should have been set to parent contact.");

            opp.CustomerId = account;
            service.Update(opp);

            opp = service.GetFirstOrDefault<Opportunity>();
            Assert.AreEqual(account.EntityId, opp.ParentAccountId.Id, "Account should have been set to parent account.");
            Assert.AreEqual(account.EntityId, opp.CustomerId.Id, "Account should have been set to customer.");

            opp.Attributes.Remove(Opportunity.Fields.CustomerId);
            opp.ParentAccountId = null;
            service.Update(opp);

            Assert.AreEqual(contact.EntityId, service.GetFirstOrDefault<Opportunity>().CustomerId.Id, "Contact should have been set to parent account.");

            opp.ParentContactId = null;
            service.Update(opp);

            Assert.IsNull(service.GetFirstOrDefault<Opportunity>().CustomerId, "Removing the Parent Contact Id should have set Customer to null.");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_CreateJoinEntity()
        {
            var dbInfo = LocalCrmDatabaseInfo.Create<CrmContext>();
            var service = new LocalCrmDatabaseOrganizationService(dbInfo);
            var info = service.GetCurrentlyExecutingUserInfo();
            var role = new Role
            {
                Name = nameof(LocalCrmTests_Crud_CreateJoinEntity)
            };
            role.Id = service.Create(role);
            service.Create(new SystemUserRoles
            {
                [SystemUserRoles.Fields.SystemUserId] = info.UserId,
                [SystemUserRoles.Fields.RoleId] = role.Id
            });
        }

        [TestMethod]
        public void LocalCrmTests_Crud_ColumnSetLookups()
        {
            var dbInfo = LocalCrmDatabaseInfo.Create<CrmContext>();
            var service = new LocalCrmDatabaseOrganizationService(dbInfo);
            const string firstName = "Joe";
            const string lastName = "Plumber";
            var contact = new Contact { FirstName = firstName, LastName = lastName };
            contact.Id = service.Create(contact);
            var cs = new ColumnSet("firstname");
            Assert.AreEqual(firstName, service.GetEntity<Contact>(contact.Id, cs).FirstName, "Failed to retrieve first name correctly");
            Assert.IsNull(service.GetEntity<Contact>(contact.Id, cs).LastName, "Last name was not requested, but was returned");
            Assert.AreEqual(firstName + " " + lastName, service.GetEntity<Contact>(contact.Id).FullName, "Full Name not populated correctly");

            // Test L, F M format
            dbInfo = LocalCrmDatabaseInfo.Create<CrmContext>(new LocalCrmDatabaseOptionalSettings
            {
                DatabaseName = nameof(LocalCrmTests_Crud_ColumnSetLookups),
                FullNameFormat = "L, F M"
            });
            service = new LocalCrmDatabaseOrganizationService(dbInfo);
            contact = new Contact {FirstName = firstName, LastName = lastName};
            contact.Id = service.Create(contact);
            Assert.AreEqual($"{lastName}, {firstName}", service.GetEntity<Contact>(contact.Id).FullName, "Full Name not populated correctly");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_LateBound()
        {
            var contact = new Contact();
            var lateContact = new Entity { LogicalName = Contact.EntityLogicalName };

            var service = GetService();
            contact.Id = service.Create(contact);
            lateContact.Id = service.Create(lateContact);

            Assert.IsNotNull(service.Retrieve(contact.LogicalName, contact.Id, new ColumnSet()), "Failed Create or Read");
            service.Execute(new RetrieveRequest
            {
                ColumnSet = new ColumnSet(),
                Target = contact.ToEntityReference()
            });
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
        public void LocalCrmTests_Crud_NestedJoins()
        {
            TestInitializer.InitializeTestSettings();
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            var user1 = new Id<SystemUser>(Guid.NewGuid()) {Entity = {FirstName = "Stan", }};
            var user2 = new Id<SystemUser>(Guid.NewGuid()) {Entity = {FirstName = "Steve"}};
            var account = new Id<Account>(Guid.NewGuid()) {Entity = {Name = "Marvel Comics"}};
            var contact1 = new Id<Contact>(Guid.NewGuid()) {Entity = {FirstName = "Bruce", CreatedOnBehalfBy = user1, ModifiedOnBehalfBy = user2 } };
            var contact2 = new Id<Contact>(Guid.NewGuid()) {Entity = {FirstName = "Peter", CreatedOnBehalfBy = user2, ModifiedOnBehalfBy = user1 } };

            var builder = new DLaBCrmEnvironmentBuilder().
                          WithChildEntities(account, contact1, contact2).
                          WithEntities(user1, user2);
            builder.Create(service);

            var temp = service.GetEntity(contact1);
            Assert.AreEqual(account.EntityReference, temp.ParentCustomerId);
            Assert.AreEqual(user1.EntityReference, temp.CreatedOnBehalfBy);
            Assert.AreEqual(user2.EntityReference, temp.ModifiedOnBehalfBy);
            temp = service.GetEntity(contact2);
            Assert.AreEqual(account.EntityReference, temp.ParentCustomerId);
            Assert.AreEqual(user1.EntityReference, temp.ModifiedOnBehalfBy);
            Assert.AreEqual(user2.EntityReference, temp.CreatedOnBehalfBy);

            var qe = QueryExpressionFactory.Create<Account>(a => new {a.Name});
            var contactLink = qe.AddLink<Contact>(Account.Fields.Id, Contact.Fields.ParentCustomerId, c => new { c.FirstName, c.Id });
            var createdLink = contactLink.AddLink<SystemUser>(Contact.Fields.CreatedOnBehalfBy, SystemUser.Fields.Id, u => new { u.FirstName, u.Id });
            var modifiedLink = contactLink.AddLink<SystemUser>(Contact.Fields.ModifiedOnBehalfBy, SystemUser.Fields.Id, u => new { u.FirstName, u.Id });
            contactLink.EntityAlias = "ContactLink";
            createdLink.EntityAlias = "CreatedLink";
            modifiedLink.EntityAlias = "ModifiedLink";
            var results = service.RetrieveMultiple(qe);

            Assert.AreEqual(2, results.Entities.Count);
            foreach (var entity in results.Entities)
            {
                Assert.IsTrue(entity.Attributes.ContainsKey("CreatedLink.firstname"));
                Assert.IsTrue(entity.Attributes.ContainsKey("ModifiedLink.firstname"));
            }
        }

        [TestMethod]
        public void LocalCrmTests_Crud_OrderBy()
        {
            var service = GetService();
            service.Create(new Contact { FirstName = "Chuck", LastName = "Adams" });
            service.Create(new Contact { FirstName = "Anna", LastName = "Adams" });
            service.Create(new Contact { FirstName = "Bill", LastName = "Adams" });

            var qe = new QueryExpression(Contact.EntityLogicalName) { ColumnSet = new ColumnSet(true) };

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
            service.Create(new Contact { FirstName = "Chuck", LastName = "Bell" });
            service.Create(new Contact { FirstName = "Anna", LastName = "Bell" });
            service.Create(new Contact { FirstName = "Anna", LastName = "Carter" });
            service.Create(new Contact { FirstName = "Bill", LastName = "Bell" });
            service.Create(new Contact { FirstName = "Bill", LastName = "Carter" });
            service.Create(new Contact { FirstName = "Chuck", LastName = "Carter" });

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
        public void LocalCrmTests_Crud_QueryByAttribute()
        {
            var service = GetService();
            var johnId = service.Create(new Contact { FirstName = "John" });
            // ReSharper disable once UnusedVariable
            var janeId = service.Create(new Contact { FirstName = "Jane" });

            // Happy Path
            var query = new QueryByAttribute(Contact.EntityLogicalName) { ColumnSet = new ColumnSet() };
            query.ColumnSet.AddColumn(Contact.Fields.ContactId);
            query.Attributes.Add(Contact.Fields.FirstName);
            query.Values.Add("John");
            var contacts = service.RetrieveMultiple(query).ToEntityList<Contact>();
            Assert.AreEqual(1, contacts.Count);
            Assert.AreEqual(johnId, contacts[0].Id);

            // Unhappy Path, uneven values / attributes
            query.Values.Clear();
            AssertOrganizationServiceFaultException("QueryByAttribute had an uneven number of attributes/values, and should have thrown an exception",
                                                    ErrorCodes.GetErrorMessage(ErrorCodes.QueryBuilderByAttributeMismatch),
                                                    () => service.RetrieveMultiple(query));

            query.Attributes.Clear();
            AssertOrganizationServiceFaultException("QueryByAttribute had no attributes or values, and should have thrown an exception",
                                                    ErrorCodes.GetErrorMessage(ErrorCodes.QueryBuilderByAttributeNonEmpty),
                                                    () => service.RetrieveMultiple(query));
        }

        [TestMethod]
        public void LocalCrmTests_Crud_RetrieveById()
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
        public void LocalCrmTests_Crud_MultipleOr()
        {
            TestInitializer.InitializeTestSettings();
            var service = GetService();
            const string telephone = "9998881111";
            service.Create(new Account {Telephone1 = telephone});
            var qe = QueryExpressionFactory.Create<Account>();
            TestForPhoneNumber(service, qe, qe.Criteria, telephone);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_WhereCrmDataTypes()
        {
            var service = GetService();
            var campaignId = service.Create(new Campaign());
            var opportunity = new Opportunity
            {
                ActualValue = new Money(10m),
                BudgetStatusEnum = BudgetStatus.CanBuy,
                CampaignId = new EntityReference(Campaign.EntityLogicalName, campaignId),
                SendThankYouNote = true,
                StageId = Guid.NewGuid(),
                ActualCloseDate = DateTime.UtcNow,
                CloseProbability = 10,
                DiscountPercentage = .05m,
                InitialCommunication = new OptionSetValue(1)
            };
            var oppId = service.Create(opportunity);
            service.Create(new Opportunity());

            var dbOpportunity = service.GetFirst<Opportunity>(
                new ConditionExpression(Opportunity.Fields.ActualValue, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.BudgetStatus, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.CampaignId, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.SendThankYouNote, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.StageId, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.ActualCloseDate, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.CloseProbability, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.DiscountPercentage, ConditionOperator.NotNull),
                new ConditionExpression(Opportunity.Fields.InitialCommunication, ConditionOperator.NotNull));

            Assert.AreEqual(oppId, dbOpportunity.Id);

            dbOpportunity = service.GetFirst<Opportunity>(
                Opportunity.Fields.ActualCloseDate, opportunity.ActualCloseDate.GetValueOrDefault(),
                Opportunity.Fields.ActualValue, opportunity.ActualValue.GetValueOrDefault(),
                Opportunity.Fields.BudgetStatus, opportunity.BudgetStatus.GetValueOrDefault(),
                Opportunity.Fields.CampaignId, opportunity.CampaignId.GetIdOrDefault(),
                Opportunity.Fields.SendThankYouNote, opportunity.SendThankYouNote.GetValueOrDefault(),
                Opportunity.Fields.StageId, opportunity.StageId.GetValueOrDefault(),
                Opportunity.Fields.CloseProbability, opportunity.CloseProbability.GetValueOrDefault(),
                Opportunity.Fields.DiscountPercentage, opportunity.DiscountPercentage.GetValueOrDefault(),
                Opportunity.Fields.InitialCommunication, opportunity.InitialCommunication.GetValueOrDefault());

            Assert.AreEqual(oppId, dbOpportunity.Id);

            dbOpportunity = service.GetFirst<Opportunity>(Opportunity.Fields.CampaignId, opportunity.CampaignId.Id.ToString());
            Assert.AreEqual(oppId, dbOpportunity.Id);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_WhereIn()
        {
            var service = GetService();
            service.Create(new Contact { FirstName = "firstname", LastName= "lastname" });
            service.Create(new Contact { FirstName = "FIRSTNAME", LastName = "LASTNAME" });

            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.FirstName, "FirstName").Count, 2, "Where In Should be case insensitive!");
            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.FirstName, "FIRSTNAME").Count, 2, "Where In Should be case insensitive!");
            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.FirstName, "firstname").Count, 2, "Where In Should be case insensitive!");
            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.LastName, "LastName").Count, 2, "Where In Should be case insensitive!");
            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.LastName, "LASTNAME").Count, 2, "Where In Should be case insensitive!");
            Assert.AreEqual(service.GetEntitiesIn<Contact>(Contact.Fields.LastName, "lastname").Count, 2, "Where In Should be case insensitive!");

            var condition = new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.NotIn, "FirstName");
            var qe = QueryExpressionFactory.Create<Contact>().WhereEqual(condition);
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "FIRSTNAME";
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "firstname";
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "firstname1";
            Assert.AreEqual(service.GetEntities(qe).Count, 2, "NotIn Should be case insensitive!");

            condition.AttributeName = Contact.Fields.LastName;
            condition.Values[0] = "LastName";
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "LASTNAME";
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "lastname";
            Assert.AreEqual(service.GetEntities(qe).Count, 0, "NotIn Should be case insensitive!");
            condition.Values[0] = "lastname1";
            Assert.AreEqual(service.GetEntities(qe).Count, 2, "NotIn Should be case insensitive!");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_LinkedMultipleOr()
        {
            TestInitializer.InitializeTestSettings();
            var service = GetService();
            const string telephone = "9998882222";
            var id = service.Create(new Account
            {
                Telephone1 = telephone
            });
            service.Create(new Contact
            {
                ParentCustomerId = new EntityReference(Account.EntityLogicalName, id)
            });

            var qe = QueryExpressionFactory.Create<Contact>();

            TestForPhoneNumber(service, qe, qe.AddLink<Account>(Contact.Fields.ParentCustomerId, Account.Fields.Id).LinkCriteria, telephone);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_Associate()
        {
            var service = GetService();
            var account1Id = service.Create(new Account());
            var account2Id = service.Create(new Account());
            var account3Id = service.Create(new Account());
            var leadId = service.Create(new Lead());

            var relatedEntities = new EntityReferenceCollection
            {
                new EntityReference("account", account1Id),
                new EntityReference("account", account2Id),
                new EntityReference("account", account3Id)
            };

            // Create an object that defines the relationship between the contact and account.
            var relationship = new Relationship(AccountLeads.EntityLogicalName);

            //Associate the contact with the 3 accounts.
            service.Associate("lead", leadId, relationship, relatedEntities);

            var joinEntities = service.GetEntities(AccountLeads.EntityLogicalName);
            Assert.AreEqual(3, joinEntities.Count, "3 N:N records should have been created!");

            var qe = QueryExpressionFactory.Create<Lead>();
            qe.AddLink(AccountLeads.EntityLogicalName, Lead.Fields.LeadId)
              .WhereEqual(AccountLeads.Fields.AccountId, account1Id);
            var leads = service.GetEntities(qe);
            Assert.AreEqual(1, leads.Count, "1 N:N records should have been created!");

            qe = QueryExpressionFactory.Create<Lead>();
            qe.AddLink(AccountLeads.EntityLogicalName, Lead.Fields.LeadId);
            leads = service.GetEntities(qe);
            Assert.AreEqual(3, leads.Count, "3 N:N records should have been created!");

            //Disassociate the contact with the 3 accounts.
            service.Disassociate("lead", leadId, relationship, relatedEntities);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_ConnectionConstraints()
        {
            var to = new Id<Contact>("458DB1CB-12F8-40FD-BCEF-DCDACAEB10D8");
            var toRole = new Id<ConnectionRole>("D588F8F5-9276-471E-9A73-C62217C29FD1");
            var fromRole = new Id<ConnectionRole>("29E71A5B-692D-4777-846D-FD1687D7DDB7");
            var connection = new Id<Connection>("AC56E429-452F-49F5-A463-894E8CA8E17C");
            connection.Inject(new Connection
            {
                Record1Id = to,
                Record1RoleId = toRole
            });
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));
            service.Create(to);
            service.Create(toRole);
            service.Create(fromRole);
            var containsFailure = false;
            try
            {
                service.Create(connection);
            }
            catch (Exception ex)
            {
                containsFailure = ex.ToString().Contains("You must provide a name or select a role for both sides of this connection.");
                if (!containsFailure)
                {
                    throw;
                }
            }

            Assert.IsTrue(containsFailure, "The Create should have failed with a You must provide a name or select a role for both sides of this connection., message.");
            connection.Entity.Record2RoleId = fromRole;
            containsFailure = false;

            try
            {
                service.Create(connection);
            }
            catch (Exception ex)
            {
                containsFailure = ex.ToString().Contains("The connection roles are not related");
                if (!containsFailure)
                {
                    throw;
                }
            }

            Assert.IsTrue(containsFailure, "The Create should have failed with a The connection roles are not related, message.");

            service.Associate(toRole, toRole, new Relationship("connectionroleassociation_association"), new EntityReferenceCollection(new List<EntityReference>
            {
                fromRole.EntityReference
            }));
            service.Create(connection);
        }

        #region Shared Methods


        private static void TestForPhoneNumber(IOrganizationService service, QueryExpression qe, FilterExpression accountFilter, string telephone)
        {
            accountFilter.WhereEqual(
                Account.Fields.Telephone1, telephone,
                LogicalOperator.Or,
                Account.Fields.Telephone2, telephone,
                LogicalOperator.Or,
                Account.Fields.Telephone3, telephone
                );

            var entity = service.GetFirstOrDefault(qe);
            Assert.IsNotNull(entity, $"{qe.EntityName} should have been found with matching telephone 1 number.");

            var account = service.GetFirst<Account>();
            account.Telephone2 = telephone;
            account.Telephone1 = telephone + "1";
            service.Update(account);
            entity = service.GetFirstOrDefault(qe);
            Assert.IsNotNull(entity, $"{qe.EntityName} should have been found with matching telephone 2 number.");

            account.Telephone3 = telephone;
            account.Telephone2 = telephone + "2";
            service.Update(account);
            entity = service.GetFirstOrDefault(qe);
            Assert.IsNotNull(entity, $"{qe.EntityName} should have been found with matching telephone 3 number.");

            service = new OrganizationServiceBuilder(service)
                .WithEntityFilter<Account>(account.Id).Build();
            entity = service.GetFirstOrDefault(qe);
            Assert.IsNotNull(entity, $"{qe.EntityName} should have been found with matching telephone 3 number and Id.");

            account.Telephone3 += "A";
            service.Update(account);
            entity = service.GetFirstOrDefault(qe);
            Assert.IsNull(entity, $"{qe.EntityName} should have not have been found with a non-matching telephone 3 number and Id.");
        }


        #endregion Shared Methods
    }
}
