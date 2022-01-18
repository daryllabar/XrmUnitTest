using DLaB.Xrm;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

#if NET
using DataverseUnitTest.Assumptions;
#else
using DLaB.Xrm.Test.Assumptions;
#endif

namespace XrmUnitTest.Test.Assumptions
{
    public class ConnectionRole2 : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<ConnectionRole2, ConnectionRole>
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            // var contact = Assumptions.Get<ContactDefault>().Id;
            // Could be used to get a contact for the account.
            return service.GetFirstOrDefault<ConnectionRole>(cr => new
                {
                    cr.Id,
                    cr.Category,
                    cr.Name,
                },
                ConnectionRole.Fields.Category, 2);
        }
    }
}