using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

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
            Assert.AreEqual(2, entity.Attributes.Count);
            return entity.GetAliasedValue<Money>("SumOfAnnualIncome").GetValueOrDefault();
        }

        [TestMethod]
        public void LocalCrmTests_AggregateTypes()
        {
            var service = GetService();
            AggregateQueryExpressionTests.CreateSampleData(service);
            var fetch = new FetchExpression("""
                                            <fetch aggregate='true'>
                                              <entity name='account'>
                                                <attribute name='numberofemployees' alias='Average' aggregate='avg' />
                                                <attribute name='numberofemployees' alias='Count' aggregate='count' />
                                                <attribute name='numberofemployees' alias='ColumnCount' aggregate='countcolumn' />
                                                <attribute name='numberofemployees' alias='Maximum' aggregate='max' />
                                                <attribute name='numberofemployees' alias='Minimum' aggregate='min' />
                                                <attribute name='numberofemployees' alias='Sum' aggregate='sum' />
                                              </entity>
                                            </fetch>
                                            """);

            var account = service.RetrieveMultiple(fetch).ToEntityList<Account>().First();
            //--------------------------------------------------------------
            // | Average | Count | ColumnCount | Maximum | Minimum | Sum    |
            // --------------------------------------------------------------
            // | 3,911   | 10    | 9           | 6,200   | 1,500   | 35,200 |
            // --------------------------------------------------------------
            Assert.AreEqual(3911, account.GetAliasedValue<int>("Average"), "Incorrect Average");
            Assert.AreEqual(10, account.GetAliasedValue<int>("Count"), "Incorrect Count");
            Assert.AreEqual(9, account.GetAliasedValue<int>("ColumnCount"), "Incorrect ColumnCount");
            Assert.AreEqual(6200, account.GetAliasedValue<int>("Maximum"), "Incorrect Maximum");
            Assert.AreEqual(1500, account.GetAliasedValue<int>("Minimum"), "Incorrect Minimum");
            Assert.AreEqual(35200, account.GetAliasedValue<int>("Sum"), "Incorrect Sum");
        }

        [TestMethod]
        public void LocalCrmTests_SumWithGroupBy()
        {
            var service = GetService();
            AggregateQueryExpressionTests.CreateSampleData(service);
            var fetch = new FetchExpression("""
                                            <fetch aggregate='true'>
                                              <entity name='account'>
                                                <attribute name='address1_city' alias='City' groupby='true' />
                                                <attribute name='numberofemployees' alias='Sum' aggregate='sum' />
                                              </entity>
                                            </fetch>
                                            """);

            var account = service.RetrieveMultiple(fetch).ToEntityList<Account>().First();
        }
    }
}
