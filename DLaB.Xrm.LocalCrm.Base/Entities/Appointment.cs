using System;
using System.Collections.Generic;
using System.Text;

namespace DLaB.Xrm.LocalCrm.Entities
{
    class Appointment
    {
        		public static class Fields
		{
			public const string ActivityAdditionalParams = "activityadditionalparams";
			public const string ActivityId = "activityid";
			public const string Id = "activityid";
			public const string ActivityTypeCode = "activitytypecode";
			public const string ActualDurationMinutes = "actualdurationminutes";
			public const string ActualEnd = "actualend";
			public const string ActualStart = "actualstart";
			public const string AttachmentCount = "attachmentcount";
			public const string AttachmentErrors = "attachmenterrors";
			public const string Category = "category";
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string Description = "description";
			public const string ExchangeRate = "exchangerate";
			public const string GlobalObjectId = "globalobjectid";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string InstanceTypeCode = "instancetypecode";
			public const string IsAllDayEvent = "isalldayevent";
			public const string IsBilled = "isbilled";
			public const string IsDraft = "isdraft";
			public const string IsMapiPrivate = "ismapiprivate";
			public const string IsRegularActivity = "isregularactivity";
			public const string IsUnsafe = "isunsafe";
			public const string IsWorkflowCreated = "isworkflowcreated";
			public const string LastOnHoldTime = "lastonholdtime";
			public const string Location = "location";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedFieldsMask = "modifiedfieldsmask";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string OnHoldTime = "onholdtime";
			public const string OptionalAttendees = "optionalattendees";
			public const string Organizer = "organizer";
			public const string OriginalStartDate = "originalstartdate";
			public const string OutlookOwnerApptId = "outlookownerapptid";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string OwnerId = "ownerid";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string PriorityCode = "prioritycode";
			public const string ProcessId = "processid";
			public const string RegardingObjectId = "regardingobjectid";
			public const string RequiredAttendees = "requiredattendees";
			public const string ScheduledDurationMinutes = "scheduleddurationminutes";
			public const string ScheduledEnd = "scheduledend";
			public const string ScheduledStart = "scheduledstart";
			public const string SeriesId = "seriesid";
			public const string ServiceId = "serviceid";
			public const string SLAId = "slaid";
			public const string SLAInvokedId = "slainvokedid";
			public const string SortDate = "sortdate";
			public const string StageId = "stageid";
			public const string StateCode = "statecode";
			public const string StatusCode = "statuscode";
			public const string Subcategory = "subcategory";
			public const string Subject = "subject";
			public const string SubscriptionId = "subscriptionid";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string TransactionCurrencyId = "transactioncurrencyid";
			public const string TraversedPath = "traversedpath";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
			public const string Account_Appointments = "Account_Appointments";
			public const string activity_pointer_appointment = "activity_pointer_appointment";
            public const string business_unit_appointment_activities = "business_unit_appointment_activities";
			public const string Campaign_Appointments = "Campaign_Appointments";
			public const string Contact_Appointments = "Contact_Appointments";
			public const string Contract_Appointments = "Contract_Appointments";
            public const string Incident_Appointments = "Incident_Appointments";
			public const string Lead_Appointments = "Lead_Appointments";
			public const string lk_appointment_createdby = "lk_appointment_createdby";
			public const string lk_appointment_createdonbehalfby = "lk_appointment_createdonbehalfby";
			public const string lk_appointment_modifiedby = "lk_appointment_modifiedby";
			public const string lk_appointment_modifiedonbehalfby = "lk_appointment_modifiedonbehalfby";
            public const string Opportunity_Appointments = "Opportunity_Appointments";
			public const string processstage_appointments = "processstage_appointments";
			public const string Quote_Appointments = "Quote_Appointments";
			public const string SalesOrder_Appointments = "SalesOrder_Appointments";
			public const string site_Appointments = "site_Appointments";
			public const string sla_appointment = "sla_appointment";
			public const string team_appointment = "team_appointment";
			public const string TransactionCurrency_Appointment = "TransactionCurrency_Appointment";
			public const string user_appointment = "user_appointment";
		}
    }
}
