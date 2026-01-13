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
using Microsoft.Xrm.Sdk.Organization;

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
            var request = new CreateMultipleRequest
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
                {
                    EntityName = Account.EntityLogicalName,
                }
            };
            var response = (CreateMultipleResponse)_service.Execute(request);

            Assert.AreEqual("1st", _service.GetEntity<Account>(response.Ids[0]).Name);
            Assert.AreEqual("2nd", _service.GetEntity<Account>(response.Ids[1]).Name);

            request.Targets.EntityName = null;
            AssertEntityNameRequired(request);
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
            Assert.HasCount(2, response.Responses);

            _service.Delete(account.Entity);
            _service.Delete(contact.Entity);

            request.ReturnResponses = false;
            response = (ExecuteTransactionResponse)_service.Execute(request);
            Assert.IsEmpty(response.Responses);
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

            using var context = new CrmContext(_service);
            var firstContact = context.ContactSet.First();
            context.LoadProperty(firstContact, Contact.Fields.equipment_contacts);
            Assert.AreEqual(firstContact.PreferredEquipmentId, equipment.ToEntityReference());
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
            Assert.HasCount(2, ((PicklistAttributeMetadata)response).OptionSet.Options);
            Assert.AreEqual(AttributeTypeCode.String, GetMetadata(Account.Fields.AccountNumber).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Uniqueidentifier, GetMetadata(Account.Fields.Id).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Double, GetMetadata(Account.Fields.Address1_Longitude).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Decimal, GetMetadata(Account.Fields.ExchangeRate).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Boolean, GetMetadata(Account.Fields.FollowEmail).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Integer, GetMetadata(Account.Fields.ImportSequenceNumber).AttributeType);
            Assert.AreEqual(AttributeTypeCode.Lookup, GetMetadata(Account.Fields.ParentAccountId).AttributeType);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveCurrentOrganizationRequest()
        {
            var response = (RetrieveCurrentOrganizationResponse)_service.Execute(new RetrieveCurrentOrganizationRequest());
            var detail = response.Detail;

            Assert.AreNotEqual(Guid.Empty, detail.DatacenterId);
            Assert.IsNotNull(detail.Endpoints);
            Assert.EndsWith(".api.crm.dynamics.com/XRMServices/2011/Organization.svc", detail.Endpoints[EndpointType.OrganizationService]);
            Assert.EndsWith(".api.crm.dynamics.com/XRMServices/2011/OrganizationData.svc", detail.Endpoints[EndpointType.OrganizationDataService]);
            Assert.EndsWith(".crm.dynamics.com/", detail.Endpoints[EndpointType.WebApplication]);
            Assert.IsNotNull(detail.EnvironmentId);
            Assert.IsNotNull(detail.FriendlyName);
            Assert.IsNotNull(detail.Geo);
            Assert.AreNotEqual(Guid.Empty, detail.OrganizationId);
            Assert.AreEqual(OrganizationType.Customer, detail.OrganizationType);
            Assert.IsNotNull(detail.OrganizationVersion);
            Assert.IsNotNull(detail.SchemaType);
            Assert.AreEqual(OrganizationState.Enabled, detail.State);
            Assert.IsNotNull(detail.TenantId);
            Assert.IsNotNull(detail.UniqueName);
            Assert.IsNotNull(detail.UrlName);
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
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RetrieveTotalRecordCountRequest()
        {
            _service.Create(new Contact { LastName = "Test1" });
            _service.Create(new Contact { LastName = "Test2" });
            _service.Create(new Contact { LastName = "Test3" });
            _service.Create(new Contact { LastName = "Test4" });
            _service.Create(new Account { Name = "Test1" });
            _service.Create(new Account { Name = "Test2" });
            _service.Create(new Account { Name = "Test3" });

            var response = (RetrieveTotalRecordCountResponse)_service.Execute(new RetrieveTotalRecordCountRequest
            {
                EntityNames = new[] { Contact.EntityLogicalName, Account.EntityLogicalName }
            });
            Assert.AreEqual(3, response.EntityRecordCountCollection[Account.EntityLogicalName]);
            Assert.AreEqual(4, response.EntityRecordCountCollection[Contact.EntityLogicalName]);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpdateMultipleRequest()
        {
            var account = new Account { Name = "1st" };
            account.Id = _service.Create(account);
            var account2 = new Account { Name = "2nd" };
            account2.Id = _service.Create(account2);
            var request = new UpdateMultipleRequest
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
                {
                    EntityName = Account.EntityLogicalName,
                }
            };

            _service.Execute(request);

            Assert.AreEqual("1st Updated", _service.GetEntity<Account>(account.Id).Name);
            Assert.AreEqual("2nd Updated", _service.GetEntity<Account>(account2.Id).Name);

            request.Targets.EntityName = null;
            AssertEntityNameRequired(request);
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_UpsertMultipleRequest()
        {
            var account = new Account { Name = "1st" };
            account.Id = _service.Create(account);
            var newAccount = new Account { Name = "NEW" };
            newAccount.KeyAttributes.Add(Account.Fields.ParentAccountId, account.Id);
            var request = new UpsertMultipleRequest
            {
                Targets = new EntityCollection(new Entity[] {
                    new Account
                    {
                        Id = account.Id,
                        Name = account.Name += " Updated"
                    }, newAccount
                })
                {
                    EntityName = Account.EntityLogicalName,
                }
            };

            var results = ((UpsertMultipleResponse)_service.Execute(request)).Results;

            account = _service.GetEntity<Account>(results[0].Target.Id);
            Assert.IsFalse(results[0].RecordCreated);
            Assert.AreEqual("1st Updated", account.Name);

            account = _service.GetEntity<Account>(results[1].Target.Id);
            Assert.IsTrue(results[1].RecordCreated);
            Assert.AreEqual("NEW", account.Name);

            request.Targets.EntityName = null;
            AssertEntityNameRequired(request);
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
            var toDelete = _service.GetEntityOrDefault<Account>(account.KeyAttributes) ?? new Account();
            _service.Delete(Account.EntityLogicalName, toDelete.Id);

            account.KeyAttributes[Account.Fields.ParentAccountId] = parentId.ToString();
            TestUpsertCreateAndUpdate(account);
            toDelete = _service.GetEntityOrDefault<Account>(account.KeyAttributes) ?? new Account();
            _service.Delete(Account.EntityLogicalName, toDelete.Id);

            account.KeyAttributes[Account.Fields.ParentAccountId] = new EntityReference( Account.EntityLogicalName, parentId);
            TestUpsertCreateAndUpdate(account);
        }

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

        private void TestUpsertCreateAndUpdate(Account toUpsert)
        {
            // Test Insert
            var response = (UpsertResponse) _service.Execute(new UpsertRequest
            {
                Target = toUpsert
            });
            Assert.IsTrue(response.RecordCreated);
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
            Assert.IsFalse(response.RecordCreated);

            account = _service.GetEntity<Account>(response.Target.Id);
            Assert.AreEqual(toUpsert.Name, account.Name);
        }

        private void AssertEntityNameRequired(OrganizationRequest request)
        {
            try
            {
                _service.Execute(request);
                Assert.Fail("Exception Expected!");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual($"The '{request.RequestName}' method does not support entities of type 'none'. MessageProcessorCache returned MessageProcessor.Empty. ",
                    ex.Message);
                Assert.AreEqual($"The '{request.RequestName}' method does not support entities of type 'none'. MessageProcessorCache returned MessageProcessor.Empty. ",
                    ex.Detail.Message);
                Assert.AreEqual("Microsoft.PowerPlatform.Dataverse.Client", ex.Source);
            }
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_GrantAccessRequest()
        {
            // Create a test account
            var account = new Account { Name = "Test Account" };
            account.Id = _service.Create(account);

            // Create a test user (principal)
            var user = new SystemUser { FirstName = "Test", LastName = "User" };
            user.Id = _service.Create(user);

            // Grant access to the account for the user
            var grantRequest = new GrantAccessRequest
            {
                Target = account.ToEntityReference(),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = user.ToEntityReference(),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess
                }
            };

            var grantResponse = (GrantAccessResponse)_service.Execute(grantRequest);
            Assert.IsNotNull(grantResponse);

            // Verify that a PrincipalObjectAttributeAccess record was created
            var query = new QueryExpression("principalobjectattributeaccess")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("principalid", ConditionOperator.Equal, user.Id),
                        new ConditionExpression("objectid", ConditionOperator.Equal, account.Id)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            Assert.AreEqual(1, results.Entities.Count, "Expected one PrincipalObjectAttributeAccess record to be created");

            var accessRecord = results.Entities.First();
            Assert.AreEqual(true, accessRecord.GetAttributeValue<bool>("readaccess"), "ReadAccess should be true");
            Assert.AreEqual(true, accessRecord.GetAttributeValue<bool>("updateaccess"), "UpdateAccess should be true");
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_RevokeAccessRequest()
        {
            // Create a test account
            var account = new Account { Name = "Test Account" };
            account.Id = _service.Create(account);

            // Create a test user (principal)
            var user = new SystemUser { FirstName = "Test", LastName = "User" };
            user.Id = _service.Create(user);

            // Grant access first
            var grantRequest = new GrantAccessRequest
            {
                Target = account.ToEntityReference(),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = user.ToEntityReference(),
                    AccessMask = AccessRights.ReadAccess
                }
            };
            _service.Execute(grantRequest);

            // Verify access was granted
            var queryBefore = new QueryExpression("principalobjectattributeaccess")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("principalid", ConditionOperator.Equal, user.Id),
                        new ConditionExpression("objectid", ConditionOperator.Equal, account.Id)
                    }
                }
            };
            var resultsBefore = _service.RetrieveMultiple(queryBefore);
            Assert.AreEqual(1, resultsBefore.Entities.Count, "Expected one PrincipalObjectAttributeAccess record before revoke");

            // Revoke access
            var revokeRequest = new RevokeAccessRequest
            {
                Target = account.ToEntityReference(),
                Revokee = user.ToEntityReference()
            };

            var revokeResponse = (RevokeAccessResponse)_service.Execute(revokeRequest);
            Assert.IsNotNull(revokeResponse);

            // Verify that the PrincipalObjectAttributeAccess record was deleted
            var queryAfter = new QueryExpression("principalobjectattributeaccess")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("principalid", ConditionOperator.Equal, user.Id),
                        new ConditionExpression("objectid", ConditionOperator.Equal, account.Id)
                    }
                }
            };
            var resultsAfter = _service.RetrieveMultiple(queryAfter);
            Assert.AreEqual(0, resultsAfter.Entities.Count, "Expected no PrincipalObjectAttributeAccess records after revoke");
        }

        [TestMethod]
        public void LocalCrmDatabaseOrganizationServiceExecuteTests_GrantAccessRequest_ReadOnly()
        {
            // Create a test contact
            var contact = new Contact { LastName = "Test Contact" };
            contact.Id = _service.Create(contact);

            // Create a test user (principal)
            var user = new SystemUser { FirstName = "Test", LastName = "User" };
            user.Id = _service.Create(user);

            // Grant read-only access
            var grantRequest = new GrantAccessRequest
            {
                Target = contact.ToEntityReference(),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = user.ToEntityReference(),
                    AccessMask = AccessRights.ReadAccess
                }
            };

            _service.Execute(grantRequest);

            // Verify access rights
            var query = new QueryExpression("principalobjectattributeaccess")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("principalid", ConditionOperator.Equal, user.Id),
                        new ConditionExpression("objectid", ConditionOperator.Equal, contact.Id)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            Assert.AreEqual(1, results.Entities.Count);

            var accessRecord = results.Entities.First();
            Assert.AreEqual(true, accessRecord.GetAttributeValue<bool>("readaccess"), "ReadAccess should be true");
            Assert.AreEqual(false, accessRecord.GetAttributeValue<bool>("updateaccess"), "UpdateAccess should be false");
        }
    }
}
