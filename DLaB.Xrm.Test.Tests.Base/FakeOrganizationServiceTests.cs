using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmUnitTest.Test;
using Microsoft.Xrm.Sdk.Query;
using System.Web.Services.Description;
#if NET
using DataverseUnitTest.Assumptions;
using DataverseUnitTest;
#else
using DLaB.Xrm.Test.Assumptions;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class FakeOrganizationServiceTests
    {

        #region FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey
#if !PRE_KEYATTRIBUTE

        [TestMethod]
        public void FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey()
        {
            TestInitializer.InitializeTestSettings();
            var sut = new FakeIOrganizationService(TestBase.GetOrganizationService());
            var account = new Account
            {
                Name = "1st"
            };
            account.Id = sut.Create(account);

            var request = new RetrieveRequest
            {
                ColumnSet = new ColumnSet(),
                Target = new EntityReference(Account.EntityLogicalName, Account.Fields.Name, account.Name)
            };

            var response = (RetrieveResponse)sut.Execute(request);

            Assert.AreEqual(account.Id, response.Entity.Id);
        }

#endif
        #endregion FakeIOrganizationService_Execute_Should_RetrieveRequestByAltKey

    }
}
