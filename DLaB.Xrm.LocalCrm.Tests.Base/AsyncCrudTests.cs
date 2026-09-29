#if NET
using System.Threading;
using DLaB.Xrm.Client;
using DLaB.Xrm.Entities;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using Task = System.Threading.Tasks.Task;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class AsyncCrudTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Async_ServiceImplementsIOrganizationServiceAsync2()
        {
            Assert.IsInstanceOfType<IOrganizationServiceAsync2>(Service);
            Assert.IsInstanceOfType<IOrganizationServiceAsync2>(new ClientSideOrganizationService(Service));
        }

        [TestMethod]
        public async Task LocalCrmTests_Async_CreateRetrieveUpdateDelete()
        {
            IClientSideOrganizationService service = Service;
            var id = await service.CreateAsync(new Account { Name = "Async" });
            var account = await service.RetrieveAsync(Account.EntityLogicalName, id, new ColumnSet(true));
            Assert.AreEqual("Async", account.ToEntity<Account>().Name);

            await service.UpdateAsync(new Account { Id = id, Name = "Async Updated" });
            account = await service.RetrieveAsync(Account.EntityLogicalName, id, new ColumnSet(true), CancellationToken.None);
            Assert.AreEqual("Async Updated", account.ToEntity<Account>().Name);

            var accounts = await service.RetrieveMultipleAsync(new QueryExpression(Account.EntityLogicalName));
            Assert.AreEqual(1, accounts.Entities.Count);

            await service.DeleteAsync(Account.EntityLogicalName, id);
            accounts = await service.RetrieveMultipleAsync(new QueryExpression(Account.EntityLogicalName), CancellationToken.None);
            Assert.AreEqual(0, accounts.Entities.Count);
        }

        [TestMethod]
        public async Task LocalCrmTests_Async_ClientSideOrganizationServiceWrapsAsyncCalls()
        {
            using IClientSideOrganizationService service = new ClientSideOrganizationService(Service);
            var contact = await service.CreateAndReturnAsync(new Contact { FirstName = "Async" }, CancellationToken.None);
            Assert.AreNotEqual(default, contact.Id);

            var retrieved = await service.RetrieveAsync(Contact.EntityLogicalName, contact.Id, new ColumnSet(true));
            Assert.AreEqual("Async", retrieved.ToEntity<Contact>().FirstName);
        }
    }
}
#endif
