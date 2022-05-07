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
        [ExpectedException(typeof(FaultException<OrganizationServiceFault>))]
        public void LocalCrmTests_AssertCrmException_CaseRequiresCustomer()
        {
            var service = GetService();

            var incident = new Incident();
            try
            {
                service.Create(incident);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("You should specify a parent contact or account.", ex.Message);
                throw;
            }
        }

#if !PRE_KEYATTRIBUTE
        [TestMethod]
        [ExpectedException(typeof(FaultException<OrganizationServiceFault>))]
        public void LocalCrmTests_AssertCrmException_AlternateKeyNotFound()
        {
            var service = GetService();

            var contact = new Contact
            {
                TransactionCurrencyId = new EntityReference(TransactionCurrency.EntityLogicalName, TransactionCurrency.Fields.CurrencyName, Guid.NewGuid().ToString())
            };
            try
            {
                service.Create(contact);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual($"A record with the specified key values does not exist in {TransactionCurrency.EntityLogicalName} entity", ex.Message);
                throw;
            }
        }
#endif
    }
}
