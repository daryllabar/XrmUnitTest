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

            Assert.HasCount(1, entities, "Only Joe opportunities should have been returned!");
            Assert.AreEqual(contact1.FirstName, entities[0].GetAliasedEntity<Contact>().FirstName);

            qe.Criteria.AddCondition("MyAlias", Contact.Fields.FirstName, ConditionOperator.Equal, "Jim");
            qe.Criteria.FilterOperator = LogicalOperator.Or;
            entities = service.GetEntities(qe);

            Assert.HasCount(2, entities, "Joe and Jim opportunities should have been returned!");
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
            Assert.HasCount(2, entities, "Two opportunities should have been returned!");
            Assert.AreEqual(contact.FirstName, entities.First(o => o.ParentContactId != null).GetAliasedEntity<Contact>().FirstName, "First Name wasn't returned!");
            Assert.IsNull(entities.First(o => o.ParentContactId == null).GetAliasedEntity<Contact>().FirstName, "Second Opportunity some how has a contact!");
        }

        [TestMethod]
        public void LocalCrmTests_MultipleJoinWithNull_ShouldReturnNull()
        {
            var service = GetService();
            var user = new SystemUser { FirstName = "Joe" };
            user.Id = service.Create(user);

            var role = new Role { Name = "Test Role" };
            role.Id = service.Create(role);

            var roleUser = new SystemUserRoles { ["systemuserid"] = user.ToEntityReference(), ["roleid"] = role.ToEntityReference() };

            roleUser.Id = service.Create(roleUser);

            var qe = QueryExpressionFactory.Create<SystemUser>();
            qe.WhereEqual(SystemUser.Fields.SystemUserId, user.Id);
            qe.AddLink<SystemUserRoles>(SystemUser.Fields.SystemUserId, JoinOperator.LeftOuter)
                .AddLink<Role>(SystemUserRoles.Fields.RoleId, JoinOperator.LeftOuter, r => new { r.Name });

            // Verify that the Links work and that the Role Name is being returned
            var userResult = service.GetFirst(qe);
            Assert.AreEqual(user.FirstName, userResult.FirstName);
            Assert.AreEqual(role.Name, userResult.GetAliasedEntity<Role>().Name);

            // Remove RoleId from the SystemUserRoles record and verify that the Role Name is now null
            roleUser["roleid"] = null;
            service.Update(roleUser);
            service.Delete(role);
            userResult = service.GetFirst(qe);
            Assert.AreEqual(user.FirstName, userResult.FirstName);
            Assert.IsNull(userResult.GetAliasedEntity<Role>().Name);

            // Remove the SystemUserRoles record and verify that the Role Name is still null
            service.Delete(roleUser);
            userResult = service.GetFirst(qe);
            Assert.AreEqual(user.FirstName, userResult.FirstName);
            Assert.IsNull(userResult.GetAliasedEntity<Role>().Name);
        }
    }
}
