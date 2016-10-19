using System;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class IdTests
    {
        public void Id_ImplicitConversions_()
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
    }
}
