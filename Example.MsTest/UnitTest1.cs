using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Example.MsTestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Example.MsTestBase.Builders;

namespace Example.MsTest
{
    [TestClass]
    public class UnitTest1
    {
        #region CreateAccount_Should_RetrieveByName

        [TestMethod]
        public void CreateAccount_Should_RetrieveByName()
        {
            new CreateAccount_Should_PopulateAccountNumber().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CreateAccount_Should_PopulateAccountNumber : TestMethodClassBase // Test Method Class Base Handles Setup and Cleanup
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
                Ids.Account.Entity.Name = "John Doe";

                // The Crm Environment Builder Handles Creating all Entities.  
                // It can also associate entities together and determine which order entities should be created in
                new CrmEnvironmentBuilder().WithEntities(Ids.Account).Create(service);
            }

            /// <summary>
            /// The actual test to perform.  The IOrganization Service passed in is either a local CRM service, or a real connection
            /// Depending on the UnitTestSettings's UseLocalCrm App Setting
            /// </summary>
            /// <param name="service">The service.</param>
            protected override void Test(IOrganizationService service)
            {
                using (var context = new CrmContext(service))
                {
                    var account = context.AccountSet.FirstOrDefault(c => c.Name == Ids.Account.Entity.Name);
                    Assert.IsNotNull(account);
                    Assert.IsNotNull(account.AccountNumber, "Account Number should have been populated by Builder");
                    account = context.AccountSet.FirstOrDefault(c => c.Name == "Jane Doe");
                    Assert.IsNull(account, "Jane Doe was not added and the query should have returned null");
                }
            }
        }

        #endregion CreateAccount_Should_RetrieveByName
    }
}
