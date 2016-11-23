using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DLaB.Common;
using DLaB.Xrm.LocalCrm;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    /// <summary>
    /// Base Class for Assumption Entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class EntityDataAssumptionBaseAttribute : Attribute
    {
        private IEnumerable<Type> _prerequisites;
        private IEnumerable<Type> Prerequisites
        {
            get
            {
                return _prerequisites ??
                    (_prerequisites = (GetType().GetCustomAttributes(true).Select(a => a as PrerequisiteAssumptionsAttribute).FirstOrDefault()
                                      ??
                                      new PrerequisiteAssumptionsAttribute()
                                      ).Prerequisites);
            }
        }

        private AssumedEntities _assumedEntities;
        private HashSet<Type> _currentlyProcessingPreReqs;


        /// <summary>
        /// Gets the assumed entity.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="assumption">The assumption.</param>
        /// <returns></returns>
        protected TEntity GetAssumedEntity<TAttribute, TEntity>(IAssumptionEntityType<TAttribute, TEntity> assumption)
            where TAttribute : EntityDataAssumptionBaseAttribute
            where TEntity : Entity
        {
            return GetAssumedEntity<TAttribute, TEntity>();
        }

        /// <summary>
        /// Checks to ensure that the assumption being asked for has been defined in the PrerequisiteAssumptions for the current Assumption
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected TEntity GetAssumedEntity<TAttribute, TEntity>()
            where TAttribute : EntityDataAssumptionBaseAttribute
            where TEntity : Entity
        {
            if (!Prerequisites.Any())
            {
                throw new Exception(string.Format(
                    "Assumption {0} is attempting to retrieve Assumption of type {1}, but it has not defined a PrerequisiteAssumptions Attribute.{2}{2}" +
                    "Please add a PrerequisiteAssumption Attribute with a(n) {1} in it's constructor to Assumption {0}.",
                    ShortName, GetShortName(typeof(TAttribute)), Environment.NewLine));
            }

            if (Prerequisites.All(t => t != typeof(TAttribute)))
            {
                throw new Exception(string.Format(
                    "Assumption {0} is attempting to retrieve Assumption of type {1}, but it has not defined it in it's PrerequisiteAssumptions Attribute.{2}{2}" +
                    "Please add a(n) {1} to the PrerequisiteAssumption Attribute in Assumption {0}.",
                    ShortName, GetShortName(typeof(TAttribute)), Environment.NewLine));
            }

            return _assumedEntities.Get<TAttribute, TEntity>();
        }

        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix
        /// </summary>
        private string ShortName => GetShortName(GetType());

        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix, and with any namespace values that come after Assumptions
        /// </summary>
        private string AssumptionsNamespaceRelativePath => GetAssumptionsNamespaceRelativePath(GetType());

        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix
        /// </summary>
        private static string GetShortName(Type type)
        {
            var name = type.Name;
            if (name.EndsWith("Attribute", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(0, name.Length - "Attribute".Length);
            }
            return name;
        }

        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix, and with any namespace values that come after Assumptions
        /// </summary>
        private static string GetAssumptionsNamespaceRelativePath(Type type)
        {
            var name = type.FullName;
            var index = name.Substring(0, name.LastIndexOf('.') + 1).LastIndexOf("Assumptions.", StringComparison.Ordinal) + "Assumptions.".Length;
            if (index > 0)
            {
                name = name.Substring(index, name.Length - index);
                // Replace . with \ so it becomes a folder path
                name = name.Replace('.', '\\');
            }
            if (name.EndsWith("Attribute", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(0, name.Length - "Attribute".Length);
            }
            return name;
        }

        /// <summary>
        /// Return the entity assumed to exist or null if not found.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        protected abstract Entity RetrieveEntity(IOrganizationService service);

        /// <summary>
        /// Adds the entities assumed to exist to the AssumedEntities Collection
        /// </summary>
        /// <param name="service"></param>
        /// <param name="assumedEntities">Collection of Assumptions that have already been verified to be true</param>
        /// <returns></returns>
        public void AddAssumedEntities(IOrganizationService service, AssumedEntities assumedEntities)
        {
            AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(service, assumedEntities, new HashSet<Type>());
        }

        /// <summary>
        /// Adds the assumed entities with pre req infinite loop prevention.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="assumedEntities">The assumed entities.</param>
        /// <param name="currentlyProcessingPreReqs">The currently processing pre reqs.</param>
        protected void AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(IOrganizationService service, AssumedEntities assumedEntities,
                                                                                  HashSet<Type> currentlyProcessingPreReqs)
        {
            _assumedEntities = assumedEntities;
            _currentlyProcessingPreReqs = currentlyProcessingPreReqs;
            var type = GetType();

            if (_assumedEntities.Contains(this))
            {
                return;
            }
            if (_currentlyProcessingPreReqs.Contains(type))
            {
                ThrowErrorPreventingInfiniteLoop(type);
            }
            _currentlyProcessingPreReqs.Add(type);
            AddPrerequisiteAssumptions(service);
            AddAssumedEntitiesInternal(service);
            _currentlyProcessingPreReqs.Remove(type);
        }

        private void ThrowErrorPreventingInfiniteLoop(Type type)
        {
            if (_currentlyProcessingPreReqs.Count == 1)
            {
                throw new Exception($"Prerequisite Assumption Loop!  {type} called itself!");
            }
            var sb = new StringBuilder();
            sb.Append(_currentlyProcessingPreReqs.First().Name + " called " + _currentlyProcessingPreReqs.Skip(1).First().Name);
            foreach (var prereq in _currentlyProcessingPreReqs.Skip(2))
            {
                sb.Append(" which called " + prereq.Name);
            }
            sb.Append(" which attempt to call " + type.Name);
            throw new Exception("Prerequisite Assumption Loop!  " + sb);
        }

        /// <summary>
        /// Internal Implementation of Adds Assumed Entities.
        /// </summary>
        /// <param name="service">The service.</param>
        protected virtual void AddAssumedEntitiesInternal(IOrganizationService service)
        {
            var entity = RetrieveEntity(service);
            entity = VerifyAssumption(service, entity);
            _assumedEntities.Add(this, entity);
        }


        /// <summary>
        /// Adds the assumed entity.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="assumption">The assumption.</param>
        protected void AddAssumedEntity(IOrganizationService service, EntityDataAssumptionBaseAttribute assumption)
        {
            assumption.AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(service, _assumedEntities, _currentlyProcessingPreReqs);
        }

        private void AddPrerequisiteAssumptions(IOrganizationService service)
        {
            foreach (var assumption in Prerequisites.Select(prereq => (EntityDataAssumptionBaseAttribute)Activator.CreateInstance(prereq))
                                                    .Where(a => !_assumedEntities.Contains(a)))
            {

                assumption.AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(service, _assumedEntities, _currentlyProcessingPreReqs);
            }
        }


        /// <summary>
        /// Throws an error if Entity is null and it's using a real CRM database
        /// -or-
        /// Throws an error if the CRM database is a local CRM database, and there are no serialized version of the files
        /// to deserialize
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        private Entity VerifyAssumption(IOrganizationService service, Entity entity)
        {
            if (entity == null)
            {
                var mock = service as FakeIOrganizationService;
                // If the service is a Mock, get the Actual Service to determine if it is local or not...
                if (
                    (mock != null && !(mock.ActualService is LocalCrmDatabaseOrganizationService)) ||
                    (mock == null && !(service is LocalCrmDatabaseOrganizationService)) ||
                    FileIsNullOrEmpty(GetSerializedFilePath(AssumptionsNamespaceRelativePath)))
                {
                    throw new Exception($"Assumption {AssumptionsNamespaceRelativePath} was invalid!  The entity assumed to be there, was not found.");
                }

                entity = GetTestEntityFromXml(AssumptionsNamespaceRelativePath);
                var localService = mock == null ? (LocalCrmDatabaseOrganizationService)service : (LocalCrmDatabaseOrganizationService)mock.ActualService;
                var isSelfReferencing = CreateForeignReferences(localService, entity);
                if (isSelfReferencing)
                {
                    service.Update(entity);
                }
                else
                {
                    entity.Id = service.Create(entity);
                }
            }
            else if (Debugger.IsAttached)
            {
                Common.VersionControl.SourceControl.CheckoutAndUpdateFileIfDifferent(GetSerializedFilePath(AssumptionsNamespaceRelativePath), entity.Serialize(true));
            }

            return entity;
        }

        /// <summary>
        /// Creates all foreign references that don't exist.  This is usally due to the serialization grabbing more values than actually needed.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        private bool CreateForeignReferences(LocalCrmDatabaseOrganizationService service, Entity entity)
        {
            var isSelfReferencing = false;
            var toRemove = new List<string>();
            foreach (var attribute in entity.Attributes)
            {
                var foreign = attribute.Value as EntityReference;
                if (foreign == null)
                {
                    continue;
                }

                // Check to makes sure the type has been defined.  Don't create the Foreign Reference, and remove the attribute from the collection.
                if (!service.Info.IsTypeDefined(foreign.LogicalName))
                {
                    toRemove.Add(attribute.Key);
                    continue;
                }

                if (foreign.Id == entity.Id)
                {
                    isSelfReferencing = true;
                }

                if (service.GetEntitiesById(foreign.LogicalName, foreign.Id).Count == 0)
                {
                    service.Create(new Entity { Id = foreign.Id, LogicalName = foreign.LogicalName });
                }
            }

            foreach (var key in toRemove)
            {
                entity.Attributes.Remove(key);
            }

            return isSelfReferencing;
        }

        private static Entity GetTestEntityFromXml(string fileName)
        {
            var path = GetSerializedFilePath(fileName);
            return File.ReadAllText(path).DeserializeEntity();
        }

        private static string GetSerializedFilePath(string fileName)
        {
            if (!fileName.EndsWith(".xml"))
            {
                fileName = fileName + ".xml";
            }

            return Path.Combine(TestSettings.AssumptionXmlPath.Value, fileName);
        }

        private static bool FileIsNullOrEmpty(string filePath)
        {
            return !(File.Exists(filePath) && new FileInfo(filePath).Length > 0);
        }

        // Has to be protected inner class since only EntityDataAssumptionBase types should use it.
        /// <summary>
        /// Lists the EntityDataAssumptions that are required by the decorated class.  These assumptions will be loaded automatically by the EntityDataAssumptionBase.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class PrerequisiteAssumptionsAttribute : Attribute
        {
            /// <summary>
            /// Gets or sets the prerequisites.
            /// </summary>
            /// <value>
            /// The prerequisites.
            /// </value>
            public IEnumerable<Type> Prerequisites { get; set; }

            // ReSharper disable once UnusedMember.Local
            private PrerequisiteAssumptionsAttribute()
            {
                Prerequisites = new Type[0];
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PrerequisiteAssumptionsAttribute" /> class.
            /// </summary>
            /// <param name="prerequisites">The prerequisites.</param>
            public PrerequisiteAssumptionsAttribute(params Type[] prerequisites)
            {
                Prerequisites = prerequisites;
            }
        }
    }
}
