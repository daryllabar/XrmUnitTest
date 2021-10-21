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
    public class ContactDefault : EntityDataAssumptionBaseAttribute, IAssumptionEntityType<ContactDefault, Contact>
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            return service.GetFirstOrDefault<Contact>(c => new {c.FirstName, c.LastName },
                Contact.Fields.FirstName, "Default", 
                Contact.Fields.LastName, "Contact");
        }
    }
}
