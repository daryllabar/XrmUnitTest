using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class TestMethodClassBaseDLaBTests
    {
        #region GivenEntityIds_Should_CleanupTestsBeforeTestRun

        [TestMethod]
        public void TestMethodClassBaseDLaB_GivenEntityIds_Should_CleanupTestsBeforeTestRun()
        {
            new GivenEntityIds_Should_CleanupTestsBeforeTestRun().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class GivenEntityIds_Should_CleanupTestsBeforeTestRun : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id Contact = new Id<Contact>("8072dbf6-f966-43ec-83af-5b55b067be80");
            }

            protected override void CleanupDataPreInitialization(IOrganizationService service)
            {
                service.Create(Ids.Contact.Entity);
                base.CleanupDataPreInitialization(service);
            }

            protected override void Test(IOrganizationService service)
            {
                // Test happens CleanupTestData 
                AssertCrm.NotExists(Ids.Contact, "Contact Should have been deleted up by the CleanupDataPreInitialization");
            }
        }

        #endregion GivenEntityIds_Should_CleanupTestsBeforeTestRun
    }
}
