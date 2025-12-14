using System;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class TimeTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Owner_SystemUserBusinessUnitSet()
        {
            var timeProvider = new TestTimeProvider();
            var service = new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(new LocalCrmDatabaseOptionalSettings
            {
                TimeProvider = timeProvider
            }));
            timeProvider.UtcNow = new DateTime(2024, 1, 1);
            var contact = service.GetEntity<Contact>(service.Create(new Contact()));
            Assert.AreEqual(timeProvider.UtcNow, contact.CreatedOn);
            Assert.AreEqual(timeProvider.UtcNow, contact.ModifiedOn);
        }
    }
    
    public class TestTimeProvider : ITimeProvider
    {
        public DateTime UtcNow { get; set; }
        public TestTimeProvider()
        {
            UtcNow = DateTime.UtcNow;
        }
        public DateTime GetUtcNow()
        {
            return UtcNow;
        }
    }
}
