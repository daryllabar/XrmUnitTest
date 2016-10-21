using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
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
    }
}
