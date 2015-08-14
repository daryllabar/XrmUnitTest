using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    public abstract class MultipleEntityDataAssumptionBaseAttribute : EntityDataAssumptionBaseAttribute
    {
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
                AddAssumedEntity(service, assumption);
            }
        }
    }
}
