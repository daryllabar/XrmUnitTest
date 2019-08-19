using DLaB.Xrm.Test.Assumptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Assumptions;

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class AssumedEntitiesTests
    {
        [TestMethod]
        public void AssumedEntities_Load_Should_LoadAssumedEntities()
        {
            TestInitializer.InitializeTestSettings();
            var service = TestBase.GetOrganizationService();
            var assumptions = new AssumedEntities();
            assumptions.Load(service, new AccountDefault(), new ContactDefault());
            AssertCrm.Exists(service, assumptions.Get<AccountDefault>());
            AssertCrm.Exists(service, assumptions.Get<ContactDefault>());
        }
    }
}
