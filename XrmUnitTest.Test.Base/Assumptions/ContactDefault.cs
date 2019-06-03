using DLaB.Xrm.Entities;
using DLaB.Xrm.Test.Assumptions;
using Microsoft.Xrm.Sdk;
using DLaB.Xrm;

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
