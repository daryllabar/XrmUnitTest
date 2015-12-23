using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;
using static System.String;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// CRM Assertion helper class.
    /// </summary>
    public class AssertCrm
    {
        #region Properties

        private IOrganizationService Service { get; set; }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertCrm"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public AssertCrm(IOrganizationService service)
        {
            Service = service;    
        }

        #endregion // Constructors

        #region Exists (Entity)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public void Exists(Entity entity)
        {
            Exists(Service, entity);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, Entity entity)
        {
            Exists(service, entity, null, null);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void Exists(Entity entity, string message)
        {
            Exists(Service, entity, message);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, Entity entity, string message) 
        {
            Exists(service, entity, message, null);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists(Entity entity, string message, params object[] parameters) 
        {
            Exists(Service, entity, message, parameters);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, Entity entity, string message, params object[] parameters)
        {
            try
            {
                service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(false));
            }
            catch
            {
                HandleFail("AssertCrm.Exists", message, parameters );
            }
        }

        #endregion // Exists (Entity)

        #region Exists (EntityReference)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        [DebuggerHidden]
        public void Exists(EntityReference entityReference)
        {
            Exists(Service, entityReference);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public void Exists(Id id)
        {
            Exists(Service, id);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, EntityReference entityReference) 
        {
            Exists(service, entityReference, null, null);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void Exists(EntityReference entityReference, string message) 
        {
            Exists(Service, entityReference, message);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, EntityReference entityReference, string message) 
        {
            Exists(service, entityReference, message, null);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists(EntityReference entityReference, string message, params object[] parameters) 
        {
            Exists(Service, entityReference, message, parameters);
        }

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void Exists(IOrganizationService service, EntityReference entityReference, string message, params object[] parameters) 
        {
            try
            {
                service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(false));
            }
            catch
            {
                HandleFail("AssertCrm.Exists", message, parameters);
            }
        }

        #endregion // Exists (EntityReference)

        #region IsActive (Entity)

        /// <summary>
        /// Asserts whether the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public void IsActive<T>(T entity) where T : Entity
        {
            IsActive(Service, entity);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, T entity) where T : Entity
        {
            IsActive(service, entity, null, null);
        }

        /// <summary>
        /// Asserts whether the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void IsActive<T>(T entity, string message) where T : Entity
        {
            IsActive(Service, entity, message);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            IsActive(service, entity, message, null);
        }

        /// <summary>
        /// Asserts whether the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            IsActive(Service, entity, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, T entity, string message, params object[] parameters) where T : Entity
        {
            if (typeof (T) == typeof (Entity))
            {
                // Late Bound
                var isActive = LateBoundActivePropertyInfo.IsActive(service, entity);
                if (isActive == null)
                {
                    HandleInconclusive("AssertCrm.IsActive", "Unable to determine Status of Entity Type {0}", typeof(T).Name);
                }
                else if (isActive == false)
                {
                    HandleFail("AssertCrm.IsActive", message, parameters);
                }
            }
            else
            {
                // Early Bound
                IsActive<T>(service, entity.Id, message, parameters);
            }
        }

        #endregion // IsActive (Entity)

        #region IsActive (EntityReference)

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public void IsActive<T>(Guid id) where T : Entity
        {
            IsActive<T>(Service, id);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, Guid id) where T : Entity
        {
            IsActive<T>(service, id, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void IsActive<T>(Guid id, string message) where T : Entity
        {
            IsActive<T>(Service, id, message);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, Guid id, string message) where T : Entity
        {
            IsActive<T>(service, id, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(Guid id, string message, params object[] parameters) where T : Entity
        {
            IsActive<T>(Service, id, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, Guid id, string message, params object[] parameters) where T : Entity
        {
            var isActive = ActivePropertyInfo<T>.IsActive(service, id);
            if (isActive == null)
            {
                HandleInconclusive("AssertCrm.IsActive", "Unable to determine Status of Entity Type {0}", typeof(T).Name);
            }
            else if (isActive == false)
            {
                HandleFail("AssertCrm.IsActive", message, parameters);
            }
        }

        #endregion // IsActive (EntityReference)

        #region IsNotActive (Entity)

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(T entity) where T : Entity
        {
            IsNotActive(Service, entity);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, T entity) where T : Entity
        {
            IsNotActive(service, entity, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(T entity, string message) where T : Entity
        {
            IsNotActive(Service, entity, message);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            IsNotActive(service, entity, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            IsNotActive(Service, entity, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, T entity, string message, params object[] parameters) where T : Entity
        {
            if (typeof(T) == typeof(Entity))
            {
                // Late Bound
                var isActive = LateBoundActivePropertyInfo.IsActive(service, entity);
                if (isActive == null)
                {
                    HandleInconclusive("AssertCrm.IsNotActive", "Unable to determine Status of Entity Type {0}", typeof(T).Name);
                }
                else if (isActive == true)
                {
                    HandleFail("AssertCrm.IsActive", message, parameters);
                }
            }
            else
            {
                // Early Bound
                IsNotActive<T>(service, entity.Id, message, parameters);
            }
        }

        #endregion // IsNotActive (Entity)

        #region IsNotActive (EntityReference)

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Guid id) where T : Entity
        {
            IsNotActive<T>(Service, id);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Guid id) where T : Entity
        {
            IsNotActive<T>(service, id, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Guid id, string message) where T : Entity
        {
            IsNotActive<T>(Service, id, message);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Guid id, string message) where T : Entity
        {
            IsNotActive<T>(service, id, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Guid id, string message, params object[] parameters) where T : Entity
        {
            IsNotActive<T>(Service, id, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Guid id, string message, params object[] parameters) where T : Entity
        {
            var isActive = ActivePropertyInfo<T>.IsActive(service, id);
            if (isActive == null)
            {
                HandleInconclusive("AssertCrm.IsActive", "Unable to determine Status of Entity Type {0}", typeof(T).Name);
            }
            else if (isActive == true)
            {
                HandleFail("AssertCrm.IsActive", message, parameters);
            }
        }

        #endregion // IsNotActive (EntityReference)

        #region NotExists (Entity)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public void NotExists(Entity entity)
        {
            NotExists(Service, entity);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Entity entity)
        {
            NotExists(service, entity, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void NotExists(Entity entity, string message)
        {
            NotExists(Service, entity, message);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void NotExist(IOrganizationService service, Entity entity, string message)
        {
            NotExists(service, entity, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists(Entity entity, string message, params object[] parameters)
        {
            NotExists(Service, entity, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Entity entity, string message, params object[] parameters)
        {
            if (service.GetEntityOrDefault(entity.LogicalName, entity.Id) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion // NotExists (Entity)

        #region NotExists (EntityReference)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        [DebuggerHidden]
        public void NotExists(EntityReference entityReference)
        {
            NotExists(Service, entityReference);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, EntityReference entityReference)
        {
            NotExists(service, entityReference, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void NotExists(EntityReference entityReference, string message)
        {
            NotExists(Service, entityReference, message);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, EntityReference entityReference, string message)
        {
            NotExists(service, entityReference, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists(EntityReference entityReference, string message, params object[] parameters)
        {
            NotExists(Service, entityReference, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, EntityReference entityReference, string message, params object[] parameters)
        {
            if (service.GetEntityOrDefault(entityReference.LogicalName, entityReference.Id) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion // NotExists (EntityReference)

        #region NotExists (Id)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public void NotExists(Id id)
        {
            NotExists(Service, id);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Id id)
        {
            NotExists(service, id, null, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public void NotExists(Id id, string message)
        {
            NotExists(Service, id, message);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Id id, string message)
        {
            NotExists(service, id, message, null);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists(Id id, string message, params object[] parameters)
        {
            NotExists(Service, id, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Id id, string message, params object[] parameters)
        {
            if (service.GetEntityOrDefault(id.LogicalName, id.EntityId) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion // NotExists (Id)

        #region Handle

        [DebuggerHidden]
        private static void HandleFail(string assertName, string message, params object[] parameters )
        {
            message = message ?? Empty;
           
            message = $"{assertName} failed. " + (parameters == null ? message : Format(message, parameters));

            throw TestSettings.TestFrameworkProvider.Value.GetFailedException(message);
        }

        [DebuggerHidden]
        private static void HandleInconclusive(string assertName, string message, params object[] parameters )
        {
            message = message ?? Empty;
            message = $"{assertName} is inconclusive. " + (parameters == null ? message : Format(message, parameters));

            throw TestSettings.TestFrameworkProvider.Value.GetInconclusiveException(message);
        }

        #endregion // Handle
    }
}
