using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class AggregateFetchXmlTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Aggregate_Sum()
        {
            var service = GetService();
            var id = service.Create(new Account());
            service.Create(new Contact
            {
                AnnualIncome = new Money(10m),
                ParentCustomerId = new EntityReference(Account.EntityLogicalName, id)
            });


            Assert.AreEqual(10m, GetSum(id, service));

            service.Create(new Contact
            {
                AnnualIncome = new Money(10m),
                ParentCustomerId = new EntityReference(Account.EntityLogicalName, id)
            });

            Assert.AreEqual(20m, GetSum(id, service));
        }

        private static decimal GetSum(Guid id, IOrganizationService service)
        {
            var fetchXml = $@"<fetch aggregate='true' >
              <entity name='contact' >
                <attribute name='annualincome' alias='SumOfAnnualIncome' aggregate='sum' />
                <filter>
                  <condition attribute='accountid' operator='eq' value='{id}' />
                </filter>
              </entity>
            </fetch>
            ";
            var entity = service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities[0];
            Assert.AreEqual(1, entity.Attributes.Count);
            return entity.GetAliasedValue<Money>("SumOfAnnualIncome").GetValueOrDefault();
        }
    }
}
