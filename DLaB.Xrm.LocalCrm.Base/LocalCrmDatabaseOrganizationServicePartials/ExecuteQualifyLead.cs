using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

#if !PRE_MULTISELECT
namespace DLaB.Xrm.LocalCrm
{
    partial class LocalCrmDatabaseOrganizationService
    {
        private QualifyLeadResponse ExecuteInternal(QualifyLeadRequest request)
        {
            if (request.LeadId == null)
            {
                throw new Exception("Lead Id must be set in request.");
            }
            if (request.Status == null)
            {
                throw new Exception("A required field 'Status' is missing'");
            }
            var lead = Retrieve(request.LeadId.LogicalName, request.LeadId.Id, new ColumnSet(true));

            var createdEntities = new EntityReferenceCollection();
            var response = new QualifyLeadResponse
            {
                ["CreatedEntities"] = createdEntities
            };
            
            Entity account = lead.GetAttributeValue<EntityReference>("accountid") == null ? null : new Entity("accountid") { Id = lead.GetAttributeValue<EntityReference>("accountid").Id };
            Entity contact = lead.GetAttributeValue<EntityReference>("contactid") == null ? null : new Entity("contact") { Id = lead.GetAttributeValue<EntityReference>("contactid").Id };

            // Create Account
            if (request.CreateAccount) // ParentAccount
            {
                account = new Entity("account")
                {
                    Attributes =
                    {
                        ["originatingleadid"] = request.LeadId,
                        ["name"] = lead.GetAttributeValue<object>("companyname"),
                        ["websiteurl"] = lead.GetAttributeValue<object>("websiteurl"),
                        ["telephone1"] = lead.GetAttributeValue<object>("address1_telephone1"),
                        ["address1_line1"] = lead.GetAttributeValue<object>("address1_line1"),
                        ["address1_line2"] = lead.GetAttributeValue<object>("address1_line2"),
                        ["address1_line3"] = lead.GetAttributeValue<object>("address1_line3"),
                        ["address1_city"] = lead.GetAttributeValue<object>("address1_city"),
                        ["address1_stateorprovince"] = lead.GetAttributeValue<object>("address1_stateorprovince"),
                        ["address1_postalcode"] = lead.GetAttributeValue<object>("address1_postalcode"),
                        ["address1_country"] = lead.GetAttributeValue<object>("address1_country"),
                    }
                };
                account.Id = Create(account);
                createdEntities.Add(account.ToEntityReference());
            }

            // Create Contact
            if (request.CreateContact)
            {
                contact = new Entity("contact")
                {
                    ["originatingleadid"] = request.LeadId,
                    ["firstname"] = lead.GetAttributeValue<object>("firstname"),
                    ["lastname"] = lead.GetAttributeValue<object>("lastname"),
                    ["jobtitle"] = lead.GetAttributeValue<object>("jobtitle"),
                    ["telephone1"] = lead.GetAttributeValue<object>("telephone1"),
                    ["mobilephone"] = lead.GetAttributeValue<object>("mobilephone"),
                    ["address1_line1"] = lead.GetAttributeValue<object>("address1_line1"),
                    ["address1_line2"] = lead.GetAttributeValue<object>("address1_line2"),
                    ["address1_line3"] = lead.GetAttributeValue<object>("address1_line3"),
                    ["address1_city"] = lead.GetAttributeValue<object>("address1_city"),
                    ["address1_stateorprovince"] = lead.GetAttributeValue<object>("address1_stateorprovince"),
                    ["address1_postalcode"] = lead.GetAttributeValue<object>("address1_postalcode"),
                    ["address1_country"] = lead.GetAttributeValue<object>("address1_country"),
                };
                contact.Id = Create(contact);
                createdEntities.Add(contact.ToEntityReference());
            }

            // Create Opportunity
            if (request.CreateOpportunity)
            {
                var opportunity = new Entity("opportunity")
                {
                    ["originatingleadid"] = request.LeadId,
                    ["transactioncurrencyid"] = request.OpportunityCurrencyId,
                    ["name"] = lead.GetAttributeValue<object>("subject"),  // https://learn.microsoft.com/en-us/dynamics365/sales/define-lead-qualification-experience#field-mappings-to-other-entities Says Topic.  There is no Topic?
                };

                // Associate Account or Contact with Opportunity
                // MSDN link:
                // https://msdn.microsoft.com/en-us/library/microsoft.crm.sdk.messages.qualifyleadrequest.opportunitycustomerid.aspx
                if (request.OpportunityCustomerId != null)
                {
                    var logicalName = request.OpportunityCustomerId.LogicalName;

                    // Associate Account or Contact
                    if (logicalName.Equals("account") || logicalName.Equals("contact"))
                    {
                        opportunity.Attributes["customerid"] = request.OpportunityCustomerId;
                    }
                    // Wrong Entity was given as parameter
                    else
                    {
                        throw new Exception(string.Format("Opportunity Customer Id should be connected with Account or Contact. Instead OpportunityCustomerId was given with Entity.LogicalName = {0}", logicalName));
                    }
                }

                if (account != null)
                {
                    opportunity["accountid"] = account.ToEntityReference();
                }

                if (contact != null)
                {
                    opportunity["contactid"] = contact.ToEntityReference();
                }

                opportunity.Id = Create(opportunity);
                createdEntities.Add(opportunity.ToEntityReference());
            }

            lead.Attributes["statuscode"] = new OptionSetValue(request.Status.Value);
            lead.Attributes["statecode"] = new OptionSetValue(1);
            Update(lead);

            return response;
        }
    }
}
#endif
