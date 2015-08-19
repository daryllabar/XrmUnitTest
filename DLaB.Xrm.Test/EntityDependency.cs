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

        #region Properties

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

        public List<string> ConflictingEntityOrder { get; set; } 

        #endregion Properties


        private EntityDependency()
        {
            Types = new LinkedList<EntityDependencyInfo>();
            Infos = new Dictionary<string, EntityDependencyInfo>();
            ConflictingEntityOrder = new List<string>();
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

            if (Types.Count == 0 || Types.First.Value.DependsOn(info))
            {
                // No values in the list or the first value depends on the new info value
                info.Node = Types.AddFirst(info);
                
                /* Check for any Types that this new type is dependent on.
                 * Consider the Case of A -> B and B -> C
                 * A is added first, it becomes first.  C is added next, and it has no dependency on A or visa versa, so it is added last, then B is added, and it is added first, but C isn't moved
                 * Need to move C to the front, or list discrepency
                 */

                var existingDependent = Types.Skip(1).FirstOrDefault(existingInfo => info.DependsOn(existingInfo));
                if (existingDependent == null) { return; }

                foreach (var type in Types)
                {
                    if (type == existingDependent)
                    {
                        // Have walked the entire list and come to the one that needs to be moved without finding anything that it depends on, free to move.
                        Types.Remove(existingDependent);
                        existingDependent.Node = Types.AddFirst(existingDependent);
                        return;
                    }
                    if (existingDependent.Dependencies.Contains(type.LogicalName))
                    {
                        ConflictingEntityOrder.Add(String.Format("Circular Reference Found!  {0} was added, which depends on {1}, but {2} depends on {0} and was found before {1}.  If {1} is needed first, add it after {0}", logicalName, existingDependent.LogicalName, type.LogicalName));
                        return;
                    }
                }

            }

            var dependent = Types.Skip(1).FirstOrDefault(existingInfo => existingInfo.DependsOn(info));
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

            /// <summary>
            /// Returns true if the Entity Dependency Info depends on given Entity Dependency Info.
            /// </summary>
            /// <param name="info">The info to check to see if it is a dependency.</param>
            /// <returns></returns>
            public bool DependsOn(EntityDependencyInfo info)
            {
                return Dependencies.Contains(info.LogicalName) || info.Dependents.Contains(LogicalName);
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
