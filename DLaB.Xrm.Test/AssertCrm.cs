using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;

namespace DLaB.Xrm.Test
{
    public class AssertCrm
    {
        #region Properties

        private IOrganizationService Service { get; set; }

        #endregion // Properties

        #region Constructors

        public AssertCrm(IOrganizationService service)
        {
            Service = service;    
        }

        #endregion // Constructors

        #region Exists (Entity)

        [DebuggerHidden]
        public void Exists<T>(T entity) where T : Entity
        {
            Exists(Service, entity);
        }

        [DebuggerHidden]
        public static void Exists<T>(IOrganizationService service, T entity) where T : Entity
        {
            Exists(service, entity, null, null);
        }

        [DebuggerHidden]
        public void Exists<T>(T entity, string message) where T : Entity
        {
            Exists(Service, entity, message);
        }

        [DebuggerHidden]
        public static void Exists<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            Exists(service, entity, message, null);
        }

        [DebuggerHidden]
        public void Exists<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            Exists(Service, entity, message, parameters);
        }

        [DebuggerHidden]
        public static void Exists<T>(IOrganizationService service, T entity, string message, params object[] parameters) where T: Entity
        {
            try
            {
                service.GetEntity<T>(entity.Id, new ColumnSet(false));
            }
            catch
            {
                HandleFail("AssertCrm.Exists", message, parameters );
            }
        }

        #endregion // Exists (Entity)

        #region Exists (EntityReference)

        [DebuggerHidden]
        public void Exists(EntityReference entityReference)
        {
            Exists(Service, entityReference);
        }

        [DebuggerHidden]
        public void Exists(Id id)
        {
            Exists(Service, (EntityReference)id);
        }

        [DebuggerHidden]
        public static void Exists(IOrganizationService service, EntityReference entityReference) 
        {
            Exists(service, entityReference, null, null);
        }

        [DebuggerHidden]
        public void Exists(EntityReference entityReference, string message) 
        {
            Exists(Service, entityReference, message);
        }

        [DebuggerHidden]
        public static void Exists(IOrganizationService service, EntityReference entityReference, string message) 
        {
            Exists(service, entityReference, message, null);
        }

        [DebuggerHidden]
        public void Exists(EntityReference entityReference, string message, params object[] parameters) 
        {
            Exists(Service, entityReference, message, parameters);
        }

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

        [DebuggerHidden]
        public void IsActive<T>(T entity) where T : Entity
        {
            IsActive(Service, entity);
        }

        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, T entity) where T : Entity
        {
            IsActive(service, entity, null, null);
        }

        [DebuggerHidden]
        public void IsActive<T>(T entity, string message) where T : Entity
        {
            IsActive(Service, entity, message);
        }

        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            IsActive(service, entity, message, null);
        }

        [DebuggerHidden]
        public void IsActive<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            IsActive(Service, entity, message, parameters);
        }

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

        [DebuggerHidden]
        public void IsActive<T>(Guid id) where T : Entity
        {
            IsActive<T>(Service, id);
        }

        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, Guid id) where T : Entity
        {
            IsActive<T>(service, id, null, null);
        }

        [DebuggerHidden]
        public void IsActive<T>(Guid id, string message) where T : Entity
        {
            IsActive<T>(Service, id, message);
        }

        [DebuggerHidden]
        public static void IsActive<T>(IOrganizationService service, Guid id, string message) where T : Entity
        {
            IsActive<T>(service, id, message, null);
        }

        [DebuggerHidden]
        public void IsActive<T>(Guid id, string message, params object[] parameters) where T : Entity
        {
            IsActive<T>(Service, id, message, parameters);
        }

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

        [DebuggerHidden]
        public void IsNotActive<T>(T entity) where T : Entity
        {
            IsNotActive(Service, entity);
        }

        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, T entity) where T : Entity
        {
            IsNotActive(service, entity, null, null);
        }

        [DebuggerHidden]
        public void IsNotActive<T>(T entity, string message) where T : Entity
        {
            IsNotActive(Service, entity, message);
        }

        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            IsNotActive(service, entity, message, null);
        }

        [DebuggerHidden]
        public void IsNotActive<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            IsNotActive(Service, entity, message, parameters);
        }

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

        [DebuggerHidden]
        public void IsNotActive<T>(Guid id) where T : Entity
        {
            IsNotActive<T>(Service, id);
        }

        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Guid id) where T : Entity
        {
            IsNotActive<T>(service, id, null, null);
        }

        [DebuggerHidden]
        public void IsNotActive<T>(Guid id, string message) where T : Entity
        {
            IsNotActive<T>(Service, id, message);
        }

        [DebuggerHidden]
        public static void IsNotActive<T>(IOrganizationService service, Guid id, string message) where T : Entity
        {
            IsNotActive<T>(service, id, message, null);
        }

        [DebuggerHidden]
        public void IsNotActive<T>(Guid id, string message, params object[] parameters) where T : Entity
        {
            IsNotActive<T>(Service, id, message, parameters);
        }

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

        [DebuggerHidden]
        public void NotExists<T>(T entity) where T : Entity
        {
            NotExists(Service, entity);
        }

        [DebuggerHidden]
        public static void NotExists<T>(IOrganizationService service, T entity) where T : Entity
        {
            NotExists(service, entity, null, null);
        }

        [DebuggerHidden]
        public void NotExists<T>(T entity, string message) where T : Entity
        {
            NotExists(Service, entity, message);
        }

        [DebuggerHidden]
        public static void NotExists<T>(IOrganizationService service, T entity, string message) where T : Entity
        {
            NotExists(service, entity, message, null);
        }

        [DebuggerHidden]
        public void NotExists<T>(T entity, string message, params object[] parameters) where T : Entity
        {
            NotExists(Service, entity, message, parameters);
        }

        [DebuggerHidden]
        public static void NotExists<T>(IOrganizationService service, T entity, string message, params object[] parameters) where T : Entity
        {
            if (service.GetEntityOrDefault(entity.LogicalName, entity.Id) != null)
            {
                HandleFail("AssertCrm.NotExists", message, parameters);
            }
        }

        #endregion // NotExists (Entity)

        #region NotExists (EntityReference)

        [DebuggerHidden]
        public void NotExists(EntityReference entityReference)
        {
            NotExists(Service, entityReference);
        }

        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, EntityReference entityReference)
        {
            NotExists(service, entityReference, null, null);
        }

        [DebuggerHidden]
        public void NotExists(EntityReference entityReference, string message)
        {
            NotExists(Service, entityReference, message);
        }

        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, EntityReference entityReference, string message)
        {
            NotExists(service, entityReference, message, null);
        }

        [DebuggerHidden]
        public void NotExists(EntityReference entityReference, string message, params object[] parameters)
        {
            NotExists(Service, entityReference, message, parameters);
        }

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

        [DebuggerHidden]
        public void NotExists(Id id)
        {
            NotExists(Service, id);
        }

        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Id id)
        {
            NotExists(service, id, null, null);
        }

        [DebuggerHidden]
        public void NotExists(Id id, string message)
        {
            NotExists(Service, id, message);
        }

        [DebuggerHidden]
        public static void NotExists(IOrganizationService service, Id id, string message)
        {
            NotExists(service, id, message, null);
        }

        [DebuggerHidden]
        public void NotExists(Id id, string message, params object[] parameters)
        {
            NotExists(Service, id, message, parameters);
        }

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
            message = message ?? String.Empty;
            if (parameters == null)
            {
                throw new AssertFailedException(String.Format("{0} failed. {1}", assertName, message));
            }
            else
            {
                throw new AssertFailedException(String.Format("{0} failed. {1}", assertName, String.Format(message, parameters)));
            }
        }

        [DebuggerHidden]
        private static void HandleInconclusive(string assertName, string message, params object[] parameters )
        {
            message = message ?? String.Empty;
            if (parameters == null)
            {
                throw new AssertInconclusiveException(String.Format("{0} is inconclusive. {1}", assertName, message));
            }
            else
            {
                throw new AssertInconclusiveException(String.Format("{0} is inconclusive. {1}", assertName, String.Format(message, parameters)));
            }
        }

        #endregion // Handle
    }
}
