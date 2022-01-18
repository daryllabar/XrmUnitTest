using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Assumptions
#else
namespace DLaB.Xrm.Test.Assumptions
#endif
{
    /// <summary>
    /// Base Class for Association relationships that can be created via the Create Request
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class AssociationOnlyAssumptionBaseAttribute : EntityDataAssumptionBaseAttribute
    {
        /// <summary>
        /// Entity Reference of main Entity
        /// </summary>
        public abstract EntityReference Entity { get; }
        
        /// <summary>
        /// RelationshipName
        /// </summary>
        public abstract string Relationship { get; }

        /// <summary>
        /// Entities to Associate to the main entity
        /// </summary>
        public abstract IEnumerable<EntityReference> Entities { get; }

        /// <summary>
        /// Creates the Entity that has been loaded from the File Contents
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The Entity</param>
        /// <returns></returns>
        protected override Guid CreateEntity(IOrganizationService service, Entity entity)
        {
            service.Associate(Entity, Relationship, Entities.ToArray());
            return RetrieveEntity(service).Id;
        }
    }
}
