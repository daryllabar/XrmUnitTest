using System;
using System.Linq;
using System.ServiceModel;
using DLaB.Xrm.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Builders;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class LocalCrmDatabaseOrganizationServiceExecuteTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_ExecuteTransactionRequest()
        {
            var account = new Id<Account>("576E11B7-193A-4B80-A39A-1BF6ECD27A51");
            var contact = new Id<Contact>("A40249BF-637A-4AB9-A944-3C2506D12F18");
            var request = new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new CreateRequest{ Target = account },
                    new CreateRequest{ Target = contact }
                },
                ReturnResponses = true
            };
            var service = GetService();
            var response = (ExecuteTransactionResponse) service.Execute(request);
            AssertCrm.Exists(service, account);
            AssertCrm.Exists(service, contact);
            Assert.AreEqual(response.Responses.Count, 2);

            service.Delete(account.Entity);
            service.Delete(contact.Entity);

            request.ReturnResponses = false;
            response = (ExecuteTransactionResponse)service.Execute(request);
            Assert.AreEqual(response.Responses.Count, 0);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_ExecuteErrorTransactionRequest()
        {
            var account = new Id<Account>("576E11B7-193A-4B80-A39A-1BF6ECD22A51");
            var request = new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection
                {
                    new CreateRequest{ Target = account },
                    new CreateRequest{ Target = account }
                },
                ReturnResponses = true
            };
            var service = GetService();
            try
            {
                service.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("Cannot insert duplicate key", ex.Message);
                Assert.AreEqual("Cannot insert duplicate key", ex.Detail.Message);
                Assert.AreEqual(1, ((ExecuteTransactionFault)ex.Detail).FaultedRequestIndex);
            }
        }

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
                    case Lead.Fields.OwningBusinessUnit:
                    case Lead.Fields.OwningUser:
                    case Lead.Fields.OwningTeam:
                    case Lead.Fields.StateCode:
                    case Lead.Fields.StatusCode:
                        Assert.IsFalse(contact.Contains(key));
                        continue;
                }

                Assert.IsTrue(
                    contact.Contains(key) && contact[key].Equals(value), 
                    $"Field {attribute.Key} was not mapped correctly.");
            }
            service.Create(contact);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_LocalTimeFromUtcTimeRequest()
        {
            var service = GetService();
            var now = DateTime.UtcNow;
            var response = (LocalTimeFromUtcTimeResponse)service.Execute(new LocalTimeFromUtcTimeRequest {TimeZoneCode = 35, UtcTime = now});
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Assert.AreEqual(0, (response.LocalTime - TimeZoneInfo.ConvertTimeFromUtc(now, timeZone)).TotalMilliseconds);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveRelationshipRequest()
        {
            var service = GetService();
            var equipment = new Equipment();
            equipment.Id = service.Create(equipment);

            var currency = new Contact
            {
                PreferredEquipmentId = equipment.ToEntityReference()
            };
            currency.Id = service.Create(currency);

            using (var context = new CrmContext(service))
            {
                var firstContact = context.ContactSet.First();
                context.LoadProperty(firstContact, Contact.Fields.equipment_contacts);
                Assert.AreEqual(firstContact.PreferredEquipmentId, equipment.ToEntityReference());
            }
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveAttributeRequest()
        {
            AttributeMetadata GetMetadata(string field)
            {
                return ((RetrieveAttributeResponse)GetService().Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = Account.EntityLogicalName,
                    LogicalName = field
                })).AttributeMetadata;
            }
            TestInitializer.InitializeTestSettings();
            
            var response = GetMetadata(Account.Fields.AccountCategoryCode);
            Assert.AreEqual(AttributeTypeCode.Picklist, response.AttributeType);
            Assert.AreEqual(2, ((PicklistAttributeMetadata)response).OptionSet.Options.Count);
            Assert.AreEqual(AttributeTypeCode.String, GetMetadata(Account.Fields.AccountNumber).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Uniqueidentifier, GetMetadata(Account.Fields.Id).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Double, GetMetadata(Account.Fields.Address1_Longitude).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Decimal, GetMetadata(Account.Fields.ExchangeRate).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Boolean, GetMetadata(Account.Fields.FollowEmail).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Integer, GetMetadata(Account.Fields.ImportSequenceNumber).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Lookup, GetMetadata(Account.Fields.ParentAccountId).AttributeType);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveEntityRequest()
        {
            TestInitializer.InitializeTestSettings();
            var response = (RetrieveEntityResponse)GetService().Execute(new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = Contact.EntityLogicalName,
                RetrieveAsIfPublished = true
            });
            Assert.AreEqual(Contact.PrimaryNameAttribute, response.EntityMetadata.PrimaryNameAttribute);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpsertRequest()
        {
            TestInitializer.InitializeTestSettings();
            var service = GetService();
            var account = new Account
            {
                Name = "1st"
            };

            account.KeyAttributes.Add(Account.Fields.Description, "Custom Key");
            TestUpsertCreateAndUpdate(service, account);

            account.Id = Guid.NewGuid();
            account.Name = "1st";
            account.KeyAttributes.Clear();
            TestUpsertCreateAndUpdate(service, account);
        }

        private static void TestUpsertCreateAndUpdate(IOrganizationService service, Account toUpsert)
        {
            // Test Insert
            var response = (UpsertResponse) service.Execute(new UpsertRequest
            {
                Target = toUpsert
            });
            Assert.AreEqual(true, response.RecordCreated);
            AssertCrm.Exists(service, response.Target);
            var account = service.GetEntity<Account>(response.Target.Id);
            Assert.AreEqual(toUpsert.Name, account.Name);
            foreach(var kvp in toUpsert.KeyAttributes)
            {
                Assert.AreEqual(account[kvp.Key], kvp.Value);
            }

            // Test Update
            toUpsert.Name = "2nd";
            response = (UpsertResponse) service.Execute(new UpsertRequest
            {
                Target = toUpsert
            });
            Assert.AreEqual(false, response.RecordCreated);

            account = service.GetEntity<Account>(response.Target.Id);
            Assert.AreEqual(toUpsert.Name, account.Name);
        }
    }
}
