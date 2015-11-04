using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// In Sandbox Mode, You can't serialize and entity.  This Entity Type removes the dependencies that required a non-sandboxed plugin from serializing an Entity
    /// </summary>
    [DataContract(Name = "Entity", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableEntity : IExtensibleDataObject
    {
        [DataMember]
        public string LogicalName { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public SerializableAttributeCollection Attributes { get; set; }

        [DataMember]
        public EntityState? EntityState { get; set; }

        [DataMember]
        public SerializableFormattedValueCollection FormattedValues { get; set; }

        [DataMember]
        public SerializableRelatedEntityCollection RelatedEntities { get; set; }

        [DataMember]
        public string RowVersion { get; set; }

        [DataMember]
        public SerializableKeyAttributeCollection KeyAttributes { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public SerializableEntity()
        {
            Attributes = new SerializableAttributeCollection();
            FormattedValues = new SerializableFormattedValueCollection();
            RelatedEntities = new SerializableRelatedEntityCollection();
            KeyAttributes = new SerializableKeyAttributeCollection();
        }

        public SerializableEntity(Entity entity)
        {
            Attributes = new SerializableAttributeCollection(entity.Attributes);
            EntityState = entity.EntityState;
            ExtensionData = entity.ExtensionData;
            FormattedValues = new SerializableFormattedValueCollection(entity.FormattedValues);
            Id = entity.Id;
            KeyAttributes = new SerializableKeyAttributeCollection(entity.KeyAttributes);
            LogicalName = entity.LogicalName;
            RelatedEntities = new SerializableRelatedEntityCollection(entity.RelatedEntities);
            RowVersion = entity.RowVersion;
        }
    }
}