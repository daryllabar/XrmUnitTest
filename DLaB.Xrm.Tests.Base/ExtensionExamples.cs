using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Tests
{
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
