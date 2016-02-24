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

        public SerializableEntityReference() { KeyAttributes = new SerializableKeyAttributeCollection(); }

        public SerializableEntityReference(EntityReference entityReference)
        {
            Id = entityReference.Id;
            LogicalName = entityReference.LogicalName;
            Name = entityReference.Name;
            KeyAttributes = new SerializableKeyAttributeCollection(entityReference.KeyAttributes);
            RowVersion = entityReference.RowVersion;
        }
    }
}
