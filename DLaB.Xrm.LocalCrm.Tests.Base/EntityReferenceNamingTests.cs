using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class EntityReferenceNamingTests : BaseTestClass
    {
        [TestMethod]
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        public void LocalCrmTests_EntityReferenceNaming_NamesAreReturned()
        {
            var service = GetService();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Squeeze The Juice"
            };
            service.Create(account);
            var user = new SystemUser
            {
                Id = Guid.NewGuid(),
                FirstName = "Bart",
                LastName = "Simpson"
            }; 
            service.Create(user);
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                FirstName = "Johnny",
                LastName = "Appleseed",
                ParentCustomerId = account.ToEntityReference(),
                OwnerId = user.ToEntityReference()
            };
            service.Create(contact);

            var sut = service.GetEntity<Contact>(contact.Id);
            user = service.GetEntity<SystemUser>(user.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.FullName), "The full name should have been returned!");
            Assert.AreEqual(account.Name, sut.ParentCustomerId.Name, "Account name should have been returned!");
            Assert.AreEqual(user.FullName, sut.OwnerId.Name, "Owner Id name should have been returned!");
            Assert.IsTrue(string.IsNullOrWhiteSpace(sut.OwningUser.Name), "The Owner User Name should not have been returned!");

        }
    }
}
