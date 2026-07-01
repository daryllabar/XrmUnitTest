using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class ConditionExpressionTests : BaseTestClass
    {

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_WithIsCaseInsensitive()
        {
            var service = Service;
            service.Create(new Contact { FirstName = "Jimmy" });
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.BeginsWith, "JIM")));
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.EndsWith, "MY")));
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.DoesNotBeginWith, "MY")));
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.DoesNotEndWith, "JIM")));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LikeIsCaseInsensitive()
        {
            var service = Service;
            service.Create(new Contact { FirstName = "Jimmy" });
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.Like, "JIM%")));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LikeWithAtSymbol()
        {
            var service = Service;
            service.Create(new Account { EMailAddress1 = "test@google.com" });
            service.Create(new Account { EMailAddress1 = "other@contoso.com" });
            service.Create(new Account { EMailAddress1 = "another@google.com" });

            // Should find accounts with @google.com
            var googleAccounts = service.GetEntities<Account>(new ConditionExpression(Account.Fields.EMailAddress1, ConditionOperator.Like, "%@google.com"));
            Assert.HasCount(2, googleAccounts, "Two accounts should have @google.com email domain");

            // Should find account with @contoso.com
            var contosoAccounts = service.GetEntities<Account>(new ConditionExpression(Account.Fields.EMailAddress1, ConditionOperator.Like, "%@contoso.com"));
            Assert.HasCount(1, contosoAccounts, "One account should have @contoso.com email domain");

            // Upper-case pattern should also work (case-insensitive)
            var googleAccountsUpper = service.GetEntities<Account>(new ConditionExpression(Account.Fields.EMailAddress1, ConditionOperator.Like, "%@GOOGLE.COM"));
            Assert.HasCount(2, googleAccountsUpper, "Two accounts should be found with upper-case @GOOGLE.COM pattern");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LikeOrMultipleDomains()
        {
            var service = Service;
            service.Create(new Account { EMailAddress1 = "test@google.com" });
            service.Create(new Account { EMailAddress1 = "other@contoso.com" });
            service.Create(new Account { EMailAddress1 = "another@google.com" });
            service.Create(new Account { EMailAddress1 = "yet@microsoft.com" });

            var emails = new List<string> { "test@google.com", "other@contoso.com" };
            var domains = emails
                .Where(e => !string.IsNullOrWhiteSpace(e) && e.Contains("@"))
                .Select(e => e.Substring(e.IndexOf('@') + 1))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var qe = new QueryExpression(Account.EntityLogicalName) { ColumnSet = new ColumnSet(Account.Fields.EMailAddress1) };
            qe.Criteria.FilterOperator = LogicalOperator.Or;
            foreach (var domain in domains)
            {
                qe.Criteria.AddCondition(Account.Fields.EMailAddress1, ConditionOperator.Like, $"%@{domain}");
            }

            var results = service.RetrieveMultiple(qe).Entities;
            Assert.HasCount(3, results, "Accounts matching @google.com or @contoso.com should be returned");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LastXDays()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d)
            });

            var contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.CreatedOn, ConditionOperator.LastXDays, 2));
            Assert.IsNotNull(contact, "Contact should have been found because it was created in the last 2 days");

            Entity GetContact(int days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.LastXDays, days));

            Assert.IsNull(GetContact(1));
            Assert.IsNotNull(GetContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(2d) });
            Assert.IsNull(GetContact(3));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LastXYears()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddYears(-2)
            });

            var contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.CreatedOn, ConditionOperator.LastXYears, 2));
            Assert.IsNotNull(contact, "Contact should have been found because it was created in the last 2 days");

            Entity GetContact(int years) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.LastXYears, years));

            Assert.IsNull(GetContact(1));
            Assert.IsNotNull(GetContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddYears(2) });
            Assert.IsNull(GetContact(3));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Last7Days()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(-6d)
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Last7Days));

            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-8d) });
            Assert.IsNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Next7Days()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(6d)
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Next7Days));

            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(8d) });
            Assert.IsNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_NextXDays()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(2d)
            });

            Entity GetContact(int days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.NextXDays, days));

            Assert.IsNull(GetContact(1));
            Assert.IsNotNull(GetContact(2));
            Assert.IsNotNull(GetContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d) });
            Assert.IsNull(GetContact(3));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_NextXYears()
        {
            var now = DateTime.UtcNow;
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = now.AddYears(2)
            });

            Entity GetContact(int years) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.NextXYears, years));

            Assert.IsNull(GetContact(1));
            Assert.IsNotNull(GetContact(2));
            Assert.IsNotNull(GetContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = now.AddYears(-2) });
            Assert.IsNull(GetContact(3));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_OnOrAfter()
        {
            var service = Service;
            var now = DateTime.UtcNow;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = now.AddDays(1d)
            });

            Contact GetContact(int days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.OnOrAfter, now.AddDays(days)));

            Assert.IsNotNull(GetContact(0));
            Assert.IsNotNull(GetContact(1));
            Assert.IsNull(GetContact(2));
            service.Update(new Contact { Id = id, LastUsedInCampaign = now.AddDays(2) });
            Assert.IsNotNull(GetContact(2));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_OnOrBefore()
        {
            var service = Service;
            var now = DateTime.UtcNow;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = now.AddDays(1d)
            });

            Contact GetContact(int days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.OnOrBefore, now.AddDays(days)));

            Assert.IsNull(GetContact(0));
            Assert.IsNotNull(GetContact(1));
            Assert.IsNotNull(GetContact(2));
            service.Update(new Contact { Id = id, LastUsedInCampaign = now.AddDays(-1) });
            Assert.IsNotNull(GetContact(0));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Today()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Today));

            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(1d) });
            Assert.IsNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-1d) });
            Assert.IsNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Tomorrow()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Tomorrow));

            Assert.IsNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(1d) });
            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(2d) });
            Assert.IsNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Yesterday()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Yesterday));

            Assert.IsNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-1d) });
            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d) });
            Assert.IsNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EqualUserId()
        {
            var service = Service;
            var otherUserId = service.Create(new SystemUser());
            var contactId = service.Create(new Contact { PreferredSystemUserId = new EntityReference(SystemUser.EntityLogicalName, otherUserId) });

            var contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.OwnerId, ConditionOperator.EqualUserId));
            Assert.IsNotNull(contact, "Contact should have been found because the current user is the equal user");

            contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.PreferredSystemUserId, ConditionOperator.EqualUserId));
            Assert.IsNull(contact, "Contact should not have been found because the current user is not the equal user");

            contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.PreferredSystemUserId, ConditionOperator.NotEqualUserId));
            Assert.IsNotNull(contact, "Contact should have been found because the current user is not the equal user");

            service.Update(new Contact { Id = contactId, PreferredSystemUserId = null });
            contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.PreferredSystemUserId, ConditionOperator.EqualUserId));
            Assert.IsNull(contact, "Contact should not have been found because the preferred user was null");

            contact = service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.PreferredSystemUserId, ConditionOperator.NotEqualUserId));
            Assert.IsNotNull(contact, "Contact should have been found because the current user is not the equal user");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_ContainsValue()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                CalendarTypes = new [] { Calendar_Type.CustomerService }
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(nameof(Contact.CalendarTypes).ToLower(), ConditionOperator.ContainValues, (int)Calendar_Type.Default));

            Assert.IsNull(GetContact());
            service.Update(new Contact{ Id = id, CalendarTypes = new [] { Calendar_Type.CustomerService, Calendar_Type.Default }});
            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, CalendarTypes = new[] { Calendar_Type.Default } });
            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, CalendarTypes = Array.Empty<Calendar_Type>() });
            Assert.IsNull(GetContact());
        }


        [TestMethod]
        public void LocalCrmTests_ConditionExpression_DoesNotContainValues()
        {
            var service = Service;
            var id = service.Create(new Contact
            {
                CalendarTypes = new[] { Calendar_Type.Default }
            });

            Entity GetContact() => service.GetFirstOrDefault<Contact>(new ConditionExpression(nameof(Contact.CalendarTypes).ToLower(), ConditionOperator.DoesNotContainValues, (int)Calendar_Type.Default));

            Assert.IsNull(GetContact());
            service.Update(new Contact { Id = id, CalendarTypes = new[] { Calendar_Type.CustomerService, Calendar_Type.Default } });
            Assert.IsNull(GetContact());
            service.Update(new Contact { Id = id, CalendarTypes = new[] { Calendar_Type.CustomerService } });
            Assert.IsNotNull(GetContact());
            service.Update(new Contact { Id = id, CalendarTypes = Array.Empty<Calendar_Type>() });
            Assert.IsNotNull(GetContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EntityReferenceWithIntValueThrowsError()
        {
            AssertOrganizationServiceFaultException(
                "EntityReference attribute with int condition value should throw FaultException",
                "expected argument(s) of type 'System.Guid' but received 'System.Int32'",
                () => Service.GetFirstOrDefault<Contact>(Contact.Fields.ParentCustomerId, 2));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EntityReferenceWithGuidValueWorks()
        {
            var result = Service.GetFirstOrDefault<Contact>(Contact.Fields.ParentCustomerId, Guid.NewGuid());
            Assert.IsNull(result, "No contacts exist, so result should be null");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EntityReferenceWithEntityReferenceValueWorks()
        {
            // EntityReference values are valid for EntityReference attributes when using ConditionExpression directly
            var qe = new QueryExpression(Contact.EntityLogicalName);
            qe.Criteria.AddCondition(Contact.Fields.ParentCustomerId, ConditionOperator.Equal, new EntityReference(Account.EntityLogicalName, Guid.NewGuid()));
            var result = Service.GetFirstOrDefault(qe);
            Assert.IsNull(result, "No contacts exist, so result should be null");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EntityReferenceWithValidStringGuidValueWorks()
        {
            var result = Service.GetFirstOrDefault<Contact>(Contact.Fields.ParentCustomerId, Guid.NewGuid().ToString());
            Assert.IsNull(result, "No contacts exist, so result should be null");
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_EntityReferenceWithInvalidStringValueThrowsError()
        {
            AssertOrganizationServiceFaultException(
                "EntityReference attribute with non-parseable string condition value should throw FaultException",
                "Expected type of attribute value: System.Guid",
                () => Service.GetFirstOrDefault<Contact>(Contact.Fields.ParentCustomerId, "not-a-guid"));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_BooleanAttributeWithNonBoolValue()
        {
            var service = Service;
            service.Create(new Contact { DoNotEMail = true });
            service.Create(new Contact { DoNotEMail = false });

            // int 1 should match true
            var results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, 1));
            Assert.HasCount(1, results, "int 1 should match DoNotEMail = true");

            // int 0 should match false
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, 0));
            Assert.HasCount(1, results, "int 0 should match DoNotEMail = false");

            // long 1 should match true
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, 1L));
            Assert.HasCount(1, results, "long 1 should match DoNotEMail = true");

            // string "1" should match true
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, "1"));
            Assert.HasCount(1, results, "string '1' should match DoNotEMail = true");

            // string "0" should match false
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, "0"));
            Assert.HasCount(1, results, "string '0' should match DoNotEMail = false");

            // string "true" should match true
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, "true"));
            Assert.HasCount(1, results, "string 'true' should match DoNotEMail = true");

            // string "false" should match false
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.Equal, "false"));
            Assert.HasCount(1, results, "string 'false' should match DoNotEMail = false");

            // NotEqual with int 1 should return the false contact
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.NotEqual, 1));
            Assert.HasCount(1, results, "NotEqual int 1 should return the DoNotEMail = false contact");

            // In operator with int values
            results = service.GetEntities<Contact>(new ConditionExpression(Contact.Fields.DoNotEMail, ConditionOperator.In, 1, 0));
            Assert.HasCount(2, results, "In [1,0] should return both contacts");
        }
    }
}
