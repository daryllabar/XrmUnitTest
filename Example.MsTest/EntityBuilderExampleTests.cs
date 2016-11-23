using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Example.MsTestBase;
using Example.MsTestBase.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Example.MsTest
{
    /// <summary>
    /// Entity Builders are used to automatically create default values for entities, as well as allow for reusable instantiation code.
    /// This test example demonstrates how the Account Builder defaults the Account Number
    /// </summary>
    [TestClass]
    public class EntityBuilderExampleTests
    {
        #region CreateWithAccountBuilder_Should_PopulateAccountInfo

        /// <summary>
        /// Example test for being able to run a Unit Test in memory, or against CRM.
        /// </summary>
        [TestMethod]
        public void EntityBuilderExample_CreateWithAccountBuilder_Should_PopulateAccountInfo()
        {
            new CreateWithAccountBuilder_Should_PopulateAccountInfo().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CreateWithAccountBuilder_Should_PopulateAccountInfo : TestMethodClassBase // Test Method Class Base Handles Setup and Cleanup
        {
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("64387F79-5B4A-42F0-BA41-E348A1183379");
            }

            /// <summary>
            /// Initializes the entities that need to be created before the test executes
            /// </summary>
            /// <param name="service">The service.</param>
            protected override void InitializeTestData(IOrganizationService service)
            {
                //
                // Arrange / Act
                // 
                new AccountBuilder(Ids.Account)
                    .WithAddress1()
                    .Create(service);
            }

            /// <summary>
            /// The actual test to perform.  The IOrganization Service passed in is either a local CRM service, or a real connection
            /// Depending on the UnitTestSettings's UseLocalCrm App Setting
            /// </summary>
            /// <param name="service">The service.</param>
            protected override void Test(IOrganizationService service)
            {
                //
                // Assert
                // 
                var account = service.GetEntity(Ids.Account);

                Assert.IsNotNull(account);
                Assert.IsNotNull(account.AccountNumber, "Account Number should have been populated by Builder Account Initializer");
                Assert.IsNotNull(account.Address1_City, "Address 1 City should have been populated by Builder WithAddress1 Call");
            }
        }

        #endregion CreateWithAccountBuilder_Should_PopulateAccountInfo

        #region CreateWithEnvironmentBuilder_Should_PopulateAccountInfo

        [TestMethod]
        public void EntityBuilderExample_CreateWithEnvironmentBuilder_Should_PopulateAccountInfo()
        {
            new CreateWithEnvironmentBuilder_Should_PopulateAccountInfo().Test();
        }

        private class CreateWithEnvironmentBuilder_Should_PopulateAccountInfo : TestMethodClassBase
        {
            private struct Ids
            {
                public struct Accounts
                {
                    public static readonly Id<Account> WithAddress = new Id<Account>("63A3FD8E-BA19-46C2-A1DD-8EBBBA0CF886");
                    public static readonly Id<Account> WithoutAddress = new Id<Account>("CC083E9A-7FFE-4164-AF23-9D7B53F1C323");
                }
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder()
                    .WithBuilder<AccountBuilder>(Ids.Accounts.WithAddress, b => b.WithAddress1())
                    .WithEntities<Ids>() // Creates all Entities within Ids struct
                 // .WithEntities(Ids.Accounts.WithoutAddress) // Allows for specifying specific entities
                    .Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Assert
                //
                Assert.IsNotNull(service.GetEntity(Ids.Accounts.WithAddress).Address1_Line1);
                Assert.IsNull(service.GetEntity(Ids.Accounts.WithoutAddress).Address1_Line1);
            }
        }

        #endregion CreateWithEnvironmentBuilder_Should_PopulateAccountInfo

        #region CreateChildContact_Should_SetParentAccountOnContact

        [TestMethod]
        public void EntityBuilderExample_CreateChildContact_Should_SetParentAccountOnContact()
        {
            new CreateChildContact_Should_SetParentAccountOnContact().Test();
        }

        private class CreateChildContact_Should_SetParentAccountOnContact : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("16769ECA-6B8E-4CEE-ADF2-EDDC93D2F738");
                public static readonly Id<Contact> Contact = new Id<Contact>("819473E1-26F2-4AF6-90E2-D7A437585976");
            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                new CrmEnvironmentBuilder().WithChildEntities(Ids.Account, Ids.Contact).Create(service);
            }

            protected override void Test(IOrganizationService service)
            {
                //
                // Assert
                //
                Assert.AreEqual(Ids.Account, service.GetEntity(Ids.Contact).ParentCustomerId);
            }
        }

        #endregion CreateChildContact_Should_SetParentAccountOnContact
    }
}