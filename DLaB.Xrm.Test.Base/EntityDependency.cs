﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DLaB.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

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
        private ReadOnlyCollection<EntityDependencyInfo> TypesImmutableCopy { get; set; }

        #endregion Multithreaded Management

        private LinkedList<EntityDependencyNodeInfo> Types { get; set; }
        private Dictionary<string, EntityDependencyNodeInfo> Infos { get; }
        private ConcurrentQueue<string> DependencyOrderLog { get; }

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be created without causing an error caused by a relationship constraint i.e. Create entity without linked entities existing
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<EntityDependencyInfo> EntityCreationOrder => GetEntityCreationOrder();

        /// <summary>
        /// Gets the LogicalNames of Entities, in the order in which they can safely be deleted without causing an error caused by a relationship constraint i.e. Delete Contact before Opprotunity.
        /// </summary>
        /// <value>
        /// The entity deletion order.
        /// </value>
        public IEnumerable<string> EntityDeletionOrder => EntityCreationOrder.Reverse().Select(v => v.LogicalName);

        /// <summary>
        /// Gets the log of the structuring of the DepedencyOrder
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public IEnumerable<string> Log => DependencyOrderLog;

        #endregion Properties

        private EntityDependency()
        {
            Types = new LinkedList<EntityDependencyNodeInfo>();
            Infos = new Dictionary<string, EntityDependencyNodeInfo>();
            DependencyOrderLog = new ConcurrentQueue<string>();
            CurrentVersion = 0;
            TypesCopyVersion = 0;
            TypesImmutableCopy = new ReadOnlyCollection<EntityDependencyInfo>(new List<EntityDependencyInfo>());
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
                DependencyOrderLog.Enqueue($"Order: {string.Join(", ", Types.Select(t => t.LogicalName))}");
            }
        }

        /// <summary>
        /// Runs in the Context of a lock, and therefor can contain non thread safe calls.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        private void SingleThreadAdd(string logicalName)
        {
            DependencyOrderLog.Enqueue("Processing entity type: " + logicalName);

            var info = new EntityDependencyNodeInfo(logicalName);
            Infos.Add(logicalName, info);

            if (Types.Count == 0)
            {
                DependencyOrderLog.Enqueue("No values in the list.  Adding it as first.");
                info.Node = Types.AddFirst(info);
                return;
            }

            var lastDependOn = Types.LastOrDefault(t => info.DependsOn(t));
            var firstDependOnCurrent = Types.LastOrDefault(t => t.DependsOn(info));
            if (lastDependOn == null)
            {
                if (firstDependOnCurrent == null)
                {
                    DependencyOrderLog.Enqueue($"{logicalName} does not depend any any already processed types, and no already processed types depend on it.  Adding to end.");
                    info.Node = Types.AddLast(info);
                }
                else
                {    
                    DependencyOrderLog.Enqueue($"{logicalName} does not depend any any already processed types, but {firstDependOnCurrent.LogicalName} depends on it.  Adding before.");
                    info.Node = Types.AddBefore(firstDependOnCurrent.Node, info);
                }
                return;
            }

            if (!lastDependOn.DependsOn(info) && !Types.TakeWhile(t => !t.Equals(lastDependOn)).Any(t => t.DependsOn(info)))
            {
                DependencyOrderLog.Enqueue($"No type that has already been processed that occurs before the last type that {logicalName} depends on ({lastDependOn.LogicalName}), depends on the new type.  Adding to end.");
                info.Node = Types.AddAfter(lastDependOn.Node, info);
                return;
            }

            DependencyOrderLog.Enqueue("Reordering Types");
            var newOrder = new LinkedList<EntityDependencyNodeInfo>();
            foreach (var type in Types)
            {
                // Clear IsCurrentlyCyclic
                foreach (var dependency in type.Dependencies.Values.SelectMany(a => a))
                {
                    dependency.IsCurrentlyCyclic = false;
                }
                type.Node = null;
            }
            foreach (var type in Infos.Values)
            {
                PopulateNewOrder(newOrder, type);
            }

            Types = newOrder;
        }

        /// <summary>
        /// Recursively Populates the new order.
        /// </summary>
        /// <param name="newOrder">The new order.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException">Cyclic dependency found.</exception>
        private void PopulateNewOrder(LinkedList<EntityDependencyNodeInfo> newOrder, EntityDependencyNodeInfo type)
        {
            if (newOrder.Contains(type))
            {
                // Already visited
                if (type.IsCurrentlyBeingProcessed)
                {
                    throw new ArgumentException("Cyclic dependency found.");
                }
                return;
            }

            type.IsCurrentlyBeingProcessed = true;

            foreach (var dependency in type.Dependencies.
                Where(d => Infos.ContainsKey(d.Key)).
                Select(d => new {Type = Infos[d.Key], DependentAttributes = d.Value}))
            {
                if (dependency.Type.IsCurrentlyBeingProcessed)
                {
                    // Already Visited
                    foreach (var att in dependency.DependentAttributes)
                    {
                        att.IsCurrentlyCyclic = true;
                    }
                }
                else
                {
                    PopulateNewOrder(newOrder, dependency.Type);
                }
            }

            type.IsCurrentlyBeingProcessed = false;
            type.Node = newOrder.AddLast(type);

        }

        /// <summary>
        /// Gets the entity creation order.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<EntityDependencyInfo> GetEntityCreationOrder()
        {
            lock (_readWriteLock)
            {
                if (TypesCopyVersion != CurrentVersion)
                {
                    TypesImmutableCopy = new ReadOnlyCollection<EntityDependencyInfo>(
                            Types.Select(e => 
                                new EntityDependencyInfo(e.LogicalName, e.Dependencies.Values.
                                                                        SelectMany(v => v).
                                                                        Where(v => v.IsCurrentlyCyclic).
                                                                        Select(v => v.AttributeName))).ToList());
                    TypesCopyVersion = CurrentVersion;
                }
            }
            return TypesImmutableCopy;
        }

        [DebuggerDisplay("{LogicalName}")]
        private class EntityDependencyNodeInfo
        {
            /// <summary>
            /// Dictionary with the list of attributes whose value must exist before the entity does, keyed by logical name of the type of attribute
            /// </summary>
            /// <value>
            /// The dependents.
            /// </value>
            public Dictionary<string, List<CyclicAttribute>> Dependencies { get; }

            public LinkedListNode<EntityDependencyNodeInfo> Node { get; set; }
            public string LogicalName { get; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is currently being processed.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is currently being processed; otherwise, <c>false</c>.
            /// </value>
            public bool IsCurrentlyBeingProcessed { get; set; }

            private static readonly HashSet<string> PropertiesToIgnore = new HashSet<string>
            {
                "createdby",
                "createdonbehalfby",
                "modifiedby",
                "modifiedonbehalfby",
                "ownerid",
                "owningbusinessunit",
                "owningteam",
                "owninguser"
            };

            public EntityDependencyNodeInfo(string logicalName)
            {
                LogicalName = logicalName;
                Dependencies = new Dictionary<string, List<CyclicAttribute>>();
                IsCurrentlyBeingProcessed = false;

                PopulateDependencies(logicalName);
            }

            /// <summary>
            /// Returns true if the Entity Dependency Info depends on given Entity Dependency Info.
            /// </summary>
            /// <param name="nodeInfo">The info to check to see if it is a dependency.</param>
            /// <returns></returns>
            public bool DependsOn(EntityDependencyNodeInfo nodeInfo)
            {
                //return Dependencies.Contains(info.LogicalName) || info.Dependents.Contains(LogicalName);
                return Dependencies.ContainsKey(nodeInfo.LogicalName);
            }

            private void PopulateDependencies(string logicalName)
            {
                var properties = TestBase.GetType(logicalName).GetProperties();
                foreach (var property in properties.Where(p =>
                    // Skip Properties to Ignore
                    !PropertiesToIgnore.Contains(p.Name.ToLower()) &&
                    // Only process Properties that contain an attribute logical name, and a Relationship Schema Name
                    p.ContainsCustomAttributeTypes(typeof (AttributeLogicalNameAttribute), typeof (RelationshipSchemaNameAttribute))))
                {
                    var attribute = property.GetCustomAttribute<AttributeLogicalNameAttribute>();
                    var propertyType = property.PropertyType.GetCustomAttribute<EntityLogicalNameAttribute>(true);
                    Dependencies.AddOrAppend(propertyType.LogicalName, new CyclicAttribute(attribute.LogicalName, LogicalName == propertyType.LogicalName) {});
                }
            }
        }

        /// <summary>
        /// Allows
        /// </summary>
        [DebuggerDisplay("{AttributeName}, {IsCurrentlyCyclic}")]
        private class CyclicAttribute
        {
            public string AttributeName { get; }
            public bool IsCurrentlyCyclic { get; set; }

            public CyclicAttribute(string name, bool isCurrentlyCyclic)
            {
                AttributeName = name;
                IsCurrentlyCyclic = isCurrentlyCyclic;
            }
        }
    }
}
