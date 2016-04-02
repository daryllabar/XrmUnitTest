using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Sandbox Serializable Entity Reference
    /// </summary>
    [DataContract(Name = "EntityReference", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableEntityReference : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the ID of the record.
        /// </summary>
        /// 
        /// <returns>
        /// Type: Returns_Guid
        /// The ID of the record.
        /// </returns>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        /// 
        /// <returns>
        /// Type: Returns_StringThe logical name of the entity.
        /// </returns>
        [DataMember]
        public string LogicalName { get; set; }

        /// <summary>
        /// Gets or sets the value of the primary attribute of the entity.
        /// </summary>
        /// 
        /// <returns>
        /// Type: Returns_StringThe value of the primary attribute of the entity.
        /// </returns>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key attributes.
        /// </summary>
        /// 
        /// <returns>
        /// Type: <see cref="T:Microsoft.Xrm.Sdk.KeyAttributeCollection"/>.
        /// </returns>
        [DataMember]
        public SerializableKeyAttributeCollection KeyAttributes { get; set; }

        /// <summary>
        /// Gets or sets the row version.
        /// </summary>
        /// 
        /// <returns>
        /// Type: Returns_String.
        /// </returns>
        [DataMember]
        public string RowVersion { get; set; }

        /// <summary>
        /// ExtensionData
        /// </summary>
        /// 
        /// <returns>
        /// Type: Returns_ExtensionDataObjectThe extension data.
        /// </returns>
        public ExtensionDataObject ExtensionData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableEntityReference"/> class.
        /// </summary>
        public SerializableEntityReference() { KeyAttributes = new SerializableKeyAttributeCollection(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableEntityReference"/> class.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        public SerializableEntityReference(EntityReference entityReference)
        {
            Id = entityReference.Id;
            LogicalName = entityReference.LogicalName;
            Name = entityReference.Name;
            KeyAttributes = new SerializableKeyAttributeCollection(entityReference.KeyAttributes);
            RowVersion = entityReference.RowVersion;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="SerializableEntityReference"/> to <see cref="EntityReference"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator EntityReference(SerializableEntityReference entity)
        {
            if (entity == null)
            {
                return null;
            }
            var xrmEntity = new EntityReference
            {
                ExtensionData = entity.ExtensionData,
                KeyAttributes = (KeyAttributeCollection)entity.KeyAttributes,
                Id = entity.Id,
                LogicalName = entity.LogicalName,
                Name = entity.Name,
                RowVersion = entity.RowVersion
            };

            return xrmEntity;
        }
    }
}
