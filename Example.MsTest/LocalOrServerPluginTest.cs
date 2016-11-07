using Microsoft.VisualStudio.TestTools.UnitTesting;
using DLaB.Xrm.Entities; // Contains Early Bound Entities
using DLaB.Xrm.Test;
using DLaB.Xrm;
using Example.MsTestBase; // Test Base Project.  Contains code that is shared amoung all Unit Test Projects
using Example.MsTestBase.Builders; // Fluent Builder Namespace.  Builders can be used to create anything that's required, from creating an entity, to a OrganizationService, to a Plugin
using Example.Plugin.Advanced;
using Microsoft.Xrm.Sdk;

namespace Example.MsTest
{
    /// <summary>
    /// Example of a Local or CRM Server Based Plugin test.  
    /// The Test Method Class gives structure to declaring the data that is required by the test, as well as cleaning up the data pre and post test.
    /// Utilizes the Example.MsTestBase.UnitTestSettings.User.config for whether it is in memory or against CRM
    /// </summary>
    [TestClass]
    public class LocalOrServerPluginTest
    {
        #region SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress_Dirty

        /// <summary>
        /// Example of text that can be ran locally or against the server, but is not environment agnostic, and does not clean up the data
        /// </summary>
        [TestMethod]
        public void SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress_Dirty()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var service = TestBase.GetOrganizationService();
            var contactId = service.Create(new Contact());
            var accountId = service.Create(new Account
            {
                PrimaryContactId = new EntityReference(Contact.EntityLogicalName, contactId)
            });
            try
            {
                var contact = new Contact
                {
                    Id = contactId,
                    Address1_Line1 = "742 Evergreen Terrace"
                };

                var plugin = new SyncContactToAccount();
                var context = new PluginExecutionContextBuilder().
                    WithFirstRegisteredEvent(plugin).
                    WithTarget(contact).Build();
                var provider = new ServiceProviderBuilder(service, context, new DebugLogger()).Build();

                //
                // Act
                //
                plugin.Execute(provider);

                //
                // Assert
                //
                var account = service.GetEntity<Account>(accountId);
                Assert.AreEqual(contact.Address1_Line1, account.Address1_Line1);
            }
            finally
            {
                //
                // Clean up
                //
                service.Delete(Account.EntityLogicalName, accountId);
                service.Delete(Contact.EntityLogicalName, contactId);
            }
        }


        #endregion SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress_Dirty

        #region UpdateContactAddress_Should_UpdateAccountAddress

        [TestMethod]
        public void SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress()
        {
            new UpdateContactAddress_Should_UpdateAccountAddress().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class UpdateContactAddress_Should_UpdateAccountAddress : TestMethodClassBase
        {
            // Ids struct is used by the TestMethodClassBase to clean up any entities defined
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("7CF2BB0D-85D4-4B8C-A7B6-371D3C6EA37C");
                public static readonly Id<Contact> Contact = new Id<Contact>("3A080E66-86EB-4D92-A894-2176782E2FF6");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.Account.Entity.PrimaryContactId = Ids.Contact;
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Arrange
                //
                var contact = new Contact
                {
                    Id = Ids.Contact,
                    Address1_Line1 = "742 Evergreen Terrace"
                };

                var plugin = new SyncContactToAccount();
                var context = new PluginExecutionContextBuilder().
                              WithFirstRegisteredEvent(plugin).
                              WithTarget(contact).Build();
                var provider = new ServiceProviderBuilder(service, context, Logger).Build();

                //
                // Act
                //
                plugin.Execute(provider);

                //
                // Assert
                //
                var account = service.GetEntity(Ids.Account);
                Assert.AreEqual(contact.Address1_Line1, account.Address1_Line1);
            }
        }

        #endregion UpdateContactAddress_Should_UpdateAccountAddress
    }
}
