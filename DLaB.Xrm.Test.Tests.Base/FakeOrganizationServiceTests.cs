using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Entities;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmUnitTest.Test;
using Microsoft.Xrm.Sdk.Query;

#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class FakeOrganizationServiceTests
    {

        #region FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey

        [TestMethod]
        public void FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey()
        {
            TestInitializer.InitializeTestSettings();
            var sut = new FakeIOrganizationService(TestBase.GetOrganizationService());
            var account = new Account
            {
                Name = "1st"
            };
            account.Id = sut.Create(account);

            var request = new RetrieveRequest
            {
                ColumnSet = new ColumnSet(),
                Target = new EntityReference(Account.EntityLogicalName, Account.Fields.Name, account.Name)
            };

            var response = (RetrieveResponse)sut.Execute(request);

            Assert.AreEqual(account.Id, response.Entity.Id);
        }

        #endregion FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey


        [TestMethod]
        public void FakeIOrganizationService_InsertAt_Should_ChangeOrderOfFakes()
        {
            TestInitializer.InitializeTestSettings();
            var service = InitializeService();
            Run("1");

            service = InitializeService();
            service.InsertAt(0);
            Run("3");
            return;

            void Run(string expectedName)
            {
                var account = new Account();
                account.Id = service.Create(account);
                Assert.AreEqual(expectedName, service.GetEntity<Account>(account.Id).Name);
            }
        }

        private static FakeIOrganizationService InitializeService()
        {
            var services = new List<FakeIOrganizationService>();

            services.Add((FakeIOrganizationService) new OrganizationServiceBuilder(TestBase.GetOrganizationService())
                .WithFakeCreate((s, e) => {
                    e["name"] = "1";
                    return s.Create(e);
                })
                .Build());

            services.Add((FakeIOrganizationService) new OrganizationServiceBuilder(services.Last())
                .WithFakeCreate((s, e) => {
                    e["name"] = "2";
                    return s.Create(e);
                })
                .Build());

            services.Add((FakeIOrganizationService)new OrganizationServiceBuilder(services.Last())
                .WithFakeCreate((s, e) => {
                    e["name"] = "3";
                    return s.Create(e);
                })
                .Build());

            var service = services.Last();
            return service;
        }
    }
}
