using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;

namespace DLaB.Xrm.LocalCrm.Tests
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/queryexpression/aggregate-data
    /// </summary>
    [TestClass]
    public class AggregateQueryExpressionTests : BaseTestClass
    {

        [TestMethod]
        public void LocalCrmTests_AggregateGroupBy()
        {
            var service = GetService();
            CreateSampleData(service);
            QueryExpression qe = new()
            {
                EntityName = "account",
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        {
                            new XrmAttributeExpression(
                                attributeName: "numberofemployees",
                                alias: "Total",
                                aggregateType: XrmAggregateType.Sum)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: "address1_city",
                                alias: "Count",
                                aggregateType: XrmAggregateType.Count)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: "address1_city",
                                alias: "City",
                                aggregateType: XrmAggregateType.None){
                                HasGroupBy = true
                            }
                        }
                    }
                }
            };
            qe.Orders.Add(new OrderExpression(
                attributeName: "address1_city",
                alias: "City",
                orderType: OrderType.Ascending));
            var accounts = service.RetrieveMultiple(qe).ToEntityList<Account>();

            AssertCityCounts(0, 1, null);
            AssertCityCounts(6000, 1, "Dallas");
            AssertCityCounts(2900, 1, "Los Angeles");
            AssertCityCounts(2700, 1, "Lynnwood");
            AssertCityCounts(4800, 1, "Missoula");
            AssertCityCounts(3900, 1, "Phoenix");
            AssertCityCounts(10600, 3, "Redmond");
            AssertCityCounts(4300, 1, "Santa Cruz");
            return;

            void AssertCityCounts(int total, int count, string city)
            {
                var account = accounts.Single(a => a.GetAliasedValue<string>("City") == city);
                Assert.AreEqual(total, account.GetAliasedValue<int>("Total"), "Incorrect Total");
                Assert.AreEqual(count, account.GetAliasedValue<int>("Count"), "Incorrect Count");
                Assert.AreEqual(city, account.GetAliasedValue<string>("City"), "Incorrect City");
            }
        }

        [TestMethod]
        public void LocalCrmTests_AggregateGroupByDateGrouping()
        {
            var service = GetService();
            CreateSampleData(service);
            QueryExpression qe = new()
            {
                EntityName = "account",
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
            {
                new XrmAttributeExpression(
                    attributeName: "numberofemployees",
                    alias: "Total",
                    aggregateType: XrmAggregateType.Sum)
            },
            {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "Day",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.Day
                }
            },
            {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "Week",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.Week
                }
            },
                                    {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "Month",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.Month
                }
            },
            {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "Year",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.Year
                }
            },
            {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "FiscalPeriod",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.FiscalPeriod
                }
            },
            {
                new XrmAttributeExpression(
                    attributeName: "createdon",
                    alias: "FiscalYear",
                    aggregateType: XrmAggregateType.None){
                    HasGroupBy = true,
                    DateTimeGrouping = XrmDateTimeGrouping.FiscalYear
                }
            }
        }
                }
            };
            qe.Orders.Add(new OrderExpression(
                        attributeName: "createdon",
                        alias: "Month",
                        orderType: OrderType.Ascending));
            var accounts = service.RetrieveMultiple(qe).ToEntityList<Account>();
            AssertTotalCounts(35200, 25, 12, 3, 2023, "Quarter 1 FY2023", "FY2023");
            AssertTotalCounts(0, 27, 35, 8, 2023, "Quarter 3 FY2023", "FY2023");

            return;

            void AssertTotalCounts(int total, int day, int week, int month, int year, string fiscalPeriod, string fiscalYear)
            {
                var account = accounts.Single(a => a.GetAliasedValue<int>("Day") == day);
                Assert.AreEqual(total, account.GetAliasedValue<int>("Total"), "Incorrect Total");
                Assert.AreEqual(day, account.GetAliasedValue<int>("Day"), "Incorrect Day");
                Assert.AreEqual(week, account.GetAliasedValue<int>("Week"), "Incorrect Week");
                Assert.AreEqual(month, account.GetAliasedValue<int>("Month"), "Incorrect Month");
                Assert.AreEqual(year, account.GetAliasedValue<int>("Year"), "Incorrect Year");
                Assert.AreEqual(fiscalPeriod, account.GetAliasedValue<string>("FiscalPeriod"), "Incorrect Fiscal Period");
                Assert.AreEqual(fiscalYear, account.GetAliasedValue<string>("FiscalYear"), "Incorrect Fiscal Year");
            }
        }

        [TestMethod]
        public void LocalCrmTests_AggregateTypes()
        {
            var service = GetService();
            CreateSampleData(service);

            QueryExpression qe = new()
            {
                EntityName = Account.EntityLogicalName,
                ColumnSet = new ColumnSet(false)
                {
                    AttributeExpressions = {
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Average",
                                aggregateType: XrmAggregateType.Avg)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Count",
                                aggregateType: XrmAggregateType.Count)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "ColumnCount",
                                aggregateType: XrmAggregateType.CountColumn)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Maximum",
                                aggregateType: XrmAggregateType.Max)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Minimum",
                                aggregateType: XrmAggregateType.Min)
                        },
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Sum",
                                aggregateType: XrmAggregateType.Sum)
                        }
                    }
                }
            };

            var account = service.RetrieveMultiple(qe).ToEntityList<Account>().First();
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
        public void LocalCrmTests_InvalidAggregateQueries()
        {
            var service = GetService();
            CreateSampleData(service);
            QueryExpression qe = new()
            {
                EntityName = Account.EntityLogicalName,
                ColumnSet = new ColumnSet(true)
                {
                    AttributeExpressions = {
                        {
                            new XrmAttributeExpression(
                                attributeName: Account.Fields.NumberOfEmployees,
                                alias: "Average",
                                aggregateType: XrmAggregateType.Avg)
                        },
                    }
                }
            };

            try
            {
                service.GetFirst<Account>(qe);
                Assert.Fail("Aggregate Exception Expected");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("Attribute can not be specified if an aggregate operation is requested.", ex.Detail.Message, "Exception message does not contain expected text.");
            }

            try
            {
                qe.ColumnSet.AllColumns = false;
                qe.ColumnSet.AddColumn(Account.Fields.Name);
                service.GetFirst<Account>(qe);
                Assert.Fail("Aggregate Exception Expected");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("Attribute can not be specified if an aggregate operation is requested.", ex.Detail.Message, "Exception message does not contain expected text.");
            }

            qe.ColumnSet.Columns.Clear();
            qe.ColumnSet.AttributeExpressions.Add(new XrmAttributeExpression(
                attributeName: Account.Fields.Name,
                alias: "Name",
                aggregateType: XrmAggregateType.None)
            {
                HasGroupBy = false
            });
            service.GetFirst<Account>(qe);

            var qe2 = QueryExpressionFactory.Create<ActivityPointer>();
            qe2.ColumnSet.AttributeExpressions.Add(new XrmAttributeExpression
            {
                AggregateType = XrmAggregateType.Max,
                AttributeName = ActivityPointer.Fields.ModifiedOn,
                Alias = "LastActivityDate",
            });
            qe2.ColumnSet.AttributeExpressions.Add(new XrmAttributeExpression
            {
                AggregateType = XrmAggregateType.None,
                AttributeName = ActivityPointer.Fields.RegardingObjectId,
                Alias = "RegardingId",
                HasGroupBy = true,
            });
            try
            {
                service.GetFirstOrDefault<ActivityPointer>(qe2);
                Assert.Fail("Aggregate Exception Expected");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Assert.AreEqual("Attribute can not be specified if an aggregate operation is requested.", ex.Detail.Message, "Exception message does not contain expected text.");
            }
            qe2.ColumnSet.AllColumns = false;
            service.GetFirstOrDefault(qe);

            qe.ColumnSet.AttributeExpressions.Clear();
            Assert.IsNotNull(service.GetFirst<Account>());

            var results = service.RetrieveMultiple(new FetchExpression(@"<fetch xmlns:generator=""MarkMpn.SQL4CDS"">
  <entity name=""account"">
    <attribute name=""name"" />
    <filter>
      <condition attribute=""name"" operator=""not-in"">
        <value>60</value>
        <value>62</value>
        <value>300</value>
        <value>80</value>
      </condition>
    </filter>
    <order attribute=""name"" />
  </entity>
</fetch>"));

            Assert.AreNotEqual(0, results.Entities.Count, "Expected to find some accounts, but found none.");
        }

        public static void CreateSampleData(IOrganizationService service)
        {
            service.Create(new Account { NumberOfEmployees = null, Name = "Example Account ", 			Address1_City = null, 			OverriddenCreatedOn = new DateTime(2023, 8, 27) });
            service.Create(new Account { NumberOfEmployees = 1500, Name = "Contoso Pharmaceuticals", 	Address1_City = "Redmond", 		OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 2700, Name = "Fabrikam, Inc.", 			Address1_City = "Lynnwood", 	OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 2900, Name = "Blue Yonder Airlines", 		Address1_City = "Los Angeles", 	OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 2900, Name = "City Power & Light", 		Address1_City = "Redmond", 		OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 3900, Name = "Coho Winery", 				Address1_City = "Phoenix", 		OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 4300, Name = "Adventure Works", 			Address1_City = "Santa Cruz", 	OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 4800, Name = "Alpine Ski House", 			Address1_City = "Missoula", 	OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 6000, Name = "Litware, Inc.", 				Address1_City = "Dallas", 		OverriddenCreatedOn = new DateTime(2023, 3, 25) });
            service.Create(new Account { NumberOfEmployees = 6200, Name = "A.Datum Corporation", 		Address1_City = "Redmond", 		OverriddenCreatedOn = new DateTime(2023, 3, 25) });
        }
    }
}
