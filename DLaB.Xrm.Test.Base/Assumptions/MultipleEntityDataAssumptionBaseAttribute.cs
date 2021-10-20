﻿using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

#if NET
namespace DataverseUnitTest.Assumptions
#else
namespace DLaB.Xrm.Test.Assumptions
#endif
{
    /// <summary>
    /// Assumption Base Attribute that allows for combining multiple assumptions into one
    /// </summary>
    public abstract class MultipleEntityDataAssumptionBaseAttribute : EntityDataAssumptionBaseAttribute
    {
        /// <summary>
        /// Gets the assumptions.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<EntityDataAssumptionBaseAttribute> GetAssumptions();

        /// <summary>
        /// Return the entity assumed to exist or null if not found.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        protected sealed override Entity RetrieveEntity(IOrganizationService service)
        {
            throw new NotSupportedException("MultipleEntityDataAssumptionBase contains more than one Assumed Entity");
        }

        /// <summary>
        /// Returns the entities assumed to exist.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        protected override void AddAssumedEntitiesInternal(IOrganizationService service)
        {
            foreach (var assumption in GetAssumptions())
            {
                assumption.AddAssumedEntities(service, Assumptions);
            }
        }

        /// <summary>
        /// Used to allow the Aggregate Multi-Entity Assumptions to be loaded
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<EntityDataAssumptionBaseAttribute> GetAssumptionsInternal()
        {
            return GetAssumptions();
        }
    }
}
