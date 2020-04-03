using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
using XrmUnitTest.Test.Assumptions;

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

        #region TestService_Should_ForceIdsToBeDefined

        [TestMethod]
        public void TestMethodClassBaseDLaB_TestService_Should_ForceIdsToBeDefined()
        {
            new TestService_Should_ForceIdsToBeDefined().Test();
        }

        private class TestService_Should_ForceIdsToBeDefined : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact();

                //
                // Act
                //
                try
                {
                    service.Create(contact);
                }
                catch (AssertFailedException ex)
                {
                    //
                    // Assert
                    //
                    Assert.IsTrue(ex.Message.Contains("An attempt was made to create an entity of type contact without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate."), "Test Service should have had AssertIdNonEmptyOnCreate.");
                    return;
                }
                Assert.Fail("IOrganizationService should enforce Ids being defined");
            }
        }

        #endregion TestService_Should_ForceIdsToBeDefined

        #region AssumptionParentFirst_Should_LoadAssumptions

        [TestMethod]
        public void TestMethodClassBaseDLaB_AssumptionParentFirst_Should_LoadAssumptions()
        {
            new AssumptionParentFirst_Should_LoadAssumptions().Test();
        }

        [AccountDefault]
        [ContactDefault]
        private class AssumptionParentFirst_Should_LoadAssumptions : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                var account = service.GetEntity<Account>(AssumedEntities.Get<AccountDefault>().Id);
                Assert.AreEqual("Default Account", account.Name);
                var contact = service.GetEntity<Contact>(AssumedEntities.Get<ContactDefault>().Id);
                Assert.AreEqual("Default Contact", contact.FirstName + " " + contact.LastName);
            }
        }

        #endregion AssumptionParentFirst_Should_LoadAssumptions

        #region AssumptionChildFirst_Should_LoadAssumptions

        [TestMethod]
        public void TestMethodClassBaseDLaB_AssumptionChildFirst_Should_LoadAssumptions()
        {
            new AssumptionChildFirst_Should_LoadAssumptions().Test();
        }

        [ContactDefault]
        [AccountDefault]
        private class AssumptionChildFirst_Should_LoadAssumptions : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                var account = service.GetEntity<Account>(AssumedEntities.Get<AccountDefault>().Id);
                Assert.AreEqual("Default Account", account.Name);
                var contact = service.GetEntity<Contact>(AssumedEntities.Get<ContactDefault>().Id);
                Assert.AreEqual("Default Contact", contact.FirstName + " " + contact.LastName);
            }
        }

        #endregion AssumptionChildFirst_Should_LoadAssumptions

    }
}
