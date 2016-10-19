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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertCrm"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public AssertCrm(IOrganizationService service)
        {
            Service = service;    
        }

        #endregion Constructors

        #region Exists (Entity)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists(Entity entity, string message = null, params object[] parameters) 
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
        public static void Exists(IOrganizationService service, Entity entity, string message = null, params object[] parameters)
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

        #endregion Exists (Entity)

        #region Exists (EntityReference)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists(EntityReference entityReference, string message = null, params object[] parameters) 
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
        public static void Exists(IOrganizationService service, EntityReference entityReference, string message = null, params object[] parameters) 
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

        #endregion Exists (EntityReference)

        #region Exists (Id)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists(Id entity, string message = null, params object[] parameters)
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
        public static void Exists(IOrganizationService service, Id entity, string message = null, params object[] parameters)
        {
            try
            {
                service.Retrieve(entity.LogicalName, entity.EntityId, new ColumnSet(false));
            }
            catch
            {
                HandleFail("AssertCrm.Exists", message, parameters);
            }
        }

        #endregion Exists (Id)

        #region Exists (Id<T>)

        /// <summary>
        /// Asserts that the entity exists in Crm.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void Exists<T>(Id<T> entity, string message = null, params object[] parameters) where T: Entity
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
        public static void Exists<T>(IOrganizationService service, Id<T> entity, string message = null, params object[] parameters) where T : Entity
        {
            try
            {
                service.Retrieve(entity.LogicalName, entity.EntityId, new ColumnSet(false));
            }
            catch
            {
                HandleFail("AssertCrm.Exists", message, parameters);
            }
        }

        #endregion Exists (Id<T>)

        #region IsActive (Entity)

        /// <summary>
        /// Asserts whether the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(T entity, string message = null, params object[] parameters) where T : Entity
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
        public static void IsActive<T>(IOrganizationService service, T entity, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsActive (Entity)

        #region IsActive (EntityReference)

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(Guid id, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsActive (EntityReference)

        #region IsActive (Id)

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(Id id, string message = null, params object[] parameters) where T : Entity
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
        public static void IsActive<T>(IOrganizationService service, Id id, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsActive (Id)

        #region IsActive (Id<T>)

        /// <summary>
        /// Asserts that the specified entity is active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsActive<T>(Id<T> id, string message = null, params object[] parameters) where T : Entity
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
        public static void IsActive<T>(IOrganizationService service, Id<T> id, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsActive (Id)

        #region IsNotActive (Entity)

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(T entity, string message = null, params object[] parameters) where T : Entity
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
        public static void IsNotActive<T>(IOrganizationService service, T entity, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsNotActive (Entity)

        #region IsNotActive (EntityReference)
        
        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Guid id, string message = null, params object[] parameters) where T : Entity
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
        public static void IsNotActive<T>(IOrganizationService service, Guid id, string message = null, params object[] parameters) where T : Entity
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

        #endregion IsNotActive (EntityReference)

        #region IsNotActive (Id)

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Id id, string message = null, params object[] parameters) where T : Entity
        {
            IsNotActive<T>(Service, id, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The id.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Id id, string message = null, params object[] parameters) where T : Entity
        {
            if (typeof(T) == typeof(Entity))
            {
                // Late Bound
                var isActive = LateBoundActivePropertyInfo.IsActive(service, id.Entity);
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
                IsNotActive<T>(service, id.EntityId, message, parameters);
            }
        }

        #endregion IsNotActive (Id)

        #region IsNotActive (Id<T>)

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void IsNotActive<T>(Id<T> id, string message = null, params object[] parameters) where T : Entity
        {
            IsNotActive(Service, id, message, parameters);
        }

        /// <summary>
        /// Asserts that the specified entity is not active.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="id">The id.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Id<T> id, string message = null, params object[] parameters) where T : Entity
        {
            if (typeof(T) == typeof(Entity))
            {
                // Late Bound
                var isActive = LateBoundActivePropertyInfo.IsActive(service, id.Entity);
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
                IsNotActive<T>(service, id.EntityId, message, parameters);
            }
        }

        #endregion IsNotActive (Id<T>)

        #region NotExists (Entity)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists(Entity entity, string message = null, params object[] parameters)
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
        public static void NotExists(IOrganizationService service, Entity entity, string message = null, params object[] parameters)
        {
            if (service.GetEntityOrDefault(entity.LogicalName, entity.Id) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion NotExists (Entity)

        #region NotExists (EntityReference)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="entityReference">The entity reference.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists(EntityReference entityReference, string message = null, params object[] parameters)
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
        public static void NotExists(IOrganizationService service, EntityReference entityReference, string message = null, params object[] parameters)
        {
            if (service.GetEntityOrDefault(entityReference.LogicalName, entityReference.Id) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion NotExists (EntityReference)

        #region NotExists (Id)

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

        #endregion NotExists (Id)

        #region NotExists (Id<T>)

        /// <summary>
        /// Asserts that the specified entity does not exist.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        [DebuggerHidden]
        public void NotExists<T>(Id<T> id, string message, params object[] parameters) where T : Entity
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
        public static void NotExists<T>(IOrganizationService service, Id<T> id, string message, params object[] parameters) where T : Entity
        {
            if (service.GetEntityOrDefault(id.LogicalName, id.EntityId) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion NotExists (Id)

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

        #endregion Handle
    }
}
