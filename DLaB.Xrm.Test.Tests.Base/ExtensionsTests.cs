using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class FakeIOrganizationServiceTests
    {
        [TestMethod]
        public void IServiceProvider_GetFake_Should_RetrieveTheInstance()
        {
            var provider = new FakeServiceProvider();
            var service = new FakeIOrganizationService(new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>()));
            provider.AddService<IOrganizationService>(service);
            var fake = provider.GetFake<FakeIOrganizationService>();
            Assert.IsNotNull(fake);
            Assert.AreEqual(service, fake);
        }
    }
}
