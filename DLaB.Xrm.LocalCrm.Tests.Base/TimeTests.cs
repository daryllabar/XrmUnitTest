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

        [TestMethod]
        public void LocalCrmTests_Create_OverridenModifiedOn_SetsModifiedOnAndCreatedOn()
        {
            var timeProvider = new TestTimeProvider();
            var service = new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(new LocalCrmDatabaseOptionalSettings
            {
                TimeProvider = timeProvider
            }));
            timeProvider.UtcNow = new DateTime(2024, 1, 10);
            var overriddenModifiedOn = new DateTime(2024, 1, 8);

            var account = new Account { OverriddenCreatedOn = new DateTime(2024, 1, 9) };
            account["overridenmodifiedon"] = overriddenModifiedOn;
            var id = service.Create(account);

            var created = service.GetEntity<Account>(id);
            Assert.AreEqual(overriddenModifiedOn, created.ModifiedOn);
            Assert.AreEqual(overriddenModifiedOn, created.CreatedOn);
        }

        [TestMethod]
        public void LocalCrmTests_Create_OverridenModifiedOn_DoesNotUpdateCreatedOnWhenEarlier()
        {
            var timeProvider = new TestTimeProvider();
            var service = new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(new LocalCrmDatabaseOptionalSettings
            {
                TimeProvider = timeProvider
            }));
            timeProvider.UtcNow = new DateTime(2024, 1, 10);
            var overriddenModifiedOn = new DateTime(2024, 1, 8);
            var overriddenCreatedOn = new DateTime(2024, 1, 5);

            var account = new Account { OverriddenCreatedOn = overriddenCreatedOn };
            account["overridenmodifiedon"] = overriddenModifiedOn;
            var id = service.Create(account);

            var created = service.GetEntity<Account>(id);
            Assert.AreEqual(overriddenModifiedOn, created.ModifiedOn);
            Assert.AreEqual(overriddenCreatedOn, created.CreatedOn);
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
