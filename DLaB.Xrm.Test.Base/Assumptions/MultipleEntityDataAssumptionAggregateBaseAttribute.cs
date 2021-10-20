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
    public abstract class MultipleEntityDataAssumptionAggregateBaseAttribute : MultipleEntityDataAssumptionBaseAttribute
    {
        private List<EntityDataAssumptionBaseAttribute> InternalAssumptions { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        protected MultipleEntityDataAssumptionAggregateBaseAttribute()
        {
            InternalAssumptions = new List<EntityDataAssumptionBaseAttribute>();
        }

        /// <summary>
        /// Adds all assumptions defined for the given attributes to the current value.  Useful for combining multiple assumptions.
        /// </summary>
        /// <param name="attributes"></param>
        public void Add(params MultipleEntityDataAssumptionBaseAttribute[] attributes)
        {
            foreach (var attribute in attributes)
            {
                InternalAssumptions.AddRange(attribute.GetAssumptionsInternal());
            }
        }

        /// <summary>
        /// Adds all assumptions defined for the given attributes to the current value.  Useful for combining multiple assumptions.
        /// </summary>
        /// <param name="attributes"></param>
        public void Add(params EntityDataAssumptionBaseAttribute[] attributes)
        {
            InternalAssumptions.AddRange(attributes);
        }

        /// <summary>
        /// Gets the assumptions.
        /// </summary>
        /// <returns></returns>
        protected sealed override IEnumerable<EntityDataAssumptionBaseAttribute> GetAssumptions()
        {
            return InternalAssumptions;
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
    }
}
