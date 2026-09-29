using System.ServiceModel;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
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
        [TestMethod]
        public void N2NJoinEntity_Should_NotBeEditable()
        {
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(nameof(N2NJoinEntity_Should_NotBeEditable));
            var service = new LocalCrmDatabaseOrganizationService(info);
            var accountId = service.Create(new Account());
            var leadId = service.Create(new Lead());

            // The only supported way to create an N:N record is via an Associate
            service.Associate(Lead.EntityLogicalName, leadId, new Relationship(AccountLeads.EntityLogicalName),
                new EntityReferenceCollection { new EntityReference(Account.EntityLogicalName, accountId) });
            var accountLead = service.GetFirst<AccountLeads>();

            try
            {
                service.Create(new AccountLeads());
                Assert.Fail(ExpectedException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Create' method does not support entities of type 'accountleads'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }

            try
            {
                service.Execute(new UpdateRequest { Target = accountLead });
                Assert.Fail(ExpectedException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Update' method does not support entities of type 'accountleads'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }

            try
            {
                service.Delete(accountLead);
                Assert.Fail(ExpectedException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("The 'Delete' method does not support entities of type 'accountleads'. MessageProcessorCache returned MessageProcessor.Empty. ", ex.Detail.Message);
            }
        }

        [TestMethod]
        public void NonN2NEntity_Should_BeEditable()
        {
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(nameof(NonN2NEntity_Should_BeEditable));
            var service = new LocalCrmDatabaseOrganizationService(info);
            // The BusinessUnit contains 3 Guid attributes and no StateCode, but is not an N:N Join Entity
            var businessUnit = new BusinessUnit { Name = nameof(NonN2NEntity_Should_BeEditable) };
            businessUnit.Id = service.Create(businessUnit);
            service.Update(businessUnit);
            service.Delete(businessUnit);
        }

        [TestMethod]
        public void N2NJoinEntity_Associate_Should_BeAllowed()
        {
            var info = LocalCrmDatabaseInfo.Create<CrmContext>(nameof(N2NJoinEntity_Associate_Should_BeAllowed));
            var service = new LocalCrmDatabaseOrganizationService(info);
            var accountId = service.Create(new Account());
            var leadId = service.Create(new Lead());
            var relatedEntities = new EntityReferenceCollection
            {
                new EntityReference(Account.EntityLogicalName, accountId)
            };
            var relationship = new Relationship(AccountLeads.EntityLogicalName);

            service.Associate(Lead.EntityLogicalName, leadId, relationship, relatedEntities);
            Assert.HasCount(1, service.GetEntities(AccountLeads.EntityLogicalName), "The N:N record should have been created!");

            service.Disassociate(Lead.EntityLogicalName, leadId, relationship, relatedEntities);
            Assert.IsEmpty(service.GetEntities(AccountLeads.EntityLogicalName), "The N:N record should have been deleted!");
        }
    }
}
