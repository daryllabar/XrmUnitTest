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

        /// <summary>
        /// Determines if the entity is an N:N relationship (intersect) entity.  These entities only contain their own id, and the id of the two related entities, and can not be Created/Updated/Deleted.
        /// </summary>
        public bool IsManyToManyIntersect { get; private set; }

        /// <summary>
        /// The logical names of the two id attributes of the related entities of an N:N relationship (intersect) entity.  Empty if the entity is not an intersect entity.
        /// </summary>
        public string[] ManyToManyIntersectIdAttributes { get; private set; } = new string[0];
        
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
            entity.IsManyToManyIntersect = IsManyToManyIntersectType(properties, out var intersectIdAttributes);
            entity.ManyToManyIntersectIdAttributes = intersectIdAttributes;

            return entity;
        }

        /// <summary>
        /// An N:N relationship (intersect) entity contains three Nullable Guid attributes (its own id, and the ids of the two related entities),
        /// no state code, and no lookup or option set attributes.
        /// </summary>
        private static bool IsManyToManyIntersectType(Dictionary<string, PropertyInfo> properties, out string[] intersectIdAttributes)
        {
            intersectIdAttributes = new string[0];
            if (properties.ContainsKey("StateCode"))
            {
                return false;
            }

            var ids = new List<string>();
            foreach (var property in properties.Values.Where(p => p.GetAttributeLogicalName(false) != null))
            {
                if (property.PropertyType == typeof(EntityReference)
                    || property.PropertyType == typeof(OptionSetValue))
                {
                    return false;
                }

                if (property.PropertyType == typeof(Guid?))
                {
                    ids.Add(property.GetAttributeLogicalName(false)!);
                }
            }

            if (ids.Count != 3)
            {
                return false;
            }

            var primaryIdAttribute = properties.TryGetValue("Id", out var idProperty)
                ? idProperty.GetAttributeLogicalName(false)
                : null;
            intersectIdAttributes = ids.Where(id => id != primaryIdAttribute).ToArray();
            return true;
        }
    }
}
