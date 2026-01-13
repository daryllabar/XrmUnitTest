using System.ServiceModel;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using CrmContext = DLaB.Xrm.Entities.CrmContext;

#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class InvalidEntityOperationTests
    {
        private const string ExpectedException = "Exception should have been thrown!";

        [TestMethod]
        public void Poa_Should_NotBeEditable()
        {
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(nameof(Poa_Should_NotBeEditable));
            info.AllowCrudOperationsForEntities.Add(PrincipalObjectAccess.EntityLogicalName);
            var service = new LocalCrmDatabaseOrganizationService(info);
            var poa = new PrincipalObjectAccess();
            poa.Id = service.Create(poa); // Normally not allowed

            info.AllowCrudOperationsForEntities.Remove(PrincipalObjectAccess.EntityLogicalName);
            try
            {
                service.Create(new PrincipalObjectAccess());
                Assert.Fail(ExpectedException);
            }
            catch(FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Create' method does not support entities of type 'principalobjectaccess'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }

            try
            {
                service.Update(poa);
                Assert.Fail(ExpectedException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Update' method does not support entities of type 'principalobjectaccess'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }

            try
            {
                service.Delete(poa);
                Assert.Fail(ExpectedException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Delete' method does not support entities of type 'principalobjectaccess'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }
        }
    }
}
