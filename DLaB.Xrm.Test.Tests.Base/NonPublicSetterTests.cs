#nullable enable
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
#if NET
using DataverseUnitTest;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class NonPublicSetterTests
    {
        [TestMethod]
        public void AttributeMetadata_SetsValue()
        {
            // Arrange
            var myLogicalName = nameof(AttributeMetadata_SetsValue);

            // Act
            var data = new AttributeMetadata
            {
                LogicalName = "test_attribute",
            }.WithPrivate().Set(d => d.EntityLogicalName, myLogicalName);
            AttributeMetadata implicitlyTyped = new AttributeMetadata
            {
                LogicalName = "test_attribute",
            }.WithPrivate().Set(d => d.EntityLogicalName, myLogicalName);

            // Assert
            Assert.AreEqual(myLogicalName, data.Value.EntityLogicalName);
            Assert.AreEqual(myLogicalName, implicitlyTyped.EntityLogicalName);
        }

        [TestMethod]
        public void RequestParameter_SetsValue()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var data = new RetrieveEntityResponse().WithPrivate().Set(d => d.EntityMetadata, new EntityMetadata { MetadataId = id});
            RetrieveEntityResponse implicitlyTyped = new RetrieveEntityResponse().WithPrivate().Set(d => d.EntityMetadata, new EntityMetadata { MetadataId = id });

            // Assert
            Assert.AreEqual(id, data.Value.EntityMetadata.MetadataId);
            Assert.AreEqual(id, implicitlyTyped.EntityMetadata.MetadataId);
        }
    }
}
