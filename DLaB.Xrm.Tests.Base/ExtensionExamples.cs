using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmUnitTest.Test;
using DLaB.Xrm.Test.Builders;

namespace DLaB.Xrm.Tests
{
    /// <summary>
    /// Shows Multiple Examples of the how to use extension methods to clean up and simplify code
    /// </summary>
    [TestClass]
    public class ExtensionExamples
    {
        [TestMethod]
        public void ExtensionExamples_QueryExpressionSimplification()
        {
            var initial = GetStandardComplicatedQuery();
            Assert.AreEqual(initial.GetSqlStatement(), GetComplicatedStraightConvertedQuery().GetSqlStatement());
            Assert.AreEqual(initial.GetSqlStatement(), GetComplicatedUltraSimplifiedQuery().GetSqlStatement());
        }

        [TestMethod]
        public void ExtensionExamples_WhereEqual()
        {
            var qe = new QueryExpression();
            qe.Criteria.AddCondition("att1", ConditionOperator.Equal, "Value");
            qe.Criteria.AddCondition("att2", ConditionOperator.Equal, "Value");
            qe.Criteria.AddCondition("att3", ConditionOperator.Equal, "Value");

            var qe2 = new QueryExpression();
            qe2.WhereEqual(
                "att1", "Value",
                "att2", "Value",
                "att3", "Value");

            Assert.AreEqual(qe.GetSqlStatement(), qe2.GetSqlStatement());
        }

        [TestMethod]
        public void ExtensionExamples_WhereEqualOr()
        {
            var qe = new QueryExpression();
            qe.Criteria.AddCondition("att3", ConditionOperator.Equal, "value");
            var orFilter = new FilterExpression(LogicalOperator.Or);
            qe.Criteria.AddFilter(orFilter);

            var filter1 = new FilterExpression();
            filter1.AddCondition("att1", ConditionOperator.Equal, "Value");
            filter1.AddCondition("att2", ConditionOperator.Equal, "Value");
            orFilter.AddFilter(filter1);

            var filter2 = new FilterExpression();
            filter2.AddCondition("att1", ConditionOperator.Equal, "Value1");
            filter2.AddCondition("att2", ConditionOperator.Equal, "Value1");
            orFilter.AddFilter(filter2);

            var qe2 = new QueryExpression();
            qe2.WhereEqual("att3", "value");
            qe2.WhereEqual(
                "att1", "Value",
                "att2", "Value",
                LogicalOperator.Or,
                "att1", "Value1",
                "att2", "Value1");

            Assert.AreEqual(qe.GetSqlStatement(), qe2.GetSqlStatement());
        }

        [TestMethod]
        public void ExtensionExamples_ActiveOnly()
        {
            var qe = new QueryExpression();
            qe.Criteria.ActiveOnly<Contact>();

            var activeOnly = qe.Criteria.Conditions.First();
            Assert.AreEqual(Contact.Fields.StateCode, activeOnly.AttributeName);
            Assert.AreEqual(ConditionOperator.Equal, activeOnly.Operator);
            Assert.AreEqual((int)ContactState.Active, activeOnly.Values.First());

        }

        #region GetEntityExamples

        [TestMethod]
        public void ExtensionExamples_GetEntityExamples()
        {
            new GetEntityExamples().Test();
        }

        private class GetEntityExamples : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Contact> Contact = new Id<Contact>("5E3CFF3B-616A-436E-A93A-A7F89DE0505F");

                public struct Leads
                {
                    public static readonly Id<Lead> A = new Id<Lead>("BEE8D759-ACF7-4F8E-A63D-F4BD2E27FD1E");
                    public static readonly Id<Lead> B = new Id<Lead>("C8326EF8-5EE3-43BA-8808-25D1F1E0B52A");
                    public static readonly Id<Lead> C = new Id<Lead>("5D572F13-A53E-481B-90C9-207312F1E51B");
                    public static readonly Id<Lead> D = new Id<Lead>("C4619226-9207-4079-B91A-45576BB88DCE");
                }
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.Leads.A.Entity.Subject = "A";
                Ids.Leads.B.Entity.Subject = "B";
                Ids.Leads.C.Entity.Subject = "C";
                Ids.Leads.D.Entity.Subject = "D";
                Ids.Leads.D.Entity.CustomerId = Ids.Contact;
                Ids.Contact.Entity.EMailAddress1 = "test@test.com";
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                GetFirstVsGetFirstOrDefault(service);
                GetEntities(service);
                GetEntitiesIn(service);
                GetAllEntities(service);
                GetAliasedEntity(service);
            }

            private void GetFirstVsGetFirstOrDefault(IOrganizationService service)
            {
                var contact = service.GetFirst<Contact>();
                Assert.AreEqual(Ids.Contact.EntityId, contact.Id);

                contact = service.GetFirst<Contact>(c => new {c.FullName});
                Assert.IsNotNull(contact.FullName);

                contact = service.GetFirstOrDefault<Contact>(
                    c =>
                        new {c.FullName},
                    Contact.Fields.Address1_StateOrProvince,
                    "Iowa");
                Assert.IsNull(contact);
                try
                {
                    service.GetFirst<Contact>(Contact.Fields.Address1_StateOrProvince, "Iowa");
                }
                catch (Exception ex)
                {
                    var message = ex.ToString();
                    Assert.IsTrue(message.Contains("Iowa"));
                    return;
                }
                Assert.Fail();
            }

            private void GetEntities(IOrganizationService service)
            {
                var leads = service.GetEntities<Lead>(new ConditionExpression(Lead.Fields.Subject, ConditionOperator.NotEqual, "B"));
                Assert.AreEqual(3, leads.Count);
                Assert.IsFalse(leads.Any(l => l.Subject == "B"));

                leads = service.GetEntities<Lead>(
                    new ColumnSet(Lead.Fields.Subject),
                    Lead.Fields.Subject,
                    "A",
                    LogicalOperator.Or,
                    Lead.Fields.Subject,
                    "B",
                    LogicalOperator.Or,
                    Lead.Fields.Subject,
                    "D");

                Assert.AreEqual(3, leads.Count);
                Assert.IsFalse(leads.Any(l => l.Subject == "C"));
            }

            private void GetEntitiesIn(IOrganizationService service)
            {
                var leads = service.GetEntitiesIn<Lead>(Lead.Fields.Subject, "A", "B");
                Assert.AreEqual(2, leads.Count);
                Assert.IsFalse(leads.Any(l => l.Subject == "C" || l.Subject == "D"));
            }

            private static void GetAllEntities(IOrganizationService service)
            {
                var count = 0;
                foreach (var lead in service.GetAllEntities<Lead>())
                {
                    Assert.IsNotNull(lead.Subject);
                    count++;
                }
                Assert.AreEqual(4, count);
            }

            private void GetAliasedEntity(IOrganizationService service)
            {
                var qe = QueryExpressionFactory.Create<Lead>();
                qe.AddLink<Contact>(Lead.Fields.CustomerId, Contact.Fields.Id, c => new { c.EMailAddress1 });
                var contact = service.GetFirst(qe).GetAliasedEntity<Contact>();
                Assert.AreEqual(contact.EMailAddress1, Ids.Contact.Entity.EMailAddress1);
            }
        }

        #endregion GetEntityExamples_Should_BehaviorExpected

        public QueryExpression GetStandardComplicatedQuery()
        {
            var query = new QueryExpression(Appointment.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            };

            var activityPartyLink = new LinkEntity
            {
                JoinOperator = JoinOperator.LeftOuter,
                LinkFromEntityName = "appointment",
                LinkFromAttributeName = "activityid",
                LinkToEntityName = "activityparty",
                LinkToAttributeName = "activityid"
            };

            activityPartyLink.LinkCriteria = new FilterExpression();
            activityPartyLink.LinkCriteria.FilterOperator = LogicalOperator.And;
            activityPartyLink.LinkCriteria.AddCondition("participationtypemask", ConditionOperator.Equal, 5); // required attendee

            var customerPartyLink = new LinkEntity
            {
                JoinOperator = JoinOperator.LeftOuter,
                LinkFromEntityName = "activityparty",
                LinkFromAttributeName = "partyid",
                LinkToEntityName = "contact",
                LinkToAttributeName = "contactid"
            };

            activityPartyLink.LinkEntities.Add(customerPartyLink);

            var customerOppertunityLink = new LinkEntity
            {
                JoinOperator = JoinOperator.LeftOuter,
                LinkFromEntityName = "contact",
                LinkFromAttributeName = "parentcontactid",
                LinkToEntityName = "opportunity",
                LinkToAttributeName = "contactid"
            };

            customerOppertunityLink.LinkCriteria = new FilterExpression();
            customerOppertunityLink.LinkCriteria.FilterOperator = LogicalOperator.And;
            customerOppertunityLink.LinkCriteria.AddCondition("campaignid", ConditionOperator.NotEqual, "898EFDEA-6BAB-E211-AE0A-BC305BEFA6F9");
            customerPartyLink.LinkEntities.Add(customerOppertunityLink);

            var systemUserLink = new LinkEntity
            {
                JoinOperator = JoinOperator.Natural,
                LinkFromEntityName = "activityparty",
                LinkFromAttributeName = "partyid",
                LinkToEntityName = "systemuser",
                LinkToAttributeName = "systemuserid",
                Columns = new ColumnSet("systemuserid", "fullname", "isdisabled")
            };

            activityPartyLink.LinkEntities.Add(systemUserLink);

            query.LinkEntities.Add(activityPartyLink);

            return query;
        }

        public QueryExpression GetComplicatedStraightConvertedQuery()
        {
            var query = new QueryExpression(Appointment.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true)
            };

            var ap = query.AddLink("activityparty", "activityid", JoinOperator.LeftOuter);

            //var activityPartyLink = new LinkEntity
            //{
            //    JoinOperator = JoinOperator.LeftOuter,
            //    LinkFromEntityName = "appointment",
            //    LinkFromAttributeName = "activityid",
            //    LinkToEntityName = "activityparty",
            //    LinkToAttributeName = "activityid",
            //    EntityAlias = "ap"
            //};

            ap.WhereEqual("participationtypemask", 5);
            //activityPartyLink.LinkCriteria = new FilterExpression();
            //activityPartyLink.LinkCriteria.FilterOperator = LogicalOperator.And;
            //activityPartyLink.LinkCriteria.AddCondition("participationtypemask", ConditionOperator.Equal, 5); // required attendee

            var cp = ap.AddChildLink("contact", "partyid", "contactid", JoinOperator.LeftOuter);
            //var customerPartyLink = new LinkEntity
            //{
            //    JoinOperator = JoinOperator.LeftOuter,
            //    LinkFromEntityName = "activityparty",
            //    LinkFromAttributeName = "partyid",
            //    LinkToEntityName = "contact",
            //    LinkToAttributeName = "contactid",
            //    EntityAlias = "cp"
            //};

            //activityPartyLink.LinkEntities.Add(customerPartyLink);

            cp.AddChildLink("opportunity", "parentcontactid", "contactid", JoinOperator.LeftOuter)
              .WhereEqual(new ConditionExpression("campaignid", ConditionOperator.NotEqual, "898EFDEA-6BAB-E211-AE0A-BC305BEFA6F9"));
            //var customerOppertunityLink = new LinkEntity
            //{
            //    JoinOperator = JoinOperator.LeftOuter,
            //    LinkFromEntityName = "contact",
            //    LinkFromAttributeName = "parentcontactid",
            //    LinkToEntityName = "opportunity",
            //    LinkToAttributeName = "contactid",
            //    EntityAlias = "cp"
            //};

            ap.AddChildLink("systemuser", "partyid", "systemuserid", JoinOperator.Natural)
                .Columns = new ColumnSet("systemuserid", "fullname", "isdisabled");
            //var systemUserLink = new LinkEntity
            //{
            //    JoinOperator = JoinOperator.Natural,
            //    LinkFromEntityName = "activityparty",
            //    LinkFromAttributeName = "partyid",
            //    LinkToEntityName = "systemuser",
            //    LinkToAttributeName = "systemuserid",
            //    EntityAlias = "agent",
            //    Columns = new ColumnSet("systemuserid", "fullname", "isdisabled")
            //};

            //activityPartyLink.LinkEntities.Add(systemUserLink);

            //query.LinkEntities.Add(activityPartyLink);

            return query;
        }

        public QueryExpression GetComplicatedUltraSimplifiedQuery()
        {
            var qe = QueryExpressionFactory.Create<Appointment>();
            var ap = qe.AddLink<ActivityParty>(Appointment.Fields.ActivityId, JoinOperator.LeftOuter)
                .WhereEqual(ActivityParty.Fields.ParticipationTypeMask, (int) ActivityParty_ParticipationTypeMask.Requiredattendee);

            ap.AddLink<Contact>(ActivityParty.Fields.PartyId, Contact.Fields.Id, JoinOperator.LeftOuter)
              .AddLink<Opportunity>(Contact.Fields.ParentContactId, Opportunity.Fields.ContactId, JoinOperator.LeftOuter)
              .WhereEqual(new ConditionExpression(Opportunity.Fields.CampaignId, ConditionOperator.NotEqual, "898EFDEA-6BAB-E211-AE0A-BC305BEFA6F9"));

            ap.AddLink<SystemUser>(ActivityParty.Fields.PartyId, SystemUser.Fields.Id, JoinOperator.Natural, 
                u => new { u.SystemUserId, u.FullName, u.IsDisabled });

            return qe;
        }
    }
}
