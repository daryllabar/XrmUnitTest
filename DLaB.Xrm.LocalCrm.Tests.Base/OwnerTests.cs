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
            var service = GetService();
            var user = service.GetEntity<SystemUser>(service.Create(new SystemUser()));
            Assert.IsNotNull(user.BusinessUnitId);
            var contact = service.GetEntity<Contact>(service.Create(new Contact{OwnerId = user.ToEntityReference()}));
            Assert.AreEqual(user.Id, contact.OwnerId.Id);
            Assert.AreEqual(user.Id, contact.OwningUser.Id);
            Assert.IsNotNull(contact.OwningBusinessUnit);
            Assert.IsNull(contact.OwningTeam);
            var team = service.GetEntity<Team>(service.Create(new Team()));
            Assert.IsNotNull(team.BusinessUnitId);
            service.Update(new Contact
            {
                Id = contact.Id,
                OwnerId = team.ToEntityReference() 
            });
            contact = service.GetEntity<Contact>(contact.Id);
            Assert.AreEqual(team.Id, contact.OwnerId.Id);
            Assert.AreEqual(team.Id, contact.OwningTeam.Id);
            Assert.IsNotNull(contact.OwningBusinessUnit);
            Assert.IsNull(contact.OwningUser);
        }
    }
}
