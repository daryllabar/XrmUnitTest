using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class JoinTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Join_RetrieveFilterOnOuterJoinedColumn()
        {
            var service = Service;
            var contact1 = service.CreateEntity(new Contact { FirstName = "Joe" });
            var contact2 = service.CreateEntity(new Contact { FirstName = "Jim" });
            var contact3 = service.CreateEntity(new Contact { FirstName = "Jake" });

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
            var service = Service;
            var contact = service.CreateEntity(new Contact { FirstName = "Joe" });

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
            var service = Service;
            var user = service.CreateEntity(new SystemUser { FirstName = "Joe" });
            var role = service.CreateEntity(new Role { Name = "Test Role" });
            var roleUser = AddRole(service, user, role);

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

        private static SystemUserRoles AddRole(IOrganizationService service, SystemUser user, Role role)
        {
            return service.CreateEntity(new SystemUserRoles { [SystemUserRoles.Fields.SystemUserId] = user.ToEntityReference(), [SystemUserRoles.Fields.RoleId] = role.ToEntityReference() });
        }

        [TestMethod]
        public void LocalCrmTests_Join_NotAny_ReturnsRecordsWithNoMatchingRelated()
        {
            var service = Service;
            var userWithRole = service.CreateEntity(new SystemUser { FirstName = "WithRole" });
            var userWithoutRole = service.CreateEntity(new SystemUser { FirstName = "WithoutRole" });
            var role = service.CreateEntity(new Role { Name = "SomeRole" });
            AddRole(service, userWithRole, role);

            var qe = QueryExpressionFactory.Create<SystemUser>();
            qe.WhereIn(SystemUser.Fields.SystemUserId, userWithRole.Id, userWithoutRole.Id);
            qe.AddLink<SystemUserRoles>(SystemUser.Fields.SystemUserId, JoinOperator.NotAny);

            var results = service.GetEntities(qe);
            Assert.HasCount(1, results, "Only the user without any role should be returned.");
            Assert.AreEqual(userWithoutRole.Id, results[0].Id);
        }

        /// <summary>
        /// Verifies the anti-semi-join (NOT EXISTS) behaviour of <see cref="JoinOperator.NotAny"/>
        /// with a nested <see cref="JoinOperator.Inner"/> link.
        ///
        /// Scenario mirrors the issue description:
        ///   User1 ? RoleA only
        ///   User2 ? RoleB only
        ///   User3 ? RoleA AND RoleB
        ///   User4 ? no roles
        ///
        /// Query: return users with NO role whose Name = "RoleB".
        /// Expected: User1 and User4 (User2 and User3 each have RoleB, so they are excluded).
        /// </summary>
        [TestMethod]
        public void LocalCrmTests_Join_NotAny_WithNestedInner_ExcludesRecordsWithAnyMatch()
        {
            var service = Service;

            var roleAOnly = service.CreateEntity(new SystemUser { FirstName = "RoleA only" });
            var roleBOnly = service.CreateEntity(new SystemUser { FirstName = "RoleB only" });
            var roleAAndRoleB = service.CreateEntity(new SystemUser { FirstName = "RoleA and RoleB" });
            var noRoles = service.CreateEntity(new SystemUser { FirstName = "No roles" });
            var roleA = service.CreateEntity(new Role { Name = "RoleA" });
            var roleB = service.CreateEntity(new Role { Name = "RoleB" });

            AddRole(service, roleAOnly, roleA);
            AddRole(service, roleBOnly, roleB);
            AddRole(service, roleAAndRoleB, roleA);
            AddRole(service, roleAAndRoleB, roleB);

            var qe = QueryExpressionFactory.Create<SystemUser>();
            qe.WhereIn(SystemUser.Fields.SystemUserId, roleAOnly.Id, roleBOnly.Id, roleAAndRoleB.Id, noRoles.Id);
            qe.AddLink<SystemUserRoles>(SystemUser.Fields.SystemUserId, JoinOperator.NotAny)
                .AddLink<Role>(SystemUserRoles.Fields.RoleId, JoinOperator.Inner, r => new { r.Name })
                .WhereEqual(Role.Fields.Name, "RoleB");

            var results = service.GetEntities(qe);

            Assert.HasCount(2, results, "'RoleA only' and 'No roles' should be returned.");
            var ids = results.Select(u => u.Id).ToHashSet();
            Assert.Contains(roleAOnly.Id, ids, "'RoleA only' should be returned.");
            Assert.Contains(noRoles.Id, ids, "'No roles' should be returned.");
            Assert.DoesNotContain(roleBOnly.Id, ids, "'RoleB only' should NOT be returned.");
            Assert.DoesNotContain(roleAAndRoleB.Id, ids, "'RoleA and RoleB' should NOT be returned.");
        }
    }
}
