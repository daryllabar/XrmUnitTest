#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif
using DLaB.Xrm.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class QualifyLeadTests: BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_QualifyLead_Should_CreateEntities()
        {
            var service = GetService();
            var lead = new Lead();
            lead.Id = service.Create(lead);
            var response = (QualifyLeadResponse) service.Execute(new QualifyLeadRequest
            {
                CreateContact = true,
                CreateOpportunity = true,
                CreateAccount = true,
                LeadId  = lead.ToEntityReference(),
                Status = new OptionSetValue(3)
            });

            var opp = service.GetEntitiesById<Opportunity>(response.CreatedEntities.First(e => e.LogicalName == Opportunity.EntityLogicalName).Id).First();
            AssertCrm.Exists(service, opp.AccountId);
            AssertCrm.Exists(service, opp.ContactId);
        }
    }
}
