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
        private class CreateAccount_Should_PopulateAccountNumber : TestMethodClassBase
        {
            private struct Ids
            {
                public static readonly Id<Account> Account = new Id<Account>("64387F79-5B4A-42F0-BA41-E348A1183379");

            }

            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.Account.Entity.Name = "John Doe";

                new CrmEnvironmentBuilder().WithEntities(Ids.Account).Create(service);
            }

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
