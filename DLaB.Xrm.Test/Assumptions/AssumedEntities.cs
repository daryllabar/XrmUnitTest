using System;
using System.Collections.Concurrent;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    public class AssumedEntities
    {
        private ConcurrentDictionary<string, Entity> InternalStore { get; set; }

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

        public TEntity Get<TAttribute, TEntity>()
            where TAttribute : EntityDataAssumptionBaseAttribute
            where TEntity : Entity
        {
            return Get<TAttribute>().ToEntity<TEntity>();
        }

        public Entity Get<T>() where T : EntityDataAssumptionBaseAttribute
        {
            Entity entity;
            if (!InternalStore.TryGetValue(GetKey<T>(), out entity))
            {
                throw new Exception(String.Format("Assumption {0} made an ass out of you and me.  AssumedEntities does not contain an entity for Assumption {0}.",
                    typeof(T).Name));
            }

            return entity;
        }

        public bool Contains<T>() where T : EntityDataAssumptionBaseAttribute
        {
            return InternalStore.ContainsKey(GetKey<T>());
        }

        public bool Contains(EntityDataAssumptionBaseAttribute assumption)
        {
            return InternalStore.ContainsKey(GetKey(assumption));
        }

        public bool Add<T>(Entity entity) where T : EntityDataAssumptionBaseAttribute
        {
            return Add(GetKey<T>(), entity);
        }

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
