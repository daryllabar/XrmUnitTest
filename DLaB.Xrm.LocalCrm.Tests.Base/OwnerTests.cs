using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class OwnerTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Owner_SystemUserBusinessUnitSet()
        {
            var user = Service.GetEntity<SystemUser>(Service.Create(new SystemUser()));
            Assert.IsNotNull(user.BusinessUnitId);
            var contact = Service.GetEntity<Contact>(Service.Create(new Contact{OwnerId = user.ToEntityReference()}));
            Assert.AreEqual(user.Id, contact.OwnerId.Id);
            Assert.AreEqual(user.Id, contact.OwningUser.Id);
            Assert.IsNotNull(contact.OwningBusinessUnit);
            Assert.IsNull(contact.OwningTeam);
            var team = Service.GetEntity<Team>(Service.Create(new Team()));
            Assert.IsNotNull(team.BusinessUnitId);
            Service.Update(new Contact
            {
                Id = contact.Id,
                OwnerId = team.ToEntityReference() 
            });
            contact = Service.GetEntity<Contact>(contact.Id);
            Assert.AreEqual(team.Id, contact.OwnerId.Id);
            Assert.AreEqual(team.Id, contact.OwningTeam.Id);
            Assert.IsNotNull(contact.OwningBusinessUnit);
            Assert.IsNull(contact.OwningUser);
        }
    }
}
