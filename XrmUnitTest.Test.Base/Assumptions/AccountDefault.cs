using DLaB.Xrm.Entities;
using DLaB.Xrm.Test.Assumptions;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm;

namespace XrmUnitTest.Test.Assumptions
{
    public class AccountDefault : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<AccountDefault, Account>
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            return service.GetFirstOrDefault<Account>(a => new
            {
                a.Id,
                a.Name,
                a.PrimaryContactId
            },
            Account.Fields.Name, "Default Account");
        }
    }
}
