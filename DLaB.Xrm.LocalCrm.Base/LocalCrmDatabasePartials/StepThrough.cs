using System;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    // <summary>
    // This Class is to allow for Known Exceptions to bubble up to the calling code where they are called from, rather than where they are thrown
    // </summary>
    partial class LocalCrmDatabase
    {
        //[DebuggerStepThrough]
        //public static Guid AssociateN2N<TOne, TTwo>(LocalCrmDatabaseOrganizationService service,
        //    EntityReference one, EntityReference two, Relationship relationship)
        //    where TOne: Entity
        //    where TTwo : Entity
        //{
        //    var delay = new DelayedException();
        //    var id = AssociateN2N<TOne, TTwo>(service, one, two, relationship, delay);
        //    if (delay.Exception != null)
        //    {
        //        throw delay.Exception;
        //    }
        //    return id;
        //}

        [DebuggerStepThrough]
        public static Guid Create<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            var delay = new DelayedException();
            var id = Create(service, entity, delay);
            if (delay.Exception != null)
            {
                throw delay.Exception;
            }
            return id;
        }

        [DebuggerStepThrough]
        public static void Update<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            var delay = new DelayedException();
            Update(service, entity, delay);
            if (delay.Exception != null)
            {
                throw delay.Exception;
            }
        }

        [DebuggerStepThrough]
        public static T Read<T>(LocalCrmDatabaseOrganizationService service, Guid id, ColumnSet cs) where T : Entity
        {
            var delay = new DelayedException();
            var result = Read<T>(service, id, cs, delay);
            if (delay.Exception != null)
            {
                throw delay.Exception;
            }
            return result;
        }

        [DebuggerStepThrough]
        public static EntityCollection ReadEntitiesByAttribute<T>(LocalCrmDatabaseOrganizationService service, QueryByAttribute query) where T : Entity
        {
            var delay = new DelayedException();
            var result = ReadEntitiesByAttribute<T>(service, query, delay);
            if (delay.Exception != null)
            {
                throw delay.Exception;
            }
            return result;
        }

        [DebuggerStepThrough]
        public static void Delete<T>(LocalCrmDatabaseOrganizationService service, Guid id) where T : Entity
        {
            var delay = new DelayedException();
            Delete<T>(service, id, delay);
            if (delay.Exception != null)
            {
                throw delay.Exception;
            }
        }
        
        //[DebuggerStepThrough]
        //public static void DisassociateN2N<TOne, TTwo>(LocalCrmDatabaseOrganizationService service,
        //    EntityReference one, EntityReference two, Relationship relationship)
        //    where TOne : Entity
        //    where TTwo : Entity
        //{
        //    var delay = new DelayedException();
        //    DisassociateN2N<TOne, TTwo>(service, one, two, relationship, delay);
        //    if (delay.Exception != null)
        //    {
        //        throw delay.Exception;
        //    }
        //}

        [DebuggerStepThrough]
        private class DelayedException
        {
            public Exception Exception { get; set; }
        }
    }
}
