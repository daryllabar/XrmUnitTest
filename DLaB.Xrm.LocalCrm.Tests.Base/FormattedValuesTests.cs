using System.Globalization;
using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class FormattedValuesTests : BaseTestClass
    {

        [TestMethod]
        public void LocalCrmTests_FormattedValues_Populated()
        {
            var service = GetService();
            var lead = new Lead
            {
                BudgetStatusEnum = BudgetStatus.CanBuy,
                StatusCodeEnum = Lead_StatusCode.New,
                StateCode = LeadState.Open,
                BudgetAmount = new Money(10.05m)
            };
            var id = service.Create(lead);

            LocalCrmTests_FormattedValuesPopulated_Assert(lead, service.GetEntity<Lead>(id));
            LocalCrmTests_FormattedValuesPopulated_Assert(lead, service.GetEntitiesById<Lead>(id).Single());
        }

        private void LocalCrmTests_FormattedValuesPopulated_Assert(Lead expected, Lead actual)
        {
            Assert.AreEqual(expected.BudgetStatusEnum.ToString(), actual.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetStatus));
            Assert.AreEqual(expected.StatusCodeEnum.ToString(), actual.GetFormattedAttributeValueOrNull(Lead.Fields.StatusCode));
            Assert.AreEqual(expected.StateCode.ToString(), actual.GetFormattedAttributeValueOrNull(Lead.Fields.StateCode));
            Assert.AreEqual(expected.BudgetAmount.Value.ToString("C", CultureInfo.CurrentCulture), actual.GetFormattedAttributeValueOrNull(Lead.Fields.BudgetAmount));

        }

        [TestMethod]
        public void LocalCrmTests_FormattedValues_Ignored()
        {
            var service = GetService();

            var contact = new Contact { AccountRoleCodeEnum = Contact_AccountRoleCode.DecisionMaker };
            contact.FormattedValues.Add(Contact.Fields.AccountRoleCode, "Verify Formatted Values Are Ignored");
            service.Create(contact);

            contact = service.GetFirstOrDefault<Contact>();
            Assert.AreEqual(Contact_AccountRoleCode.DecisionMaker, contact.AccountRoleCodeEnum);
            Assert.AreEqual(Contact_AccountRoleCode.DecisionMaker.ToString(), contact.FormattedValues[Contact.Fields.AccountRoleCode]);
        }
    }
}
