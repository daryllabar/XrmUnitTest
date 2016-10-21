using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class AliasedValueTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_AliasedValue_AliasedFormattedValuePopulated()
        {
            var service = GetService();
            var id = service.Create(new Lead { BudgetStatusEnum = BudgetStatus.MayBuy });
            service.Create(new Account { OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, id) });

            var qe = QueryExpressionFactory.Create<Account>();
            qe.AddLink<Lead>(Account.Fields.OriginatingLeadId, Lead.Fields.LeadId, l => new { l.BudgetStatus });

            // Retrieve
            var entity = service.GetFirst(qe);
            Assert.AreEqual(BudgetStatus.MayBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
            Assert.AreEqual(BudgetStatus.MayBuy.ToString(), entity.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
        }

        /// <summary>
        /// A joined entity's aliased name defaults to logicalnameX where X is the occurance of the entity in the query
        /// </summary>
        [TestMethod]
        public void LocalCrmTests_AliasedValue_MultipleAliasedEntitesPostfixOccurance()
        {
            //
            // Arrange
            //
            var service = GetService();
            var qe = ArrangeAccountLinkCaseLinkPhoneCall(service);

            //
            // Act
            //
            var account = service.GetFirst(qe);

            //
            // Assert
            //
            var key = Incident.EntityLogicalName + "1." + Incident.Fields.Description;
            Assert.IsTrue(account.Attributes.ContainsKey(key), $"Expected Alliased Attribute key {key} was not found.  Found {string.Join(", ", account.Attributes.Keys)}");
            key = PhoneCall.EntityLogicalName + "2." + PhoneCall.Fields.Subject;
            Assert.IsTrue(account.Attributes.ContainsKey(key), $"Expected Alliased Attribute key {key} was not found.  Found {string.Join(", ", account.Attributes.Keys)}");
        }

        /// <summary>
        /// Only unaliased entites should be postfixed with the logical name and occurance
        /// </summary>
        [TestMethod]
        public void LocalCrmTests_AliasedValue_SingleUnaliasedEntity_Should_PostfixOccurance()
        {
            //
            // Arrange
            //
            var service = GetService();
            var qe = ArrangeAccountLinkCaseLinkPhoneCall(service);
            qe.LinkEntities.First().EntityAlias = "EntityAlias";

            //
            // Act
            //
            var account = service.GetFirst(qe);

            //
            // Assert
            //
            var key = qe.LinkEntities.First().EntityAlias + "." + Incident.Fields.Description;
            Assert.IsTrue(account.Attributes.ContainsKey(key), $"Expected Alliased Attribute key {key} was not found.  Found {string.Join(", ", account.Attributes.Keys)}");
            key = PhoneCall.EntityLogicalName + "1." + PhoneCall.Fields.Subject;
            Assert.IsTrue(account.Attributes.ContainsKey(key), $"Expected Alliased Attribute key {key} was not found.  Found {string.Join(", ", account.Attributes.Keys)}");
        }

        #region Shared Methods

        private TypedQueryExpression<Account> ArrangeAccountLinkCaseLinkPhoneCall(IOrganizationService service)
        {
            //
            // Arrange
            //
            var id = service.Create(new Account());
            var incident = new Incident()
            {
                Description = "Description",
                CustomerId = new EntityReference(Account.EntityLogicalName, id)
            };
            incident.Id = service.Create(incident);
            service.Create(new PhoneCall
            {
                Subject = "Subject",
                RegardingObjectId = incident.ToEntityReference()
            });


            var qe = QueryExpressionFactory.Create<Account>();
            qe.AddLink<Incident>(Account.Fields.Id, Incident.Fields.CustomerId, i => new { i.Description })
                .AddLink<PhoneCall>(Incident.Fields.Id, PhoneCall.Fields.RegardingObjectId, p => new { p.Subject });

            return qe;
        }

        #endregion Shared Methods
    }
}
