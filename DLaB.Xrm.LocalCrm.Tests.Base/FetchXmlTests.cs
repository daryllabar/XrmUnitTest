using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using XrmUnitTest.Test;

#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class FetchXmlTests : LocalTestBase
    {
        [TestMethod]
        public void LocalCrmTests_NotLike()
        {
            var id = Service.Create(new Account { Name = "Hello" });
            var fetchXml = @"<fetch>
              <entity name='account' >
                <attribute name='name' />
                <filter>
                  <condition attribute='name' operator='not-like' value='SomeValue%' />
                </filter>
              </entity>
            </fetch>";
            var account = Service.RetrieveMultiple(new FetchExpression(fetchXml)).ToEntityList<Account>().First();
            Assert.AreEqual(id, account.Id);
        }

        [TestMethod]
        public void LocalCrmTests_NotLikeInnerJoin()
        {
            var fetchXml = @"<fetch>
              <entity name='contact' >
                <attribute name='fullname' />
                 <link-entity name='account' to='parentcustomerid' from='accountid' alias='S' link-type='inner'>
                   <attribute name='name' />
                   <filter>
                     <condition attribute='name' operator='not-like' value='SomeValue%' />
                   </filter>
                 </link-entity>
              </entity>
            </fetch>";

            var contact = new Id<Contact>("A3759F9B-CFA9-49A2-A8C1-1B7B16A932EF");
            var account = new Id<Account>("4FB15F72-7E32-4116-8B71-6A28A170DE51");
            contact.Entity.ParentCustomerId = account;

            EnvBuilder.WithEntities(contact, account).Create(Service);
            var result = Service.RetrieveMultiple(new FetchExpression(fetchXml)).ToEntityList<Contact>().First();
            Assert.AreEqual(contact.EntityId, result.Id);
        }

        [TestMethod]
        public void LocalCrmTests_Top1()
        {
            var fetchXml = @"<fetch top='1'>
              <entity name='account' >
                <attribute name='name' />
                <filter>
                    <condition attribute='name' operator='like' value='SomeValue%' />
                </filter>
              </entity>
            </fetch>";

            var account1 = new Id<Account>("A3759F9B-CFA9-49A2-A8C1-1B7B16A932EF");
            var account2 = new Id<Account>("4FB15F72-7E32-4116-8B71-6A28A170DE51");
            account1.Entity.Name = "SomeValue1";
            account2.Entity.Name = "SomeValue2";

            EnvBuilder.WithEntities(account1, account2).Create(Service);
            var result = Service.RetrieveMultiple(new FetchExpression(fetchXml)).ToEntityList<Account>();
            Assert.AreEqual(1, result.Count);
        }
    }
}
