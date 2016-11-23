using System;
using System.Collections.Concurrent;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    /// <summary>
    /// Collection class for Assumed Enttities
    /// </summary>
    public class AssumedEntities
    {
        private ConcurrentDictionary<string, Entity> InternalStore { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssumedEntities"/> class.
        /// </summary>
        public AssumedEntities()
        {
            InternalStore = new ConcurrentDictionary<string, Entity>();
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
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public TEntity Get<TAttribute, TEntity>()
            where TAttribute : EntityDataAssumptionBaseAttribute
            where TEntity : Entity
        {
            return Get<TAttribute>().ToEntity<TEntity>();
        }

        /// <summary>
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public Entity Get<T>() where T : EntityDataAssumptionBaseAttribute
        {
            Entity entity;
            if (!InternalStore.TryGetValue(GetKey<T>(), out entity))
            {
                throw new Exception($"AssumedEntities does not contain an entity for Assumption {typeof(T).Name}.");
            }

            return entity;
        }

        /// <summary>
        /// Gets the Assumed Entity by Assumption Attribute Type, casting it to the Entity Type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="assumption">The assumption.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public TEntity Get<TAttribute, TEntity>(IAssumptionEntityType<TAttribute, TEntity> assumption) where TAttribute : EntityDataAssumptionBaseAttribute
                                                                                                      where TEntity : Entity
        {
            return Get<TAttribute>().ToEntity<TEntity>();
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
    }
}
