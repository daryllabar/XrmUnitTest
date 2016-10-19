using System;
using System.Activities;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// A Helper Class for Easily converting to and from EntityReferences, Logical Names, Ids, and Entities
    /// </summary>
    [DebuggerDisplay("{LogicalName} {EntityId})")]
    public class Id
    {
        /// <summary>
        /// Gets the Entity.Id.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public Guid EntityId => Entity.Id;

        /// <summary>
        /// Gets the logicalname of the entity.
        /// </summary>
        /// <value>
        /// The logicalname of the entity.
        /// </value>
        public string LogicalName => Entity.LogicalName;

        /// <summary>
        /// Gets the entity reference.
        /// </summary>
        /// <value>
        /// The entity reference.
        /// </value>
        public EntityReference EntityReference => Entity.ToEntityReference();

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Entity Entity { get; set; }
        /// <summary>
        /// Provides an index for Entity.Attribute values
        /// </summary>
        /// <value>
        /// </value>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        public object this[string attributeName] { 
            get { return Entity.Attributes[attributeName]; } 
            set { Entity.Attributes[attributeName] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Id"/> class.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <exception cref="System.Exception">\Entity\ is not a valid entityname.  Ids must be be of a valid Early Bound Type, ie Contact, Opportunity, etc... !  + entityId</exception>
        public Id(string logicalName, Guid entityId)
        {
            if(logicalName == "entity")
            {
                throw new Exception("\"Entity\" is not a valid entityname.  Ids must be be of a valid Early Bound Type, ie Contact, Opportunity, etc... ! " + entityId);
            }

            Entity = new Entity(logicalName, entityId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Id"/> class.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="entityId">The entity identifier.</param>
        public Id(string logicalName, string entityId) : this(logicalName, new Guid(entityId)) { }

        #region Implicit Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="Id"/> to <see cref="EntityReference"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator EntityReference(Id id)
        {
            return id.EntityReference;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Id"/> to <see cref="Guid"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Guid(Id id)
        {
            return id.EntityId;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Id"/> to <see cref="InArgument{EntityReference}"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator InArgument<EntityReference>(Id id)
        {
            return new InArgument<EntityReference>(id);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Id"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(Id id)
        {
            return id.LogicalName;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Id" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Id(Entity entity)
        {
            var id = new Id(entity.LogicalName, entity.Id) {Entity = entity};
            return id;
        }


        #endregion Implicit Operators

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{LogicalName} {EntityId}";
        }
    }

    /// <summary>
    /// A Typed version of the Id class.  Allows for easily converting to and from EntityReferences, Logical Names, Ids, and Entities
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Id<TEntity> : Id
        where TEntity : Entity
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public new TEntity Entity
        {
            get { 
                var value = base.Entity as TEntity;
                if (value == null)
                {
                    value = base.Entity.ToEntity<TEntity>();
                    base.Entity = value;
                }
                return value;
            }
            set
            {
                base.Entity = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Id{TEntity}"/> class.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        public Id(Guid entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Id{TEntity}"/> class.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        public Id(string entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:DLaB.Xrm.Test.Id" /> to <see cref="T:System.String" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator TEntity(Id<TEntity> entity)
        {
            return entity?.Entity;
        }
    }
}
