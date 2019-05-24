using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
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

            Entity = new Entity(logicalName){ Id = entityId };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Id"/> class.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="entityId">The entity identifier.</param>
        public Id(string logicalName, string entityId) : this(logicalName, new Guid(entityId)) { }

        #region Static Methods

        /// <summary>
        /// Enumerates all of the Ids in a struct
        /// </summary>
        /// <typeparam name="T">The Struct</typeparam>
        /// <returns></returns>
        public static IEnumerable<Id> GetIds<T>() where T : struct { return GetIdsInternal(typeof(T)); }

        private static IEnumerable<Id> GetIdsInternal(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var value = field.GetValue(null);
                if (value == null)
                {
                    continue;
                }

                if (value is Id id)
                {
                    yield return id;
                }
            }

            foreach(var nested in type.GetNestedTypes())
            {
                foreach (var id in GetIdsInternal(nested))
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Returns the Ids Struct from a type as a dynamic object.  This allows for a base class to use the Ids defined by the child class.
        /// </summary>
        /// <typeparam name="T">The Type to lookup the ids struct for.</typeparam>
        /// <returns></returns>
        public static dynamic GetIdsForType<T>()
        {
            return GetIdsForType(typeof(T));
        }

        /// <summary>
        /// Returns the Ids Struct from the given type as a dynamic object.
        /// </summary>
        /// <param name="type">The Type to lookup the ids struct for.</param>
        /// <returns></returns>
        public static dynamic GetIdsForType(Type type)
        {
            var ids = (ICollection<KeyValuePair<string, object>>)new ExpandoObject();
            foreach (var id in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                                        .SelectMany(GetIdsWithName))
            {
                ids.Add(id);
            }

            return ids;
        }

        private static IEnumerable<KeyValuePair<string, object>> GetIdsWithName(Type type)
        {
            var idType = typeof(Id);

            if (type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                // lambda enclosures will generate display classes.  This could be of type Id, but static, not instance, causing errors.
                yield break;
            }

            foreach (var field in type.GetFields().Where(field => idType.IsAssignableFrom(field.FieldType)))
            {
                yield return new KeyValuePair<string, object>(field.Name, GetValue(field));
            }

            foreach (var nestedType in type.GetNestedTypes())
            {
                var nested = new ExpandoObject();
                var ids = (ICollection<KeyValuePair<string, object>>)nested;
                foreach (var id in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                                       .SelectMany(GetIdsWithName))
                {
                    ids.Add(id);
                }
                yield return new KeyValuePair<string, object>(nestedType.Name, nested);
            }
        }

        [DebuggerHidden]
        private static Id GetValue(FieldInfo field)
        {
            try
            {
                return (Id)field.GetValue(null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException?.InnerException != null)
                {
                    Exception innerException = ex.InnerException.InnerException;
                    if (innerException.Message.Contains("\"Entity\" is not a valid entity name"))
                        throw innerException;
                }
                throw;
            }
        }

        #endregion Static Methods

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
                if (!(base.Entity is TEntity value))
                {
                    value = base.Entity.AsEntity<TEntity>();
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

    /// <summary>
    /// Extensions that help to distinguish between methods that accept both EntityReference and Entity
    /// </summary>
    public static class IdExtensions
    {
        /// <summary>
        /// Deletes the Specified Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="id"></param>
        public static void Delete<T>(this IOrganizationService service, Id<T> id) where T:Entity
        {
            service.Delete(id.LogicalName, id.EntityId);
        }
    }
}
