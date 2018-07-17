using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Test;
using DLaB.Xrm.Tests.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Tests
{
    [TestClass]
    public class ExtendedOrganizationServiceTests
    {
        public IOrganizationService Service { get; set; }
        public FakeTraceService Trace { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Trace = new FakeTraceService(new DebugLogger());
            Service = new ExtendedOrganizationService(new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>()), Trace);
        }

        [TestMethod]
        public void ExtendedOrganizationService_Create_ShouldTrace()
        {
            var contact = new Contact {Id = Guid.NewGuid(), FirstName = nameof(ExtendedOrganizationService_Create_ShouldTrace) };
            Service.Create(contact);
            AssertTraceContains(contact.Id, "Create Request", nameof(ExtendedOrganizationService_Create_ShouldTrace));
            AssertTraceContains("Timer Ended", "seconds");
        }

        [TestMethod]
        public void ExtendedOrganizationService_Retrieve_ShouldTrace()
        {
            var contact = new Contact { Id = Guid.NewGuid() };
            Service.Create(contact);
            Trace.Traces.Clear();

            Service.GetEntity<Contact>(contact.Id, c => new { c.FirstName, c.LastName });
            AssertTraceContains(contact.Id, "Retrieve Request", Contact.Fields.FirstName, Contact.Fields.LastName);
            AssertTraceContains("Timer Ended", "seconds");
        }

        [TestMethod]
        public void ExtendedOrganizationService_RetrieveMultiple_ShouldTrace()
        {
            Service.GetEntities<Contact>(c => new { c.Address1_City},
                Contact.Fields.FirstName, "Hello", 
                Contact.Fields.LastName, "World");
            AssertTraceContains("Hello", "World", "Retrieve Multiple Request", Contact.Fields.FirstName, Contact.Fields.LastName, Contact.Fields.Address1_City);
            AssertTraceContains("Returned: 0");
            AssertTraceContains("Timer Ended", "seconds");
        }

        [TestMethod]
        public void ExtendedOrganizationService_Update_ShouldTrace()
        {
            var contact = new Contact { Id = Guid.NewGuid() };
            Service.Create(contact);
            Trace.Traces.Clear();

            contact.FirstName = "Hello";
            Service.Update(contact);
            AssertTraceContains(contact.Id, "Update Request", Contact.Fields.FirstName, "Hello");
            AssertTraceContains("Timer Ended", "seconds");
        }

        [TestMethod]
        public void ExtendedOrganizationService_Delete_ShouldTrace()
        {
            var contact = new Contact { Id = Guid.NewGuid() };
            Service.Create(contact);
            Trace.Traces.Clear();

            Service.Delete(contact);
            AssertTraceContains(contact.Id, "Delete Request", Contact.EntityLogicalName);
            AssertTraceContains("Timer Ended", "seconds");
        }

        [TestMethod]
        public void ExtendedOrganizationService_Execute_ShouldTrace()
        {
            Service.GetCurrentlyExecutingUserInfo();
            AssertTraceContains("WhoAmI");
            AssertTraceContains("Timer Ended", "seconds");
        }


        public void AssertTraceContains(params object[] values)
        {
            var oneContainsAll = Trace.Traces.Any(t => values.All(v => t.Trace.Contains(v.ToString())));
            if (!oneContainsAll)
            {
                Assert.Fail(string.Join(", ", Trace.Traces.Select(t => t.Trace)) + " did not contain " + string.Join(", ", values.Select(v => v.ToString())));
            }
        }
    }
}
