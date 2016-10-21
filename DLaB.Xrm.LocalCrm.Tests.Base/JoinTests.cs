using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class JoinTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Join_RetrieveFilterOnOuterJoinedColumn()
        {
            var service = GetService();
            var contact1 = new Contact { FirstName = "Joe" };
            contact1.Id = service.Create(contact1);
            var contact2 = new Contact { FirstName = "Jim" };
            contact2.Id = service.Create(contact2);
            var contact3 = new Contact { FirstName = "Jake" };
            contact3.Id = service.Create(contact3);

            service.Create(new Opportunity { CustomerId = contact1.ToEntityReference() });
            service.Create(new Opportunity { CustomerId = contact2.ToEntityReference() });
            service.Create(new Opportunity { CustomerId = contact3.ToEntityReference() });
            service.Create(new Opportunity());

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, Contact.Fields.ContactId, JoinOperator.LeftOuter, c => new { c.FirstName }).EntityAlias = "MyAlias";
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
        public void LocalCrmTests_Join_RetrieveOuterJoinedColumn()
        {
            var service = GetService();
            var contact = new Contact { FirstName = "Joe" };
            contact.Id = service.Create(contact);

            // Create 2 opportunities
            service.Create(new Opportunity { CustomerId = contact.ToEntityReference() });
            service.Create(new Opportunity());

            var qe = QueryExpressionFactory.Create<Opportunity>();
            qe.AddLink<Contact>(Opportunity.Fields.ParentContactId, Contact.Fields.ContactId, JoinOperator.LeftOuter, c => new { c.FirstName });

            var entities = service.GetEntities(qe);
            Assert.AreEqual(2, entities.Count, "Two opportunities should have been returned!");
            Assert.AreEqual(contact.FirstName, entities.First(o => o.ParentContactId != null).GetAliasedEntity<Contact>().FirstName, "First Name wasn't returned!");
            Assert.IsNull(entities.First(o => o.ParentContactId == null).GetAliasedEntity<Contact>().FirstName, "Second Opportunity some how has a contact!");
        }
    }
}
