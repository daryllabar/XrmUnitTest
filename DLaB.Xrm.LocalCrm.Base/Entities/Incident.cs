using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    internal enum IncidentState
    {

        [System.Runtime.Serialization.EnumMember]
        Active = 0,

        [System.Runtime.Serialization.EnumMember]
        Resolved = 1,

        [System.Runtime.Serialization.EnumMember]
        Canceled = 2,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Incident
    {
        public struct Fields
        {
            public const string AccountId = "accountid";
            public const string ActivitiesComplete = "activitiescomplete";
            public const string ActualServiceUnits = "actualserviceunits";
            public const string BilledServiceUnits = "billedserviceunits";
            public const string BlockedProfile = "blockedprofile";
            public const string CaseOriginCode = "caseorigincode";
            public const string CaseTypeCode = "casetypecode";
            public const string CheckEmail = "checkemail";
            public const string ContactId = "contactid";
            public const string ContractDetailId = "contractdetailid";
            public const string ContractId = "contractid";
            public const string ContractServiceLevelCode = "contractservicelevelcode";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string CustomerContacted = "customercontacted";
            public const string CustomerId = "customerid";
            public const string CustomerSatisfactionCode = "customersatisfactioncode";
            public const string DecrementEntitlementTerm = "decremententitlementterm";
            public const string Description = "description";
            public const string EntitlementId = "entitlementid";
            public const string EntityImage = "entityimage";
            public const string EntityImage_Timestamp = "entityimage_timestamp";
            public const string EntityImage_URL = "entityimage_url";
            public const string EntityImageId = "entityimageid";
            public const string EscalatedOn = "escalatedon";
            public const string ExchangeRate = "exchangerate";
            public const string ExistingCase = "existingcase";
            public const string FirstResponseByKPIId = "firstresponsebykpiid";
            public const string FirstResponseSent = "firstresponsesent";
            public const string FirstResponseSLAStatus = "firstresponseslastatus";
            public const string FollowupBy = "followupby";
            public const string FollowUpTaskCreated = "followuptaskcreated";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string IncidentId = "incidentid";
            public const string Id = "incidentid";
            public const string IncidentStageCode = "incidentstagecode";
            public const string InfluenceScore = "influencescore";
            public const string IsDecrementing = "isdecrementing";
            public const string IsEscalated = "isescalated";
            public const string KbArticleId = "kbarticleid";
            public const string LastOnHoldTime = "lastonholdtime";
            public const string MasterId = "masterid";
            public const string Merged = "merged";
            public const string MessageTypeCode = "messagetypecode";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string NumberOfChildIncidents = "numberofchildincidents";
            public const string OnHoldTime = "onholdtime";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningTeam = "owningteam";
            public const string OwningUser = "owninguser";
            public const string ParentCaseId = "parentcaseid";
            public const string PrimaryContactId = "primarycontactid";
            public const string PriorityCode = "prioritycode";
            public const string ProcessId = "processid";
            public const string ProductId = "productid";
            public const string ProductSerialNumber = "productserialnumber";
            public const string ResolveBy = "resolveby";
            public const string ResolveByKPIId = "resolvebykpiid";
            public const string ResolveBySLAStatus = "resolvebyslastatus";
            public const string ResponseBy = "responseby";
            public const string ResponsibleContactId = "responsiblecontactid";
            public const string RouteCase = "routecase";
            public const string SentimentValue = "sentimentvalue";
            public const string ServiceStage = "servicestage";
            public const string SeverityCode = "severitycode";
            public const string SLAInvokedId = "slainvokedid";
            public const string SocialProfileId = "socialprofileid";
            public const string StageId = "stageid";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
            public const string SubjectId = "subjectid";
            public const string TicketNumber = "ticketnumber";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string Title = "title";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string TraversedPath = "traversedpath";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
            public const string business_unit_incidents = "owningbusinessunit";
            public const string contact_as_primary_contact = "primarycontactid";
            public const string contact_as_responsible_contact = "responsiblecontactid";
            public const string contract_cases = "contractid";
            public const string contract_detail_cases = "contractdetailid";
            public const string entitlement_cases = "entitlementid";
            public const string incident_customer_accounts = "customerid";
            public const string incident_customer_contacts = "customerid";
            public const string Referencingincident_existingcase = "existingcase";
            public const string Referencingincident_master_incident = "masterid";
            public const string Referencingincident_parent_incident = "parentcaseid";
            public const string kbarticle_incidents = "kbarticleid";
            public const string lk_incidentbase_createdby = "createdby";
            public const string lk_incidentbase_createdonbehalfby = "createdonbehalfby";
            public const string lk_incidentbase_modifiedby = "modifiedby";
            public const string lk_incidentbase_modifiedonbehalfby = "modifiedonbehalfby";
            public const string processstage_incident = "stageid";
            public const string product_incidents = "productid";
            public const string sla_cases = "slainvokedid";
            public const string slakpiinstance_incident_firstresponsebykpi = "firstresponsebykpiid";
            public const string slakpiinstance_incident_resolvebykpi = "resolvebykpiid";
            public const string socialprofile_cases = "socialprofileid";
            public const string subject_incidents = "subjectid";
            public const string system_user_incidents = "owninguser";
            public const string team_incidents = "owningteam";
            public const string TransactionCurrency_Incident = "transactioncurrencyid";
        }

        public const string EntityLogicalName = "incident";
    }
}
