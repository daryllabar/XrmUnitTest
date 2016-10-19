using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Builders;

namespace DLaB.Xrm.Tests
{
    [TestClass]
    public class ExtensionsIOrganizationServiceTests
    {
        #region FirstOrDefault

        [TestMethod]
        public void Extensions_IOrganizationService_FirstOrDefault()
        {
            new FirstOrDefault().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class FirstOrDefault : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Contact> Contact = new Id<Contact>("A19CFF4C-5599-4BD4-B24A-759A50643BB3");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                // Test Exists
                var contact = service.GetFirstOrDefault<Contact>();
                Assert.IsNotNull(contact);

                // Test Not Exists
                service.Delete<Contact>(Ids.Contact);
                contact = service.GetFirstOrDefault<Contact>();
                Assert.IsNull(contact);
            }
        }

        #endregion FirstOrDefault
    }
}
