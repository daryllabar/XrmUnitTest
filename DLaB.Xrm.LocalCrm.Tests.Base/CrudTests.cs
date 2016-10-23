using System;
using DLaB.Xrm.CrmSdk;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class CrudTests : BaseTestClass
    {
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

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, "contactid", c => new { c.FirstName });

            var otherC = service.GetFirstOrDefault(qe);

            Assert.IsNotNull(otherC, "Failed Simple Lookup with Linked Entity on Entity Reference");
            Assert.AreEqual(c1.FirstName, otherC.GetAliasedEntity<Contact>().FirstName, "Failed Simple Lookup retrieving Linked Entity columns");
        }

        [TestMethod]
        public void LocalCrmTests_Crud_ColumnSetLookups()
        {
            var service = GetService();
            const string firstName = "Joe";
            const string lastName = "Plumber";
            var contact = new Contact { FirstName = firstName, LastName = lastName };
            contact.Id = service.Create(contact);
            var cs = new ColumnSet("firstname");
            Assert.AreEqual(firstName, service.GetEntity<Contact>(contact.Id, cs).FirstName, "Failed to retrieve first name correctly");
            Assert.IsNull(service.GetEntity<Contact>(contact.Id, cs).LastName, "Last name was not requested, but was returned");
            Assert.AreEqual(firstName + " " + lastName, service.GetEntity<Contact>(contact.Id).FullName, "Full Name not populated correctly");
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
            var service = GetService();
            const string telephone = "9998881111";
            service.Create(new Account {Telephone1 = telephone});
            var qe = QueryExpressionFactory.Create<Account>();
            TestForPhoneNumber(service, qe, qe.Criteria, telephone);
        }

        [TestMethod]
        public void LocalCrmTests_Crud_LinkedMultipleOr()
        {
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
