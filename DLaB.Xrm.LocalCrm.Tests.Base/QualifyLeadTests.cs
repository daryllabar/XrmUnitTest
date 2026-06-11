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
            var lead = new Lead();
            lead.Id = Service.Create(lead);
            var response = (QualifyLeadResponse) Service.Execute(new QualifyLeadRequest
            {
                CreateContact = true,
                CreateOpportunity = true,
                CreateAccount = true,
                LeadId  = lead.ToEntityReference(),
                Status = new OptionSetValue(3)
            });

            var opp = Service.GetEntitiesById<Opportunity>(response.CreatedEntities.First(e => e.LogicalName == Opportunity.EntityLogicalName).Id).First();
            AssertCrm.Exists(Service, opp.AccountId);
            AssertCrm.Exists(Service, opp.ContactId);
        }
    }
}
