using System;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using XrmUnitTest.Test;

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
        [TestMethod]
        public void Execute_UpsertGrandfatherServiceAssertsNonEmpty_Should_UseDefaultIds()
        {
            TestInitializer.InitializeTestSettings();
            var id = new Id<Account>(Guid.NewGuid());

            var service = new OrganizationServiceBuilder(TestBase.GetOrganizationService())
                .AssertIdNonEmptyOnCreate()
                .Build();

            service = new OrganizationServiceBuilder(service)
               .WithFakeDelete((s, n, i) => { })
               .Build();

            service = new OrganizationServiceBuilder(service)
                .WithIdsDefaultedForCreate(id).Build();


            var sut = (FakeIOrganizationService)service;
            var response = sut.Upsert(new Account());
            Assert.AreEqual(id.EntityId, response.Target.Id);
        }

        [TestMethod]
        public void Execute_UpsertFatherServiceAssertsNonEmpty_Should_UseDefaultIds()
        {
            TestInitializer.InitializeTestSettings();
            var id = new Id<Account>(Guid.NewGuid());

            var service = new OrganizationServiceBuilder(TestBase.GetOrganizationService())
                .AssertIdNonEmptyOnCreate()
                .Build();

            service = new OrganizationServiceBuilder(service)
                .WithIdsDefaultedForCreate(id).Build();


            var sut = (FakeIOrganizationService)service;
            var response = sut.Upsert(new Account());
            Assert.AreEqual(id.EntityId, response.Target.Id);
        }

        [TestMethod]
        public void Execute_UpsertSingleService_Should_UseDefaultIds()
        {
            TestInitializer.InitializeTestSettings();
            var id = new Id<Account>(Guid.NewGuid());

            var service = new OrganizationServiceBuilder(TestBase.GetOrganizationService())
                .AssertIdNonEmptyOnCreate()
                .WithIdsDefaultedForCreate(id)
                .Build();

            var sut = (FakeIOrganizationService)service;
            var response = sut.Upsert(new Account());
            Assert.AreEqual(id.EntityId, response.Target.Id);

        }

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

        //[TestMethod]
        //public void FakeIOrganizationService_InsertAt_Should_ChangeOrderOfFakes()
        //{
        //    TestInitializer.InitializeTestSettings();
        //    var service = InitializeService();
        //    Run("1");
        //
        //    service = InitializeService();
        //    service.InsertAt(0);
        //    Run("3");
        //    return;
        //
        //    void Run(string expectedName)
        //    {
        //        var account = new Account();
        //        account.Id = service.Create(account);
        //        Assert.AreEqual(expectedName, service.GetEntity<Account>(account.Id).Name);
        //    }
        //}
        //
        //private static FakeIOrganizationService InitializeService()
        //{
        //    var services = new List<FakeIOrganizationService>();
        //
        //    services.Add((FakeIOrganizationService) new OrganizationServiceBuilder(TestBase.GetOrganizationService())
        //        .WithFakeCreate((s, e) => {
        //            e["name"] = "1";
        //            return s.Create(e);
        //        })
        //        .Build());
        //
        //    services.Add((FakeIOrganizationService) new OrganizationServiceBuilder(services.Last())
        //        .WithFakeCreate((s, e) => {
        //            e["name"] = "2";
        //            return s.Create(e);
        //        })
        //        .Build());
        //
        //    services.Add((FakeIOrganizationService)new OrganizationServiceBuilder(services.Last())
        //        .WithFakeCreate((s, e) => {
        //            e["name"] = "3";
        //            return s.Create(e);
        //        })
        //        .Build());
        //
        //    var service = services.Last();
        //    return service;
        //}
    }
}
