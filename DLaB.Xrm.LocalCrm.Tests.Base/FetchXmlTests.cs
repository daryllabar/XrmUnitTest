using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class FetchXmlTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_NotLike()
        {
            var service = GetService();
            var id = service.Create(new Account());
            var fetchXml = @"<fetch>
              <entity name='account' >
                <attribute name='name' />
                <filter>
                  <condition attribute='name' operator='not-like' value='SomeValue%' />
                </filter>
              </entity>
            </fetch>";
            var account = service.RetrieveMultiple(new FetchExpression(fetchXml)).ToEntityList<Account>().First();
            Assert.AreEqual(id, account.Id);
        }
    }
}
