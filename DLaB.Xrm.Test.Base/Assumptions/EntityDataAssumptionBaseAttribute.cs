﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DLaB.Common;
using DLaB.Xrm.LocalCrm;
using Microsoft.Xrm.Sdk;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Assumptions
#else
namespace DLaB.Xrm.Test.Assumptions
#endif
{
    /// <summary>
    /// Base Class for Assumption Entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class EntityDataAssumptionBaseAttribute : Attribute
    {
        /// <summary>
        /// The AssumedEntities populated by the AddAssumedEntities call.
        /// </summary>
        protected AssumedEntities Assumptions { get; private set; }
        private IEnumerable<Type> _prerequisites;
        private IEnumerable<Type> Prerequisites => _prerequisites ?? (_prerequisites = GetPrerequisites());

        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix, and with any namespace values that come after Assumptions
        /// </summary>
        private string AssumptionsNamespaceRelativePath => GetAssumptionsNamespaceRelativePath(GetType());

        private static readonly Dictionary<string, Entity> EntitiesFromServerByAttributeType = new Dictionary<string, Entity>();
        private static IOrganizationService LocalServiceForEntitiesFromServer { get;set;}

        private Entity PreviouslyRetrievedEntity
        {
            get => EntitiesFromServerByAttributeType.ContainsKey(GetType().FullName ?? GetType().Name)
                    ? EntitiesFromServerByAttributeType[GetType().FullName ?? GetType().Name]
                    : null;

            set => EntitiesFromServerByAttributeType[GetType().FullName ?? GetType().Name] = value.ToSdkEntity();
        }

        private IEnumerable<Type> GetPrerequisites()
        {
            var preReqs = new List<Type>();
            var type = GetType();
            do
            {
                if (type.GetCustomAttribute(typeof(PrerequisiteAssumptionsAttribute), false) is PrerequisiteAssumptionsAttribute att)
                {
                    preReqs.AddRange(att.Prerequisites);
                }

                type = type.BaseType;
            } while (type != null);

            preReqs.Reverse();
            return preReqs;
        }
        /// <summary>
        /// Gets the name of the type, without the "Attribute" postfix, and with any namespace values that come after Assumptions
        /// </summary>
        private static string GetAssumptionsNamespaceRelativePath(Type type)
        {
            var name = type.FullName ?? "";
            var index = name.Contains("Assumptions.")
                ? name.Substring(0, name.LastIndexOf('.') + 1).LastIndexOf("Assumptions.", StringComparison.Ordinal) + "Assumptions.".Length
                : 0;
            if (index > 0)
            {
                name = name.Substring(index, name.Length - index);
                name = name.Replace('.', '\\');
                // Replace . with \ so it becomes a folder path
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
        internal void AddAssumedEntities(IOrganizationService service, AssumedEntities assumedEntities)
        {
            Assumptions = assumedEntities;
            AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(service, new HashSet<Type>());
        }

        /// <summary>
        /// Adds the assumed entities with pre req infinite loop prevention.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="currentlyProcessingPreReqs">The currently processing pre reqs.</param>
        protected void AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(IOrganizationService service, HashSet<Type> currentlyProcessingPreReqs)
        {
            var type = GetType();

            if (Assumptions.Contains(this))
            {
                return;
            }
            if (currentlyProcessingPreReqs.Contains(type))
            {
                ThrowErrorPreventingInfiniteLoop(type, currentlyProcessingPreReqs);
            }
            currentlyProcessingPreReqs.Add(type);
            AddPrerequisiteAssumptions(service, currentlyProcessingPreReqs);
            AddAssumedEntitiesInternal(service);
            currentlyProcessingPreReqs.Remove(type);
        }

        private void ThrowErrorPreventingInfiniteLoop(Type type, HashSet<Type> currentlyProcessingPreReqs)
        {
            if (currentlyProcessingPreReqs.Count == 1)
            {
                throw new Exception($"Prerequisite Assumption Loop!  {type} called itself!");
            }
            var sb = new StringBuilder();
            sb.Append(currentlyProcessingPreReqs.First().Name + " called " + currentlyProcessingPreReqs.Skip(1).First().Name);
            foreach (var prereq in currentlyProcessingPreReqs.Skip(2))
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
            var entity = (IsLocal(service) && LocalServiceForEntitiesFromServer != service)
                ? null
                : PreviouslyRetrievedEntity?.Clone();
            if (entity == null)
            {
                entity = RetrieveEntity(service);
                if (entity != null)
                {
                    PreviouslyRetrievedEntity = entity; 
                    LocalServiceForEntitiesFromServer = service;
                }
            }
            entity = VerifyAssumption(service, entity);
            Assumptions.Add(this, entity);
        }

        private void AddPrerequisiteAssumptions(IOrganizationService service, HashSet<Type> currentlyProcessingPreReqs)
        {
            foreach (var assumption in Prerequisites.Select(prereq => (EntityDataAssumptionBaseAttribute)Activator.CreateInstance(prereq))
                                                    .Where(a => !Assumptions.Contains(a)))
            {
                assumption.Assumptions = Assumptions;
                assumption.AddAssumedEntitiesWithPreReqInfiniteLoopPrevention(service, currentlyProcessingPreReqs);
            }
        }

        private bool IsLocal(IOrganizationService service)
        {
            var mock = service as FakeIOrganizationService;
            // If the service is a Mock, get the Actual Service to determine if it is local or not...
            return mock?.ActualService is LocalCrmDatabaseOrganizationService 
                || mock == null && service is LocalCrmDatabaseOrganizationService;
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
                var filePath = GetSerializedFilePath(AssumptionsNamespaceRelativePath);
                var mock = service as FakeIOrganizationService;
                if (!IsLocal(service)
                    || FileIsNullOrEmpty(filePath))
                {
                    throw new Exception($"Assumption {AssumptionsNamespaceRelativePath} was invalid!  The entity assumed to be there, was not found and no file was found at {filePath}.");
                }

                entity = GetTestEntityFromFile(AssumptionsNamespaceRelativePath);
                var localService = mock == null ? (LocalCrmDatabaseOrganizationService)service : (LocalCrmDatabaseOrganizationService)mock.ActualService;
                var isSelfReferencing = CreateForeignReferences(localService, entity);
                if (isSelfReferencing 
                    || entity.Id != Guid.Empty
                    && Assumptions.AlreadyCreatedAsEntityReference(entity.Id))
                {
                    UpdateEntity(service, entity);
                }
                else
                {
                    entity.Id = CreateEntity(service, entity);
                }
            }
            else if (Debugger.IsAttached)
            {
                var sdkEntity = entity.ToSdkEntity();
#if NET
                var serializedValue = sdkEntity.SerializeToJson(null, null, true);
#else
                var serializedValue = sdkEntity.Serialize(true);
#endif
            }

            return entity;
        }

        /// <summary>
        /// Creates all foreign references that don't exist.  This is usually due to the serialization assumptions grabbing more values than actually needed.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        private bool CreateForeignReferences(LocalCrmDatabaseOrganizationService service, Entity entity)
        {
            var isSelfReferencing = false;
            var toRemove = new List<string>();
            foreach (var attribute in entity.Attributes)
            {
                if (!(attribute.Value is EntityReference foreign))
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
                    Assumptions.AddCreatedEntityReference(CreateEntityFromEntityReference(service, foreign));
                }
            }

            foreach (var key in toRemove)
            {
                entity.Attributes.Remove(key);
            }

            return isSelfReferencing;
        }

        /// <summary>
        /// Creates the Entity from an EntityReference
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entityRef">The EntityReference</param>
        /// <returns></returns>
        protected virtual Guid CreateEntityFromEntityReference(IOrganizationService service, EntityReference entityRef)
        {
            return service.Create(new Entity {Id = entityRef.Id, LogicalName = entityRef.LogicalName});
        }

        /// <summary>
        /// Creates the Entity that has been loaded from the File Contents
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The Entity</param>
        /// <returns></returns>
        protected virtual Guid CreateEntity(IOrganizationService service, Entity entity)
        {
            return service.Create(entity);
        }

        /// <summary>
        /// Updates the Entity that has been loaded from the File Contents but was previously created from an EntityReference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        protected virtual void UpdateEntity(IOrganizationService service, Entity entity)
        {
            service.Update(entity);
        }

        private static Entity GetTestEntityFromFile(string fileName)
        {
            var path = GetSerializedFilePath(fileName);
            var text = File.ReadAllText(path);
#if NET
            return text.DeserializeJson<Entity>();
#else
            return text.DeserializeEntity();
#endif
        }

        private static string GetSerializedFilePath(string fileName)
        {
#if NET
            var fileType = ".json";
            var configPath = TestSettings.AssumptionJsonPath.Value;
#else
            var fileType = ".xml";
            var configPath = TestSettings.AssumptionXmlPath.Value;
#endif
            if (!fileName.EndsWith(fileType))
            {
                fileName += fileType;
            }

            // Check for normal path
            var path = Path.Combine(configPath, fileName);
            if (File.Exists(path))
            {
                return path;
            }

            // Check for bin directory
            var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(binPath))
            {
                return binPath;
            }

            // For NET 5 projects, some files get copied over with folder hierarchy.  Walk backwards to see if the directory substructure exists
            binPath = Path.GetDirectoryName(binPath);
            var parts = configPath.Split(new [] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                var newPath = Path.Combine(binPath, parts[i]);
                if (Directory.Exists(newPath))
                {
                    for(var j = i+1; j < parts.Length; j++)
                    {
                        newPath = Path.Combine(newPath, parts[j]);
                    }

                    newPath = Path.Combine(newPath, fileName);
                    if (File.Exists(newPath))
                    {
                        return newPath;
                    }
                }
            }

            return path;
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
