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
using Microsoft.Xrm.Sdk.Query;
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
        private IOrganizationService _service;

        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.InitializeTestSettings();
            _service = GetService();
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_CreateMultipleRequest()
        {
            var response = (CreateMultipleResponse)_service.Execute(new CreateMultipleRequest
            {
                Targets = new EntityCollection(new Entity[] { 
                    new Account
                    {
                        Name = "1st"
                    }, new Account
                    {
                        Name = "2nd"
                    }
                })
            });

            Assert.AreEqual("1st", _service.GetEntity<Account>(response.Ids[0]).Name);
            Assert.AreEqual("2nd", _service.GetEntity<Account>(response.Ids[1]).Name);
        }

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
            var response = (ExecuteTransactionResponse) _service.Execute(request);
            AssertCrm.Exists(_service, account);
            AssertCrm.Exists(_service, contact);
            Assert.AreEqual(response.Responses.Count, 2);

            _service.Delete(account.Entity);
            _service.Delete(contact.Entity);

            request.ReturnResponses = false;
            response = (ExecuteTransactionResponse)_service.Execute(request);
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
            try
            {
                _service.Execute(request);
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
            var currency = new TransactionCurrency();
            currency.Id = _service.Create(currency);

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

            lead.Id = _service.Create(lead);
            lead = _service.GetEntity<Lead>(lead.Id);

            var contact = _service.InitializeFrom<Contact>(lead.ToEntityReference(), TargetFieldType.ValidForCreate);
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
            _service.Create(contact);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_LocalTimeFromUtcTimeRequest()
        {
            var now = DateTime.UtcNow;
            var response = (LocalTimeFromUtcTimeResponse)_service.Execute(new LocalTimeFromUtcTimeRequest {TimeZoneCode = 35, UtcTime = now});
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Assert.AreEqual(0, (response.LocalTime - TimeZoneInfo.ConvertTimeFromUtc(now, timeZone)).TotalMilliseconds);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveRelationshipRequest()
        {
            var equipment = new Equipment();
            equipment.Id = _service.Create(equipment);

            var currency = new Contact
            {
                PreferredEquipmentId = equipment.ToEntityReference()
            };
            currency.Id = _service.Create(currency);

            using (var context = new CrmContext(_service))
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
                return ((RetrieveAttributeResponse)_service.Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = Account.EntityLogicalName,
                    LogicalName = field
                })).AttributeMetadata;
            }
            
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
            var response = (RetrieveEntityResponse)_service.Execute(new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = Contact.EntityLogicalName,
                RetrieveAsIfPublished = true
            });
            Assert.AreEqual(Contact.PrimaryNameAttribute, response.EntityMetadata.PrimaryNameAttribute);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpdateMultipleRequest()
        {
            var account = new Account { Name = "1st" };
            account.Id = _service.Create(account);
            var account2 = new Account { Name = "2nd" };
            account2.Id = _service.Create(account2);

            _service.Execute(new UpdateMultipleRequest
            {
                Targets = new EntityCollection(new Entity[] {
                    new Account
                    {
                        Id = account.Id,
                        Name = account.Name += " Updated"
                    }, new Account
                    {
                        Id = account2.Id,
                        Name = account2.Name += " Updated"
                    }
                })
            });

            Assert.AreEqual("1st Updated", _service.GetEntity<Account>(account.Id).Name);
            Assert.AreEqual("2nd Updated", _service.GetEntity<Account>(account2.Id).Name);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpsertMultipleRequest()
        {
            var account = new Account { Name = "1st" };
            account.Id = _service.Create(account);
            var newAccount = new Account { Name = "NEW" };
            newAccount.KeyAttributes.Add(Account.Fields.ParentAccountId, account.Id);

            var results = ((UpsertMultipleResponse)_service.Execute(new UpsertMultipleRequest
            {
                Targets = new EntityCollection(new Entity[] {
                    new Account
                    {
                        Id = account.Id,
                        Name = account.Name += " Updated"
                    }, newAccount
                })
            })).Results;

            account = _service.GetEntity<Account>(results[0].Target.Id);
            Assert.IsFalse(results[0].RecordCreated);
            Assert.AreEqual("1st Updated", account.Name);

            account = _service.GetEntity<Account>(results[1].Target.Id);
            Assert.IsTrue(results[1].RecordCreated);
            Assert.AreEqual("NEW", account.Name);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpsertRequest()
        {
            var account = new Account
            {
                Name = "1st"
            };

            account.KeyAttributes.Add(Account.Fields.Description, "Custom Key");
            TestUpsertCreateAndUpdate(account);

            account.Id = Guid.NewGuid();
            account.Name = "1st";
            account.KeyAttributes.Clear();
            TestUpsertCreateAndUpdate(account);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpsertEntityRefRequest()
        {
            var parentId = _service.Create(new Account());
            var account = new Account
            {
                Name = "1st"
            };

            account.KeyAttributes.Add(Account.Fields.ParentAccountId, parentId);
            TestUpsertCreateAndUpdate(account);
            var toDelete = _service.GetEntityOrDefault<Account>(account.KeyAttributes);
            _service.Delete(Account.EntityLogicalName, toDelete.Id);

            account.KeyAttributes[Account.Fields.ParentAccountId] = parentId.ToString();
            TestUpsertCreateAndUpdate(account);
            toDelete = _service.GetEntityOrDefault<Account>(account.KeyAttributes);
            _service.Delete(Account.EntityLogicalName, toDelete.Id);

            account.KeyAttributes[Account.Fields.ParentAccountId] = new EntityReference( Account.EntityLogicalName, parentId);
            TestUpsertCreateAndUpdate(account);
        }

#if !PRE_KEYATTRIBUTE
        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveRequestByAltKey()
        {
            var account = new Account
            {
                Name = "1st"
            };
            account.Id = _service.Create(account);

            var request = new RetrieveRequest
            {
                ColumnSet = new ColumnSet(),
                Target = new EntityReference(Account.EntityLogicalName, Account.Fields.Name, account.Name)
            };

            var response = (RetrieveResponse)_service.Execute(request);
            
            Assert.AreEqual(account.Id, response.Entity.Id);
        }
#endif

        private void TestUpsertCreateAndUpdate(Account toUpsert)
        {
            // Test Insert
            var response = (UpsertResponse) _service.Execute(new UpsertRequest
            {
                Target = toUpsert
            });
            Assert.AreEqual(true, response.RecordCreated);
            AssertCrm.Exists(_service, response.Target);
            var account = _service.GetEntity<Account>(response.Target.Id);
            Assert.AreEqual(toUpsert.Name, account.Name);
            foreach(var kvp in toUpsert.KeyAttributes)
            {
                Assert.AreEqual(account[kvp.Key], kvp.Value);
            }

            // Test Update
            toUpsert.Name = "2nd";
            response = (UpsertResponse) _service.Execute(new UpsertRequest
            {
                Target = toUpsert
            });
            Assert.AreEqual(false, response.RecordCreated);

            account = _service.GetEntity<Account>(response.Target.Id);
            Assert.AreEqual(toUpsert.Name, account.Name);
        }
    }
}
