using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Manages which entities are dependent on which other entities.  This is then used to determine the order in which the entities must be deleted
    /// NOTE: It utilizes the properties of the earlybound entity classes, so if the entity is not an earlybound type, it probably won't work.
    /// </summary>
    public class EntityDependency
    {
        #region Singleton Logic

        private static readonly Lazy<EntityDependency> Singleton = new Lazy<EntityDependency>(() => new EntityDependency());

        /// <summary>
        /// Gets Singleton Mapper.  This exists as a singleton if multiple unit tests are run at one time, the Entity Dependency Map structure can be resused from previous tests
        /// </summary>
        public static EntityDependency Mapper { get { return Singleton.Value; } }

        #endregion // Singleton Logic

        private LinkedList<EntityDependencyInfo> Types { get; set; }
        private Dictionary<String, EntityDependencyInfo> Infos { get; set; }

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be created without causing an error caused by a relationship constraint i.e. Create entity without linked entities existing
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<String> EntityCreationOrder { get { return Types.Select(e => e.LogicalName); } }

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be deleted without causing an error caused by a relationship constraint i.e. Delete Contact before Opprotunity.
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<String> EntityDeletionOrder { get { return EntityCreationOrder.Reverse(); } }

        private EntityDependency()
        {
            Types = new LinkedList<EntityDependencyInfo>();
            Infos = new Dictionary<string, EntityDependencyInfo>();
        }

        /// <summary>
        /// Adds the specified logical name to the collection of entities that are mapped.  Entities are cached so no work is performed when an entity type is added a second time
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        public void Add(string logicalName)
        {
            if (Infos.ContainsKey(logicalName))
            {
                // Already added Id type, nothing to do.
                return;
            }

            var info = new EntityDependencyInfo(logicalName);
            Infos.Add(logicalName, info);

            if (Types.Count == 0 || Types.First.Value.Dependencies.Contains(logicalName) || info.Dependents.Contains(Types.First.Value.LogicalName))
            {
                // No values in the list or the first value in the list contains id as a dependency, or id contains the first value as a dependent
                info.Node = Types.AddFirst(info);
                return;
            }

            var dependent = Types.Skip(1).FirstOrDefault(existingInfo => existingInfo.Dependencies.Contains(logicalName) || info.Dependents.Contains(existingInfo.LogicalName));
            info.Node = dependent == null ? Types.AddLast(info) : Types.AddBefore(dependent.Node, info);
        }

        private class EntityDependencyInfo
        {
            public HashSet<String> Dependencies { get; private set; }
            public HashSet<String> Dependents { get; private set; }
            public LinkedListNode<EntityDependencyInfo> Node { get; set; }
            public String LogicalName { get; private set; }

            private static readonly HashSet<String> PropertiesToIgnore = new HashSet<string>{
            "createdby",
            "createdonbehalfby",
            "modifiedby",
            "modifiedonbehalfby",
            "ownerid",
            "owningbusinessunit",
            "owningteam",
            "owninguser"};

            public EntityDependencyInfo(string logicalName)
            {
                LogicalName = logicalName;
                Dependencies = new HashSet<string>();
                Dependents = new HashSet<string>();

                var properties = TestBase.GetType(logicalName).GetProperties();
                PopulateDependencies(properties);
            }

            private void PopulateDependencies(IEnumerable<PropertyInfo> properties)
            {
                foreach (var property in properties.Where(p => !PropertiesToIgnore.Contains(p.Name.ToLower())))
                {
                    if (property.PropertyType == typeof(EntityReference))
                    {
                        var attribute = property.GetCustomAttribute(typeof(AttributeLogicalNameAttribute)) as AttributeLogicalNameAttribute;
                        if (attribute == null)
                        {
                            continue;
                        }
                        Dependencies.Add(attribute.LogicalName);
                        continue;
                    }

                    if (!property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                    {
                        continue;
                    }

                    var genericTypes = property.PropertyType.GetGenericArguments();
                    if (genericTypes.Length != 1) { continue; }
                    var genericType = genericTypes[0];
                    if (!typeof(Entity).IsAssignableFrom(genericType))
                    {
                        continue;
                    }

                    Dependents.Add(EntityHelper.GetEntityLogicalName(genericType));
                }
            }
        }
    }
}
