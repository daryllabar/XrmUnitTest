using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm;
#if NET
using DataverseUnitTest.Assumptions;
#else
using DLaB.Xrm.Test.Assumptions;
#endif

namespace XrmUnitTest.Test.Assumptions
{
    // [PrerequisiteAssumptions(typeof(ContactDefault))]
    public class AccountDefault : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<AccountDefault, Account>
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            // var contact = Assumptions.Get<ContactDefault>().Id;
            // Could be used to get a contact for the account.
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
