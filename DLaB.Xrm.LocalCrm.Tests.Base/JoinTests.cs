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

        [TestMethod]
        public void LocalCrmTests_Join_NotAny_ReturnsRecordsWithNoMatchingRelated()
        {
            var service = GetService();

            var userWithRole = new SystemUser { FirstName = "WithRole" };
            userWithRole.Id = service.Create(userWithRole);
            var userWithoutRole = new SystemUser { FirstName = "WithoutRole" };
            userWithoutRole.Id = service.Create(userWithoutRole);

            var role = new Role { Name = "SomeRole" };
            role.Id = service.Create(role);
            service.Create(new SystemUserRoles
            {
                ["systemuserid"] = userWithRole.ToEntityReference(),
                ["roleid"] = role.ToEntityReference()
            });

            var qe = QueryExpressionFactory.Create<SystemUser>();
            qe.Criteria.AddCondition(SystemUser.Fields.SystemUserId, ConditionOperator.In,
                userWithRole.Id, userWithoutRole.Id);

            // NotAny: users that have NO SystemUserRoles record at all.
            var notAnyLink = new LinkEntity
            {
                JoinOperator = JoinOperator.NotAny,
                LinkFromEntityName = SystemUser.EntityLogicalName,
                LinkFromAttributeName = SystemUser.Fields.SystemUserId,
                LinkToEntityName = SystemUserRoles.EntityLogicalName,
                LinkToAttributeName = SystemUserRoles.Fields.SystemUserId,
                EntityAlias = "notany_sur"
            };
            qe.LinkEntities.Add(notAnyLink);

            var results = service.GetEntities(qe);

            Assert.HasCount(1, results, "Only the user without any role should be returned.");
            Assert.AreEqual(userWithoutRole.FirstName, results[0].FirstName);
        }

        /// <summary>
        /// Verifies the anti-semi-join (NOT EXISTS) behaviour of <see cref="JoinOperator.NotAny"/>
        /// with a nested <see cref="JoinOperator.Inner"/> link.
        ///
        /// Scenario mirrors the issue description:
        ///   User1 → RoleA only
        ///   User2 → RoleB only
        ///   User3 → RoleA AND RoleB
        ///   User4 → no roles
        ///
        /// Query: return users with NO role whose Name = "RoleB".
        /// Expected: User1 and User4 (User2 and User3 each have RoleB, so they are excluded).
        /// </summary>
        [TestMethod]
        public void LocalCrmTests_Join_NotAny_WithNestedInner_ExcludesRecordsWithAnyMatch()
        {
            var service = GetService();

            var user1 = new SystemUser { FirstName = "User1" }; // RoleA only
            user1.Id = service.Create(user1);
            var user2 = new SystemUser { FirstName = "User2" }; // RoleB only
            user2.Id = service.Create(user2);
            var user3 = new SystemUser { FirstName = "User3" }; // RoleA AND RoleB
            user3.Id = service.Create(user3);
            var user4 = new SystemUser { FirstName = "User4" }; // no roles
            user4.Id = service.Create(user4);

            var roleA = new Role { Name = "RoleA" };
            roleA.Id = service.Create(roleA);
            var roleB = new Role { Name = "RoleB" };
            roleB.Id = service.Create(roleB);

            service.Create(new SystemUserRoles { ["systemuserid"] = user1.ToEntityReference(), ["roleid"] = roleA.ToEntityReference() });
            service.Create(new SystemUserRoles { ["systemuserid"] = user2.ToEntityReference(), ["roleid"] = roleB.ToEntityReference() });
            service.Create(new SystemUserRoles { ["systemuserid"] = user3.ToEntityReference(), ["roleid"] = roleA.ToEntityReference() });
            service.Create(new SystemUserRoles { ["systemuserid"] = user3.ToEntityReference(), ["roleid"] = roleB.ToEntityReference() });

            var qe = QueryExpressionFactory.Create<SystemUser>();
            qe.Criteria.AddCondition(SystemUser.Fields.SystemUserId, ConditionOperator.In,
                user1.Id, user2.Id, user3.Id, user4.Id);

            // NotAny: users with NO SystemUserRoles that links (via Inner) to a Role named "RoleB".
            var notAnyLink = new LinkEntity
            {
                JoinOperator = JoinOperator.NotAny,
                LinkFromEntityName = SystemUser.EntityLogicalName,
                LinkFromAttributeName = SystemUser.Fields.SystemUserId,
                LinkToEntityName = SystemUserRoles.EntityLogicalName,
                LinkToAttributeName = SystemUserRoles.Fields.SystemUserId,
                EntityAlias = "notany_sur"
            };

            var innerRoleLink = new LinkEntity
            {
                JoinOperator = JoinOperator.Inner,
                LinkFromEntityName = SystemUserRoles.EntityLogicalName,
                LinkFromAttributeName = SystemUserRoles.Fields.RoleId,
                LinkToEntityName = Role.EntityLogicalName,
                LinkToAttributeName = Role.Fields.RoleId,
                EntityAlias = "notany_role"
            };
            innerRoleLink.LinkCriteria.AddCondition(Role.Fields.Name, ConditionOperator.Equal, "RoleB");
            notAnyLink.LinkEntities.Add(innerRoleLink);
            qe.LinkEntities.Add(notAnyLink);

            var results = service.GetEntities(qe);

            Assert.HasCount(2, results, "User1 (RoleA only) and User4 (no roles) should be returned.");
            var firstNames = results.Select(u => u.FirstName).ToHashSet();
            Assert.IsTrue(firstNames.Contains("User1"), "User1 (RoleA only) should be returned.");
            Assert.IsTrue(firstNames.Contains("User4"), "User4 (no roles) should be returned.");
            Assert.IsFalse(firstNames.Contains("User2"), "User2 (has RoleB) should NOT be returned.");
            Assert.IsFalse(firstNames.Contains("User3"), "User3 (has RoleA and RoleB) should NOT be returned.");
        }
    }
}
