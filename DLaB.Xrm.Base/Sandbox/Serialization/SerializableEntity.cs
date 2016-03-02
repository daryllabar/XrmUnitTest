using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// In Sandbox Mode, You can't serialize and entity.  This Entity Type removes the dependencies that required a non-sandboxed plugin from serializing an Entity
    /// </summary>
    [DataContract(Name = "Entity", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableEntity : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the name of the logical.
        /// </summary>
        /// <value>
        /// The name of the logical.
        /// </value>
        [DataMember]
        public string LogicalName { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [DataMember]
        public SerializableAttributeCollection Attributes { get; set; }

        /// <summary>
        /// Gets or sets the state of the entity.
        /// </summary>
        /// <value>
        /// The state of the entity.
        /// </value>
        [DataMember]
        public EntityState? EntityState { get; set; }

        /// <summary>
        /// Gets or sets the formatted values.
        /// </summary>
        /// <value>
        /// The formatted values.
        /// </value>
        [DataMember]
        public SerializableFormattedValueCollection FormattedValues { get; set; }

        /// <summary>
        /// Gets or sets the related entities.
        /// </summary>
        /// <value>
        /// The related entities.
        /// </value>
        [DataMember]
        public SerializableRelatedEntityCollection RelatedEntities { get; set; }

        /// <summary>
        /// Gets or sets the row version.
        /// </summary>
        /// <value>
        /// The row version.
        /// </value>
        [DataMember]
        public string RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the key attributes.
        /// </summary>
        /// <value>
        /// The key attributes.
        /// </value>
        [DataMember]
        public SerializableKeyAttributeCollection KeyAttributes { get; set; }

        /// <summary>
        /// Gets or sets the extension data.
        /// </summary>
        /// <value>
        /// The extension data.
        /// </value>
        public ExtensionDataObject ExtensionData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableEntity"/> class.
        /// </summary>
        public SerializableEntity()
        {
            Attributes = new SerializableAttributeCollection();
            FormattedValues = new SerializableFormattedValueCollection();
            RelatedEntities = new SerializableRelatedEntityCollection();
            KeyAttributes = new SerializableKeyAttributeCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableEntity"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
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

        /// <summary>
        /// Performs an explicit conversion from <see cref="SerializableEntity"/> to <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Entity(SerializableEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            var xrmEntity = new Entity
            {
                LogicalName = entity.LogicalName,
                Id = entity.Id,
                Attributes = (AttributeCollection) entity.Attributes,
                KeyAttributes = (KeyAttributeCollection) entity.KeyAttributes,
                RowVersion = entity.RowVersion,
                EntityState = entity.EntityState,
                ExtensionData = entity.ExtensionData,
            };

            xrmEntity.FormattedValues.AddRange(entity.FormattedValues.Select(v => (KeyValuePair<string,string>) v));
            xrmEntity.RelatedEntities.AddRange(entity.RelatedEntities.Select(v => (KeyValuePair<Relationship, EntityCollection>)v));

            return xrmEntity;
        }
    }
} 