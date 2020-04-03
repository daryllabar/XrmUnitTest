using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test.Assumptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Assumptions;

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class AssumedEntitiesTests
    {
        [TestInitialize]
        public void InitializeTestSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

        [TestMethod]
        public void AssumedEntities_Load_Should_LoadAssumedEntities()
        {
            var service = TestBase.GetOrganizationService();
            var assumptions = LoadTestAssumptions(service);
            AssertCrm.Exists(service, assumptions.Get<AccountDefault>());
            AssertCrm.Exists(service, assumptions.Get<ContactDefault>());
        }

        [TestMethod]
        public void AssumedEntities_Get_Should_ReturnEntitiesOfType()
        {
            var service = TestBase.GetOrganizationService();
            var assumptions = LoadTestAssumptions(service);
            Assert.AreEqual(assumptions.Get<AccountDefault>().Id, assumptions.Get(new AccountDefault()).Id);
            Assert.AreEqual(assumptions.Get<ContactDefault>().Id, assumptions.Get(new ContactDefault()).Id);
            Assert.AreEqual(assumptions.Get<AccountDefault>().Id, assumptions.Get<AccountDefault, Account>().Id);
            Assert.AreEqual(assumptions.Get<ContactDefault>().Id, assumptions.Get<ContactDefault,Contact>().Id);
        }


        [TestMethod]
        public void AssumedEntities_GetAll_Should_ReturnEntitiesOfType()
        {
            var service = TestBase.GetOrganizationService();
            var assumptions = LoadTestAssumptions(service);
            Assert.AreEqual(assumptions.Get<AccountDefault>().Id, assumptions.GetAll<Account>().First().Id);
            Assert.AreEqual(assumptions.Get<ContactDefault>().Id, assumptions.GetAll<Contact>().First().Id);
        }

        [TestMethod]
        public void AssumedEntities_GetId_Should_ReturnAssumedEntity()
        {
            var service = TestBase.GetOrganizationService();
            var assumptions = LoadTestAssumptions(service);
            var id = assumptions.GetId(new AccountDefault());
            Assert.AreEqual(assumptions.Get<AccountDefault>().Id, id);
            Assert.AreEqual(assumptions.Get<AccountDefault>().LogicalName, id);
        }

        [TestMethod]
        public void AssumedEntities_MultiThreadedGet_Should_ReturnAssumedEntity()
        {
            var items = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            Parallel.ForEach(items, item =>
            {
                var service = TestBase.GetOrganizationService(Guid.NewGuid().ToString());
                var assumptions = LoadTestAssumptions(service);
                var id = assumptions.GetId(new AccountDefault());
                Assert.AreEqual(assumptions.Get<AccountDefault>().Id, id);
                Assert.AreEqual(assumptions.Get<AccountDefault>().LogicalName, id);
            });
        }

        private AssumedEntities LoadTestAssumptions(IOrganizationService service)
        {
            var assumptions = new AssumedEntities();
            assumptions.Load(service, new AccountDefault(), new ContactDefault());
            return assumptions;
        }
    }
}
