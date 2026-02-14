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
        public Dictionary<string, List<PropertyInfo>> PropertiesByLowerCaseName { get; private set; } = null!;
        public Dictionary<string, PropertyInfo> PropertiesByLogicalName { get; private set; } = null!;
        public Dictionary<string, List<PropertyInfo>> AllPropertiesByLogicalName { get; private set; } = null!;
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
            if (PropertiesByName.TryGetValue(name, out var property) ||
                PropertiesByLogicalName.TryGetValue(name, out property))
            {
                return property;
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
            var propertiesWithLogicalName = properties.Values
                                                     .Select(p => new { Key = p.GetAttributeLogicalName(false) ?? NullKey, Property = p })
                                                     .Where(p => p.Key != NullKey)
                                                     .ToList();
            
            var entity = new EntityProperties
            {
                EntityName = EntityHelper.GetEntityLogicalName(type),
                PropertiesByName = properties,
                PropertiesByLogicalName = propertiesWithLogicalName
                                                     .GroupBy(k => k.Key, p => p.Property)
                                                     .Select(g => new { g.Key, Property = g.First()})
                                                     .ToDictionary(k => k.Key, p => p.Property),
                AllPropertiesByLogicalName = propertiesWithLogicalName
                                                     .GroupBy(k => k.Key, p => p.Property)
                                                     .ToDictionary(k => k.Key, p => p.ToList()),
                PropertiesByLowerCaseName = properties.GroupBy(v => v.Key.ToLower(), v => v.Value).ToDictionary(v => v.Key, v => v.ToList())
            };

            return entity;
        }
    }
}
