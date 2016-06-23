using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ActivityPointer
    {
        public struct Fields
        {
            public const string ActivityId = "activityid";
            public const string Id = "activityid";
            public const string ActivityTypeCode = "activitytypecode";
            public const string ActualDurationMinutes = "actualdurationminutes";
            public const string ActualEnd = "actualend";
            public const string ActualStart = "actualstart";
            public const string allparties = "allparties";
            public const string Community = "community";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string DeliveryLastAttemptedOn = "deliverylastattemptedon";
            public const string DeliveryPriorityCode = "deliveryprioritycode";
            public const string Description = "description";
            public const string ExchangeRate = "exchangerate";
            public const string InstanceTypeCode = "instancetypecode";
            public const string IsBilled = "isbilled";
            public const string IsMapiPrivate = "ismapiprivate";
            public const string IsRegularActivity = "isregularactivity";
            public const string IsWorkflowCreated = "isworkflowcreated";
            public const string LeftVoiceMail = "leftvoicemail";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningTeam = "owningteam";
            public const string OwningUser = "owninguser";
            public const string PostponeActivityProcessingUntil = "postponeactivityprocessinguntil";
            public const string PriorityCode = "prioritycode";
            public const string ProcessId = "processid";
            public const string RegardingObjectId = "regardingobjectid";
            public const string ScheduledDurationMinutes = "scheduleddurationminutes";
            public const string ScheduledEnd = "scheduledend";
            public const string ScheduledStart = "scheduledstart";
            public const string SenderMailboxId = "sendermailboxid";
            public const string SentOn = "senton";
            public const string SeriesId = "seriesid";
            public const string ServiceId = "serviceid";
            public const string StageId = "stageid";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
            public const string Subject = "subject";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string TraversedPath = "traversedpath";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
            public const string Account_ActivityPointers = "regardingobjectid";
            public const string activitypointer_sendermailboxid_mailbox = "sendermailboxid";
            public const string BulkOperation_ActivityPointers = "regardingobjectid";
            public const string business_unit_activitypointer = "owningbusinessunit";
            public const string Campaign_ActivityPointers = "regardingobjectid";
            public const string CampaignActivity_ActivityPointers = "regardingobjectid";
            public const string Contact_ActivityPointers = "regardingobjectid";
            public const string Contract_ActivityPointers = "regardingobjectid";
            public const string entitlement_ActivityPointers = "regardingobjectid";
            public const string entitlementtemplate_ActivityPointers = "regardingobjectid";
            public const string Incident_ActivityPointers = "regardingobjectid";
            public const string Invoice_ActivityPointers = "regardingobjectid";
            public const string Lead_ActivityPointers = "regardingobjectid";
            public const string lk_activitypointer_createdby = "createdby";
            public const string lk_activitypointer_createdonbehalfby = "createdonbehalfby";
            public const string lk_activitypointer_modifiedby = "modifiedby";
            public const string lk_activitypointer_modifiedonbehalfby = "modifiedonbehalfby";
            public const string msdyn_postalbum_ActivityPointers = "regardingobjectid";
            public const string Opportunity_ActivityPointers = "regardingobjectid";
            public const string Quote_ActivityPointers = "regardingobjectid";
            public const string SalesOrder_ActivityPointers = "regardingobjectid";
            public const string service_activity_pointers = "serviceid";
            public const string team_activity = "owningteam";
            public const string TransactionCurrency_ActivityPointer = "transactioncurrencyid";
            public const string user_activity = "owninguser";
        }

        public const string EntityLogicalName = "activitypointer";
    }
}
