using System;
using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class IdTests
    {
        [TestMethod]
        public void Id_ImplicitConversions()
        {
            var id = new Id<Contact>(Guid.NewGuid())
            {
                Entity =
                {
                    FirstName = "Id_ImplicitConversions",
                    LastName = "Should_BeAllowed"
                }
            };

            Guid guid = id;
            Assert.AreEqual(guid, id.EntityId, "Implicit Guid Conversion Failed");

            EntityReference entityReference = id;
            Assert.AreEqual(entityReference, id.EntityReference, "EntityReference Guid Conversion Failed");

            Entity entity = id;
            Assert.AreEqual(entity, ((Id) id).Entity, "Implicit Entity Conversion Failed");

            Contact contactEntity = id;
            Assert.AreEqual(contactEntity, id.Entity, "Implicit TEntity Conversion Failed");
            Assert.AreEqual(contactEntity.FirstName, id.Entity.FirstName, "Implicit TEntity Conversion Failed for First Name");
            Assert.AreEqual(contactEntity.LastName, id.Entity.LastName, "Implicit TEntity Conversion Failed for Last Name");
        }

        private struct Ids
        {
            public struct Accounts
            {
                public static readonly Id<Account> A = new Id<Account>("7A46EE9B-EB39-4A82-AFDA-3606911AC782");
                public static readonly Id<Account> B = new Id<Account>("D1CBEA3D-5A0F-4E09-95C4-EA71F70ABB61");
            }
            public static readonly Id<Contact> Contact  = new Id<Contact>("872DD7E2-637D-4D4D-91DB-B54DD17C2BA3");
        }

        [TestMethod]
        public void GetIds_Should_EnumerateStruct()
        {
            var ids = Id.GetIds<Ids>().ToList();
            Assert.IsTrue(ids.Contains(Ids.Accounts.A));
            Assert.IsTrue(ids.Contains(Ids.Accounts.B));
            Assert.IsTrue(ids.Contains(Ids.Contact));
        }

        [TestMethod]
        public void Id_Inject_Should_AddAttributes()
        {
            var id = Guid.NewGuid();
            Ids.Contact.Inject(new Contact
            {
                Id = id,
                Address1_AddressTypeCodeEnum = Contact_Address1_AddressTypeCode.BillTo
            });
            Assert.AreEqual(Contact_Address1_AddressTypeCode.BillTo, Ids.Contact.Entity.Address1_AddressTypeCodeEnum);
            Assert.AreNotEqual(id, Ids.Contact.EntityId);
        }
    }
}

