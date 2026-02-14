using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DLaB.Xrm.LocalCrm
{
    internal class EntityProperties
    {
        public Dictionary<string, PropertyInfo> PropertiesByName { get; private set; } = null!;
        public Dictionary<string, List<PropertyInfo>> PropertiesByLogicalName { get; private set; } = null!;
        public string EntityName { get; private set; } = string.Empty;

        public bool IsActivityType => PropertiesByName.ContainsKey("ActivityId");
        
        private EntityProperties()
        {
        }

        public bool ContainsProperty(string name)
        {
            return PropertiesByName.ContainsKey(name) 
                || PropertiesByLogicalName.ContainsKey(name);
        }

        public PropertyInfo GetProperty(string name)
        {
            if (PropertiesByName.TryGetValue(name, out var property))
            {
                return property;
            }
            if (PropertiesByLogicalName.TryGetValue(name, out var properties))
            {
                // If there are multiple properties with the same logical name, prefer the one that isn't an OptionSetValue, since there is typically a duplicate property that is an enum
                return properties.FirstOrDefault(p => p.PropertyType != typeof(OptionSetValue)) ?? properties.First();
            }
            throw new KeyNotFoundException($"The property \"{name}\" was not found in the entity type \"{EntityName}\".");
        }

        public static EntityProperties Get<T>() where T: Entity
        {
            return Get(typeof(T));
        }

        private const string NullKey = "ATTRIBUTE LOGICAL NAME MISSING";
        public static EntityProperties Get(Type type) 
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name);
            
            var entity = new EntityProperties
            {
                EntityName = EntityHelper.GetEntityLogicalName(type),
                PropertiesByName = properties,
                PropertiesByLogicalName = properties.Values
                                                    .Select(p => new { Key = p.GetAttributeLogicalName(false) ?? NullKey, Property = p })
                                                    .Where(p => p.Key != NullKey)
                                                    .GroupBy(k => k.Key, p => p.Property)
                                                    .ToDictionary(k => k.Key, p => p.ToList()),
            };

            return entity;
        }
    }
}
