using System;
using DLaB.Common.Exceptions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm
{
    public enum ActiveAttributeType
    {
        None,
        IsDisabled,
        StateCode,
    }

    public class ActivePropertyInfo<T> where T : Entity
    {
        public ActiveAttributeType ActiveAttribute { get; set; }
        public string AttributeName { get; set; }
        public int? ActiveState { get; set; }
        public int? NotActiveState { get; set; }

        public ActivePropertyInfo()
        {
            if (typeof(T) == typeof(Entity))
            {
                throw new TypeArgumentException("'Entity' is an invalid type for T.  Please use the LateBoundActivePropertyInfo.");
            }
            SetAttributeNameAndType(EntityHelper.GetEntityLogicalName<T>());
        }

        protected ActivePropertyInfo(string logicalName)
        {
            if (logicalName == null)
            {
                throw new ArgumentNullException("logicalName");
            }
            SetAttributeNameAndType(logicalName);
        }

        private void SetAttributeNameAndType(string logicalName)
        {
            switch (logicalName)
            {
                case "businessunit":
                case "equipment":
                case "organization":
                case "resource":
                case "systemuser":
                    ActiveAttribute = ActiveAttributeType.IsDisabled;
                    AttributeName = "isdisabled";
                    break;

                #region Default CRM Entites with no active flag
                case "accountleads":
                case "activitymimeattachment":
                case "activityparty":
                case "annotation":
                case "annualfiscalcalendar":
                case "attributemap":
                case "audit":
                case "bulkdeletefailure":
                case "bulkoperationlog":
                case "businessunitnewsarticle":
                case "calendar":
                case "calendarrule":
                case "campaignactivityitem":
                case "campaignitem":
                case "competitor":
                case "competitorproduct":
                case "competitorsalesliterature":
                case "connectionroleassociation":
                case "connectionroleobjecttypecode":
                case "constraintbasedgroup":
                case "contactinvoices":
                case "contactleads":
                case "contactorders":
                case "contactquotes":
                case "contracttemplate":
                case "customeraddress":
                case "customeropportunityrole":
                case "customerrelationship":
                case "convertruleitem":
                case "dependency":
                case "discount":
                case "displaystring":
                case "duplicaterecord":
                case "duplicaterulecondition":
                case "dynamicpropertyassociation":
                case "dynamicpropertyinstance":
                case "dynamicpropertyoptionsetitem":
                case "entitlementchannel":
                case "entitlementcontacts":
                case "entitlementproducts":
                case "entitlementtemplate":
                case "entitlementtemplatechannel":
                case "entitlementtemplateproducts":
                case "entitymap":
                case "exchangesyncidmapping":
                case "fieldpermission":
                case "fieldsecurityprofile":
                case "fixedmonthlyfiscalcalendar":
                case "hierarchyrule":
                case "hierarchysecurityconfiguration":
                case "importjob":
                case "invaliddependency":
                case "invoicedetail":
                case "isvconfig":
                case "kbarticlecomment":
                case "kbarticletemplate":
                case "leadaddress":
                case "leadcompetitors":
                case "leadproduct":
                case "license":
                case "listmember":
                case "monthlyfiscalcalendar":
                case "opportunitycompetitors":
                case "opportunityproduct":
                case "organizationui":
                case "pluginassembly":
                case "plugintype":
                case "plugintypestatistic":
                case "processstage":
                case "processtrigger":
                case "post":
                case "postcomment":
                case "postfollow":
                case "postlike":
                case "principalentitymap":
                case "principalobjectattributeaccess":
                case "privilege":
                case "productassociation":
                case "productpricelevel":
                case "productsalesliterature":
                case "productsubstitute":
                case "publisher":
                case "publisheraddress":
                case "quarterlyfiscalcalendar":
                case "queuemembership":
                case "quotedetail":
                case "recurrencerule":
                case "relationshiprolemap":
                case "report":
                case "reportcategory":
                case "reportentity":
                case "reportlink":
                case "reportvisibility":
                case "resourcegroup":
                case "resourcespec":
                case "ribboncustomization":
                case "role":
                case "roleprivileges":
                case "roletemplateprivileges":
                case "rollupfield":
                case "routingruleitem":
                case "salesliterature":
                case "salesliteratureitem":
                case "salesorderdetail":
                case "savedqueryvisualization":
                case "sdkmessage":
                case "sdkmessagefilter":
                case "sdkmessagepair":
                case "sdkmessageprocessingstepimage":
                case "sdkmessageprocessingstepsecureconfig":
                case "sdkmessagerequest":
                case "sdkmessagerequestfield":
                case "sdkmessageresponse":
                case "sdkmessageresponsefield":
                case "semiannualfiscalcalendar":
                case "service":
                case "servicecontractcontacts":
                case "serviceendpoint":
                case "sharepointdata":
                case "sharepointdocument":
                case "site":
                case "sitemap":
                case "slaitem":
                case "slakpiinstance":
                case "socialinsightsconfiguration":
                case "solution":
                case "solutioncomponent":
                case "subject":
                case "subscriptiontrackingdeletedobject":
                case "subscriptionmanuallytrackedobject":
                case "systemform":
                case "systemuserlicenses":
                case "systemuserprofiles":
                case "systemuserroles":
                case "systemusersyncmappingprofiles":
                case "team":
                case "teammembership":
                case "teamprofiles":
                case "teamroles":
                case "teamsyncattributemappingprofiles":
                case "teamtemplate":
                case "template":
                case "territory":
                case "timezonedefinition":
                case "timezonelocalizedname":
                case "timezonerule":
                case "tracelog":
                case "transformationparametermapping":
                case "uom":
                case "userentityinstancedata":
                case "userentityuisettings":
                case "userform":
                case "userqueryvisualization":
                case "usersettings":
                case "webresource":
                case "workflowdependency":
                case "workflowlog":
                #endregion // Default CRM Entites with no active flag
                    ActiveAttribute = ActiveAttributeType.None;
                    break;

                default:
                    if (logicalName.Length > 4 && logicalName[3] == '_')
                    {
                        var prefix = logicalName.ToLower().Substring(0, 3);
                        if (logicalName.ToLower().Split(new [] { prefix }, StringSplitOptions.None).Length >= 3 || logicalName.ToLower().EndsWith("_association"))
                        {
                            // N:N Joins or association entities do not contain active flags
                            ActiveAttribute = ActiveAttributeType.None;
                            break;
                        }
                    }
                    SetStateAttributesAndValue(logicalName);
                    break;
            }
        }

        protected void SetStateAttributesAndValue(string logicalName)
        {
            ActiveAttribute = ActiveAttributeType.StateCode;
            AttributeName = "statecode";

            switch (logicalName)
            {
                // Entities with a Canceled State
                case "activitypointer":
                case "appointment":
                case "bulkoperation":
                case "campaignactivity":
                case "contractdetail":
                case "email":
                case "fax":
                case "letter":
                case "orderclose":
                case "phonecall":
                case "quoteclose":
                case "recurringappointmentmaster":
                case "serviceappointment":
                case "taskstate":
                    NotActiveState = 2;
                    break;

                case "duplicaterule": // don't ask me why, but this one is flipped
                    ActiveState = 1;
                    break;

                // Entities with states that can't be grouped into seperate all inclusive active and inactive states
                case "asyncoperation":
                case "bulkdeleteoperation":
                case "contract":
                case "lead":
                case "opportunity":
                case "processsession":
                case "quote":
                case "sdkmessageprocessingstep":
                case "workflow":
                    ActiveAttribute = ActiveAttributeType.None;
                    break;

                default:
                    if (IsJoinEntity(logicalName))
                    {
                        ActiveAttribute = ActiveAttributeType.None;
                    }
                    else
                    {
                        ActiveState = 0;
                    }
                    break;
            }
        }

        private bool IsJoinEntity(string logicalName)
        {
            // Entities of the type new_Foo_Bar are usually Join Entities that don't have a state
            return logicalName.Split('_').Length >= 3;
        }

        public static bool? IsActive(IOrganizationService service, Guid entityId)
        {
            var info = new ActivePropertyInfo<T>();
            var entity = service.GetEntity<T>(entityId, new ColumnSet(info.AttributeName));
            return IsActive(info, entity);
        }

        protected static bool? IsActive(ActivePropertyInfo<T> info, T entity)
        {
            bool? active;
            switch (info.ActiveAttribute)
            {
                case ActiveAttributeType.None:
                    // Unable to determine
                    active = null;
                    break;
                case ActiveAttributeType.IsDisabled:
                    active = !entity.GetAttributeValue<bool>(info.AttributeName);
                    break;
                case ActiveAttributeType.StateCode:
                    var state = entity.GetAttributeValue<OptionSetValue>(info.AttributeName).Value;
                    if (info.ActiveState.HasValue)
                    {
                        active = state == info.ActiveState;
                    }
                    else if (info.NotActiveState.HasValue)
                    {
                        active = state != info.NotActiveState;
                    }
                    else
                    {
                        throw new Exception("ActivePropertyInfo defines Attribute StateCode, but neither ActiveState or NotActiveState is popualted");
                    }
                    break;
                default:
                    throw new EnumCaseUndefinedException<ActiveAttributeType>(info.ActiveAttribute);
            }

            return active;
        }
    }
}
