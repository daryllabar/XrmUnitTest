using DLaB.Xrm.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XrmUnitTest.Test.Builders;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class LocalCrmDatabaseOrganizationServiceExecuteTests : BaseTestClass
    {

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_InitializeFromRequest()
        {
            var service = GetService();
            var currency = new TransactionCurrency();
            currency.Id = service.Create(currency);

            var lead = new LeadBuilder
            {
                Lead = new Lead
                {
                    Address1_Line2 = "Test Address1_Line2",
                    Address1_Line3 = "Test Address1_Line3",
                    Description = "Test Description",
                    Fax = "555-555-1234",
                    JobTitle = "Sales Agent",
                    LeadSourceCodeEnum = Lead_LeadSourceCode.Advertisement,
                    PreferredContactMethodCodeEnum = Lead_PreferredContactMethodCode.Phone,
                    WebSiteUrl = "https://github.com/daryllabar/XrmUnitTest",
                    TransactionCurrencyId = currency.ToEntityReference()
                }
            }
                .WithAddress1()
                .WithAddress2()
                .WithDoNotContact(false)
                .WithEmail()
                .WithName()
                .WithPhone()
                .Build();

            lead.Id = service.Create(lead);
            lead = service.GetEntity<Lead>(lead.Id);

            var contact = service.InitializeFrom<Contact>(lead.ToEntityReference(), TargetFieldType.ValidForCreate);
            foreach (var attribute in lead.Attributes)
            {
                var key = attribute.Key;
                var value = attribute.Value;
                switch (key)
                {
                    case Lead.Fields.LeadId:
                        key = Contact.Fields.OriginatingLeadId;
                        value = lead.ToEntityReference();
                        break;
                    case Lead.Fields.CreatedOn:
                    case Lead.Fields.CreatedBy:
                    case Lead.Fields.ModifiedOn:
                    case Lead.Fields.ModifiedBy:
                        Assert.IsFalse(contact.Contains(key));
                        continue;
                }

                Assert.IsTrue(
                    contact.Contains(key) && contact[key].Equals(value), 
                    $"Field {attribute.Key} was not mapped correctly.");
            }
            service.Create(contact);
        }
    }
}
