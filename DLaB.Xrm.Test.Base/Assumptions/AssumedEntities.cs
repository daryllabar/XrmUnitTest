using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    /// <summary>
    /// Collection class for Assumed Entities
    /// </summary>
    public class AssumedEntities
    {
        private ConcurrentDictionary<string, Entity> InternalStore { get; }
        private ConcurrentDictionary<Guid, Guid> CreatedAsEntityReferences { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssumedEntities"/> class.
        /// </summary>
        public AssumedEntities()
        {
            InternalStore = new ConcurrentDictionary<string, Entity>();
            CreatedAsEntityReferences = new ConcurrentDictionary<Guid, Guid>();
        }

        private static string GetKey<T>() where T : EntityDataAssumptionBaseAttribute
        {
            return typeof(T).AssemblyQualifiedName;
        }

        private static string GetKey(EntityDataAssumptionBaseAttribute assumption)
        {
            return assumption.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Validates the assumption attributes of the given type(s)
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="types">The type(s) that has/have EntityDataAssumptionBaseAttribute Attributes</param>
        public static AssumedEntities Load(IOrganizationService service, params Type[] types)
        {

            var assumedEntities = new AssumedEntities();
            foreach (var type in types)
            {
                foreach (var entityAssumption in type.GetCustomAttributes(true)
                                                     .Select(a => a as EntityDataAssumptionBaseAttribute)
                                                     .Where(a => a != null))
                {
                    assumedEntities.Load(service, entityAssumption);
                }
            }

            return assumedEntities;
        }

        /// <summary>
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="TAssumption">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public TEntity Get<TAssumption, TEntity>()
            where TAssumption : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<TAssumption, TEntity>
            where TEntity : Entity
        {
            return Get<TAssumption>().AsEntity<TEntity>();
        }

        /// <summary>
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="TAssumption"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public Entity Get<TAssumption>() where TAssumption : EntityDataAssumptionBaseAttribute
        {
            if (!InternalStore.TryGetValue(GetKey<TAssumption>(), out Entity entity))
            {
                throw new Exception($"AssumedEntities does not contain an entity for Assumption {typeof(TAssumption).Name}.");
            }

            return entity;
        }

        /// <summary>
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="TAssumption">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="assumption">The assumption.</param>
        /// <returns></returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public TEntity Get<TAssumption, TEntity>(IAssumptionEntityType<TAssumption, TEntity> assumption)
#pragma warning restore IDE0060 // Remove unused parameter
            where TAssumption : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<TAssumption, TEntity>
            where TEntity : Entity
        {
            return Get<TAssumption>().AsEntity<TEntity>();
        }

        /// <summary>
        /// Determines whether the assumption type is contained.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Contains<T>() where T : EntityDataAssumptionBaseAttribute
        {
            return InternalStore.ContainsKey(GetKey<T>());
        }

        /// <summary>
        /// Determines whether the assumption type is contained.
        /// </summary>
        /// <param name="assumption">The assumption.</param>
        /// <returns></returns>
        public bool Contains(EntityDataAssumptionBaseAttribute assumption)
        {
            return InternalStore.ContainsKey(GetKey(assumption));
        }

        /// <summary>
        /// Adds the entity for the Assumption Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool Add<T>(Entity entity) where T : EntityDataAssumptionBaseAttribute
        {
            return Add(GetKey<T>(), entity);
        }

        internal bool AlreadyCreatedAsEntityReference(Guid id)
        {
            return CreatedAsEntityReferences.ContainsKey(id);
        }

        internal void AddCreatedEntityReference(Guid id)
        {
            CreatedAsEntityReferences.TryAdd(id, id);
        }

        /// <summary>
        /// Adds the specified assumption Type and Entity.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool Add(EntityDataAssumptionBaseAttribute attribute, Entity entity)
        {
            return Add(GetKey(attribute), entity);
        }

        private bool Add(string key, Entity entity)
        {
            return InternalStore.TryAdd(key, entity);
        }

        /// <summary>
        /// Validates the given assumption
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityAssumptions">EntityDataAssumptionBase objects</param>
        public void Load(IOrganizationService service, params EntityDataAssumptionBaseAttribute[] entityAssumptions)
        {
            foreach (var entityAssumption in entityAssumptions)
            {
                try
                {
                    entityAssumption.AddAssumedEntities(service, this);
                }
                catch (Exception ex)
                {
                    throw new Exception($"There was an exception attempting to load assumption of type {entityAssumption.GetType().FullName}" + Environment.NewLine +
                        $"--{Environment.NewLine}Current Assumed Entities Loaded:{Environment.NewLine}" +
                            string.Join("," + Environment.NewLine, InternalStore.Keys) + Environment.NewLine + "--" + Environment.NewLine, ex);

                }
            }
        }

        #region GetAll

        /// <summary>
        /// Gets all Assumed Entities of the given entity type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetAll<T>() where T : Entity
        {
            var logicalName = EntityHelper.GetEntityLogicalName<T>();
            return InternalStore.Values.Where(e => e.LogicalName == logicalName).Select(e => e.ToEntity<T>()).ToList();
        }

        /// <summary>
        /// Gets all Assumed Entities of the given entity type.
        /// </summary>
        /// <returns></returns>
        public List<Entity> GetAll(string logicalName)
        {
            return InternalStore.Values.Where(e => e.LogicalName == logicalName).ToList();
        }


        #endregion GetAll

        #region GetId

        /// <summary>
        /// Gets the Assumed Entity as a typed Id
        /// </summary>
        /// <typeparam name="TAssumption"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public Id<TEntity> GetId<TAssumption, TEntity>()
            where TAssumption : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<TAssumption, TEntity>
            where TEntity : Entity
        {
            var entity = Get<TAssumption, TEntity>();
            return new Id<TEntity>(entity.Id)
            {
                Entity = entity
            };
        }

        /// <summary>
        /// Gets the Assumed Entity as a typed Id
        /// </summary>
        /// <typeparam name="TAssumption">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="assumption">The assumption.</param>
        /// <returns></returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public Id<TEntity> GetId<TAssumption, TEntity>(IAssumptionEntityType<TAssumption, TEntity> assumption)
#pragma warning restore IDE0060 // Remove unused parameter
            where TAssumption : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<TAssumption, TEntity>
            where TEntity : Entity
        {
            return GetId<TAssumption, TEntity>();
        }


        /// <summary>
        /// Gets the Assumed Entity as an untyped Id.
        /// </summary>
        /// <typeparam name="TAssumption">The type of the attribute.</typeparam>
        /// <returns></returns>
        public Id GetId<TAssumption>()
            where TAssumption : EntityDataAssumptionBaseAttribute
        {
            var entity = Get<TAssumption>();
            return new Id(entity.LogicalName, entity.Id)
            {
                Entity = entity
            };
        }



        #endregion GetId
    }
}
