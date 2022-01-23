using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class ConnectionEntityTests: BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_ConnectionEntity_DuplicateConnectionCreated()
        {
            var service = GetService();
            var id1 = service.Create(new ConnectionRole());
            var id2 = service.Create(new ConnectionRole());
            var contactId1 = service.Create(new Contact());
            var contactId2 = service.Create(new Contact());
            service.Associate(new EntityReference(ConnectionRole.EntityLogicalName, id1), "connectionroleassociation_association", new EntityReference(ConnectionRole.EntityLogicalName, id2));

            var connection = new Connection
            {
                Record1Id = new EntityReference(Contact.EntityLogicalName, contactId1),
                Record2Id = new EntityReference(Contact.EntityLogicalName, contactId2),
                Record1RoleId = new EntityReference(ConnectionRole.EntityLogicalName, id1),
                Record2RoleId = new EntityReference(ConnectionRole.EntityLogicalName, id2),
            };
            service.Create(connection);
            var definedRecord = service.GetFirst<Connection>(Connection.Fields.Record1Id, contactId1);
            Assert.That.AttributesAreEqual(connection, definedRecord);
            var dupRecord = service.GetFirst<Connection>(Connection.Fields.Record1Id, contactId2);
            var connection2 = connection.Clone(true);
            connection2.Attributes.Remove(Connection.Fields.Id);
            connection2.Record1Id = connection.Record2Id;
            connection2.Record1RoleId = connection.Record2RoleId;
            connection2.Record2Id = connection.Record1Id;
            connection2.Record2RoleId = connection.Record1RoleId;
            Assert.That.AttributesAreEqual(connection2, dupRecord);

        }
    }
}
