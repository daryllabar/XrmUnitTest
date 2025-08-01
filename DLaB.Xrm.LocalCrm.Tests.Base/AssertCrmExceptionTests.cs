using System;
using System.ServiceModel;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class AssertCrmExceptionTests : BaseTestClass 
    {
        [TestMethod]
        public void LocalCrmTests_AssertCrmException_CaseRequiresCustomer()
        {
            var service = GetService();

            var incident = new Incident();

            var ex = Assert.ThrowsExactly<FaultException<OrganizationServiceFault>>(() => service.Create(incident));
            Assert.AreEqual("You should specify a parent contact or account.", ex.Message);
        }

#if !PRE_KEYATTRIBUTE
        [TestMethod]
        public void LocalCrmTests_AssertCrmException_AlternateKeyNotFound()
        {
            var service = GetService();

            var contact = new Contact
            {
                TransactionCurrencyId = new EntityReference(TransactionCurrency.EntityLogicalName, TransactionCurrency.Fields.CurrencyName, Guid.NewGuid().ToString())
            };
            var ex = Assert.ThrowsExactly<FaultException<OrganizationServiceFault>>(() => service.Create(contact));
            Assert.AreEqual($"A record with the specified key values does not exist in {TransactionCurrency.EntityLogicalName} entity", ex.Message);
        }
#endif
    }
}
