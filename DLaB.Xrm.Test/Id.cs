using System;
using System.Activities;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    [DebuggerDisplay("{LogicalName} {EntityId})")]
    public class Id
    {
        public Guid EntityId { get; private set; }
        public String LogicalName { get; private set; }
        public EntityReference EntityReference { get; private set; }

        public Id(string logicalName, Guid entityId)
        {
            if(logicalName == "entity")
            {
                throw new Exception("\"Entity\" is not a valid entityname.  Ids must be be of a valid Early Bound Type, ie Contact, Opportunity, etc... ! " + entityId);
            }

            LogicalName = logicalName;
            EntityId = entityId;
            EntityReference = new EntityReference(logicalName, entityId);
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
        public Id(Guid entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }

        public Id(string entityId) : base(EntityHelper.GetEntityLogicalName<TEntity>(), entityId) { }
    }
}
