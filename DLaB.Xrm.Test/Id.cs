using System;
using System.Activities;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    [DebuggerDisplay("{LogicalName} {EntityId})")]
    public class Id
    {
        public Guid EntityId { get { return Entity.Id; } }
        public String LogicalName { get { return Entity.LogicalName; } }
        public EntityReference EntityReference { get { return Entity.ToEntityReference(); } }
        public Entity Entity { get; set; }
        /// <summary>
        /// Provides an index for Entity.Attribute values
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        public object this[string attributeName] { 
            get { return Entity.Attributes[attributeName]; } 
            set { Entity.Attributes[attributeName] = value; }
        }

        public Id(string logicalName, Guid entityId)
        {
            if(logicalName == "entity")
            {
                throw new Exception("\"Entity\" is not a valid entityname.  Ids must be be of a valid Early Bound Type, ie Contact, Opportunity, etc... ! " + entityId);
            }

            Entity = new Entity(logicalName, entityId);
        }

        public Id(string logicalName, string entityId) : this(logicalName, new Guid(entityId)) { }

        #region Implicit Operators

        public static implicit operator EntityReference(Id id)
        {
            return id.EntityReference;
        }

        public static implicit operator Guid(Id id)
        {
            return id.EntityId;
        }

        public static implicit operator InArgument<EntityReference>(Id id)
        {
            return new InArgument<EntityReference>(id);
        }

        public static implicit operator String(Id id)
        {
            return id.LogicalName;
        }


        #endregion Implicit Operators

        public override string ToString()
        {
            return String.Format("{0} {1}", LogicalName, EntityId);
        }
    }

    public class Id<TEntity> : Id
        where TEntity : Entity
    {
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
        public Id(Guid entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }

        public Id(string entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }
    }
}
