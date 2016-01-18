using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DLaB.Common;
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
        public static EntityDependency Mapper => Singleton.Value;

        #endregion Singleton Logic

        #region Properties

        #region Multithreaded Management

        // Some Unit Tests could be looping over the Types Collection while it is being updated, causing an exception
        // These properties help manage threading by keeping a readonly collection that doesn't get updated in the middle of enumerating it

        private readonly object _readWriteLock = new object();
        private int CurrentVersion { get; set; }
        private int TypesCopyVersion { get; set; }
        private ReadOnlyCollection<string> TypesImmutableCopy { get; set; }

        #endregion Multithreaded Management

        private LinkedList<EntityDependencyInfo> Types { get; }
        private Dictionary<string, EntityDependencyInfo> Infos { get; }
        private ConcurrentBag<string> Conflicts { get; }
        private ConcurrentQueue<string> DependencyOrderLog { get; }

        /// <summary>
        /// Gets the conflicting entity dependencies.
        /// </summary>
        /// <value>
        /// The conflicting entity order.
        /// </value>
        public IEnumerable<string> CircularCreationConflicts => Conflicts;

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be created without causing an error caused by a relationship constraint i.e. Create entity without linked entities existing
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<string> EntityCreationOrder => GetEntityCreationOrder();

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be deleted without causing an error caused by a relationship constraint i.e. Delete Contact before Opprotunity.
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<string> EntityDeletionOrder => EntityCreationOrder.Reverse();

        /// <summary>
        /// Gets the log of the structuring of the DepedencyOrder
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public IEnumerable<string> Log => DependencyOrderLog;

        private Queue<string> _circularReferenceResolutionOrder = new Queue<string>(new [] { "account", "contact", "lead"});

        /// <summary>
        /// Defines how to resolve Circular Dependencies that may occur when creating Entities
        /// </summary>
        /// <value>
        /// The circular reference resolution order.
        /// </value>
        public IEnumerable<string> CircularReferenceResolutionOrder
        {
            private get { return _circularReferenceResolutionOrder; }
            set
            {
                lock (_readWriteLock)
                {
                    DependencyOrderLog.Enqueue($"CircularReferenceResolutionOrder updated from {_circularReferenceResolutionOrder.ToCsv()} to {value.ToCsv()}");
                    _circularReferenceResolutionOrder = new Queue<string>(value);
                }
            }
        }

        #endregion Properties

        private EntityDependency()
        {
            Types = new LinkedList<EntityDependencyInfo>();
            Infos = new Dictionary<string, EntityDependencyInfo>();
            Conflicts = new ConcurrentBag<string>();
            DependencyOrderLog = new ConcurrentQueue<string>();
            CurrentVersion = 0;
            TypesCopyVersion = 0;
            TypesImmutableCopy = new ReadOnlyCollection<string>(new List<string>());
        }
        /// <summary>
        /// Adds the specified logical name to the collection of entities that are mapped.  Entities are cached so no work is performed when an entity type is added a second time
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        public void Add(string logicalName)
        {
            lock (_readWriteLock)
            {
                if (Infos.ContainsKey(logicalName))
                {
                    // Already added Id type, nothing to do.
                    return;
                }

                CurrentVersion++;
                SingleThreadAdd(logicalName);
            }
        }

        /// <summary>
        /// Runs in the Context of a lock, and therefor can contain non thread safe calls.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        private void SingleThreadAdd(string logicalName)
        {
            DependencyOrderLog.Enqueue("Processing entity type: " + logicalName);
            var info = new EntityDependencyInfo(logicalName, this);
            Infos.Add(logicalName, info);

            if (Types.Count == 0)
            {
                DependencyOrderLog.Enqueue("No values in the list.");
                info.Node = Types.AddFirst(info);
                return;
            }

            if (Types.First.Value.DependsOn(info))
            {
                DependencyOrderLog.Enqueue("The first value depends on the new info value.");
                info.Node = Types.AddFirst(info);

                /* Check for any Types that this new type is dependent on.
                 * Consider the Case of A -> B and B -> C
                 * A is added first, it becomes first.  C is added next, and it has no dependency on A or visa versa, so it is added last, then B is added, and it is added first, but C isn't moved
                 * Need to move C to the front, or list discrepency
                 */

                var existingDependent = Types.Skip(1).FirstOrDefault(existingInfo => info.DependsOn(existingInfo));
                if (existingDependent == null)
                {
                    DependencyOrderLog.Enqueue("Finished Processing " + logicalName);
                    return;
                }

                DependencyOrderLog.Enqueue(logicalName + " was added first, but existing dependency " + existingDependent.LogicalName + " was found!  Reordering list.");


                foreach (var type in Types)
                {
                    if (type == existingDependent)
                    {
                        DependencyOrderLog.Enqueue("Have walked the entire list and come to the dependency that needs to be moved without finding anything that it depends on.  Moving dependency to front.");
                        Types.Remove(existingDependent);
                        existingDependent.Node = Types.AddFirst(existingDependent);
                        DependencyOrderLog.Enqueue("New Order: " + string.Join(",", Types.Select(t => t.LogicalName)));
                        return;
                    }
                    if (type.DependsOn(existingDependent))
                    {
                        var conflict = string.Format("Circular reference found!  {0} was added, which depends on {1}, but {2} depends on {0} and was found before {1}.  If {1} is needed first, add it after {0}", logicalName, existingDependent.LogicalName, type.LogicalName);
                        Conflicts.Add(conflict);
                        DependencyOrderLog.Enqueue(conflict);
                    }
                }
            }

            // Try to add the new type as far to the end of the list as possible
            var dependent = Types.Skip(1).FirstOrDefault(existingInfo => existingInfo.DependsOn(info));
            if (dependent == null)
            {
                DependencyOrderLog.Enqueue("No dependent type found.  Adding to end of list.");
                info.Node = Types.AddLast(info);
            }
            else
            {
                DependencyOrderLog.Enqueue("Found dependent " + dependent.LogicalName + ".   Adding before.");
                info.Node = Types.AddBefore(dependent.Node, info);
            }
        }

        /// <summary>
        /// Gets the entity creation order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetEntityCreationOrder()
        {
            lock (_readWriteLock)
            {
                if (TypesCopyVersion != CurrentVersion)
                {
                    TypesImmutableCopy = new ReadOnlyCollection<string>(Types.Select(e => e.LogicalName).ToList());
                    TypesCopyVersion = CurrentVersion;
                }
            }
            return TypesImmutableCopy;
        }

        [DebuggerDisplay("{LogicalName}")]
        private class EntityDependencyInfo
        {
            //public HashSet<string> Dependencies { get; }
            public HashSet<string> Dependents { get; }
            public LinkedListNode<EntityDependencyInfo> Node { get; set; }
            public string LogicalName { get; }

            private static readonly HashSet<string> PropertiesToIgnore = new HashSet<string>{
            "createdby",
            "createdonbehalfby",
            "modifiedby",
            "modifiedonbehalfby",
            "ownerid",
            "owningbusinessunit",
            "owningteam",
            "owninguser"};

            public EntityDependencyInfo(string logicalName, EntityDependency dependency)
            {
                LogicalName = logicalName;
                //Dependencies = new HashSet<string>();
                Dependents = new HashSet<string>();

                var properties = TestBase.GetType(logicalName).GetProperties();
                PopulateDependencies(dependency, properties);
            }

            /// <summary>
            /// Returns true if the Entity Dependency Info depends on given Entity Dependency Info.
            /// </summary>
            /// <param name="info">The info to check to see if it is a dependency.</param>
            /// <returns></returns>
            public bool DependsOn(EntityDependencyInfo info)
            {
                //return Dependencies.Contains(info.LogicalName) || info.Dependents.Contains(LogicalName);
                return info.Dependents.Contains(LogicalName);
            }

            private void PopulateDependencies(EntityDependency dependency, IEnumerable<PropertyInfo> properties)
            {
                var preference = dependency.CircularReferenceResolutionOrder.ToList();
                var circularDependencies = preference.Contains(LogicalName) ? new HashSet<string>(preference.TakeWhile(p => p != LogicalName)) : new HashSet<string>();
                foreach (var property in properties.Where(p => !PropertiesToIgnore.Contains(p.Name.ToLower())))
                {
                    //if (property.PropertyType == typeof(EntityReference))
                    //{
                    //    var attribute = property.GetCustomAttribute(typeof(AttributeLogicalNameAttribute)) as AttributeLogicalNameAttribute;
                    //    if (attribute == null)
                    //    {
                    //        continue;
                    //    }
                    //    Dependencies.Add(attribute.LogicalName);
                    //    continue;
                    //}

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

                    var dependent = EntityHelper.GetEntityLogicalName(genericType);
                    if (circularDependencies.Contains(dependent))
                    {
                        if (dependent != LogicalName)
                        {
                            dependency.DependencyOrderLog.Enqueue($"Circular Reference Resolution Order has defined that {LogicalName} should not be dependent on {dependent}.  The dependency has been removed.");
                        }
                    }
                    else
                    {
                        Dependents.Add(dependent);
                    }
                }
            }
        }
    }
}
