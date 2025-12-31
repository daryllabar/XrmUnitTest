using System;
using System.Collections.Concurrent;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal class EntityPropertiesCache
    {
        private readonly ConcurrentDictionary<string, EntityProperties> _dictionary = new ConcurrentDictionary<string, EntityProperties>();

        public static EntityPropertiesCache Instance { get; } = new EntityPropertiesCache();

        private EntityPropertiesCache()
        {
            
        }

        public EntityProperties For<T>() where T: Entity
        {
            var type = typeof(T);
            return For(type);
        }

        public EntityProperties For(LocalCrmDatabaseInfo info, string logicalName)
        {
            return For(LocalCrmDatabase.GetType(info, logicalName));
        }

        public EntityProperties For(Type type)
        {
            var typeName = type.AssemblyQualifiedName;
            if (typeName == null)
            {
                throw new NullReferenceException("Assembly Qualified Name for Type was null!");
            }
            if (_dictionary.TryGetValue(typeName, out var properties))
            {
                return properties;
            }

            properties = EntityProperties.Get(type);

            if (_dictionary.TryAdd(typeName, properties))
            {
                return properties;
            }

            if (_dictionary.TryGetValue(typeName, out properties))
            {
                return properties;
            }

            throw new Exception($"Could Not Add Type \"{typeName}\" to {GetType().Name}");
        }
    }
}
