using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Email
    {
        public struct Fields
        {
            public const string ActivityId = "activityid";
            public const string Id = "activityid";
            public const string ActivityTypeCode = "activitytypecode";
            public const string ActualDurationMinutes = "actualdurationminutes";
            public const string ActualEnd = "actualend";
            public const string ActualStart = "actualstart";
            public const string AttachmentCount = "attachmentcount";
            public const string Bcc = "bcc";
            public const string Category = "category";
            public const string Cc = "cc";
            public const string Compressed = "compressed";
            public const string ConversationIndex = "conversationindex";
            public const string CorrelationMethod = "correlationmethod";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string DeliveryAttempts = "deliveryattempts";
            public const string DeliveryPriorityCode = "deliveryprioritycode";
            public const string DeliveryReceiptRequested = "deliveryreceiptrequested";
            public const string Description = "description";
            public const string DirectionCode = "directioncode";
            public const string EmailSender = "emailsender";
            public const string ExchangeRate = "exchangerate";
            public const string From = "from";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string InReplyTo = "inreplyto";
            public const string IsBilled = "isbilled";
            public const string IsRegularActivity = "isregularactivity";
            public const string IsWorkflowCreated = "isworkflowcreated";
            public const string MessageId = "messageid";
            public const string MessageIdDupCheck = "messageiddupcheck";
            public const string MimeType = "mimetype";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string Notifications = "notifications";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningTeam = "owningteam";
            public const string OwningUser = "owninguser";
            public const string ParentActivityId = "parentactivityid";
            public const string PostponeEmailProcessingUntil = "postponeemailprocessinguntil";
            public const string PriorityCode = "prioritycode";
            public const string ProcessId = "processid";
            public const string ReadReceiptRequested = "readreceiptrequested";
            public const string RegardingObjectId = "regardingobjectid";
            public const string ScheduledDurationMinutes = "scheduleddurationminutes";
            public const string ScheduledEnd = "scheduledend";
            public const string ScheduledStart = "scheduledstart";
            public const string Sender = "sender";
            public const string SenderMailboxId = "sendermailboxid";
            public const string SendersAccount = "sendersaccount";
            public const string SentOn = "senton";
            public const string ServiceId = "serviceid";
            public const string StageId = "stageid";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
            public const string Subcategory = "subcategory";
            public const string Subject = "subject";
            public const string SubmittedBy = "submittedby";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string To = "to";
            public const string ToRecipients = "torecipients";
            public const string TrackingToken = "trackingtoken";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string TraversedPath = "traversedpath";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
            public const string Account_Email_EmailSender = "emailsender";
            public const string Account_Email_SendersAccount = "sendersaccount";
            public const string Account_Emails = "regardingobjectid";
            public const string activity_pointer_email = "activityid";
            public const string AsyncOperation_Emails = "regardingobjectid";
            public const string BulkOperation_Email = "regardingobjectid";
            public const string business_unit_email_activities = "owningbusinessunit";
            public const string Campaign_Emails = "regardingobjectid";
            public const string CampaignActivity_Emails = "regardingobjectid";
            public const string Contact_Email_EmailSender = "emailsender";
            public const string Contact_Emails = "regardingobjectid";
            public const string Contract_Emails = "regardingobjectid";
            public const string Referencingemail_email_parentactivityid = "parentactivityid";
            public const string email_sendermailboxid_mailbox = "sendermailboxid";
            public const string entitlement_Emails = "regardingobjectid";
            public const string entitlementtemplate_Emails = "regardingobjectid";
            public const string Equipment_Email_EmailSender = "emailsender";
            public const string Incident_Emails = "regardingobjectid";
            public const string Invoice_Emails = "regardingobjectid";
            public const string Lead_Email_EmailSender = "emailsender";
            public const string Lead_Emails = "regardingobjectid";
            public const string lk_email_createdby = "createdby";
            public const string lk_email_createdonbehalfby = "createdonbehalfby";
            public const string lk_email_modifiedby = "modifiedby";
            public const string lk_email_modifiedonbehalfby = "modifiedonbehalfby";
            public const string msdyn_postalbum_Emails = "regardingobjectid";
            public const string Opportunity_Emails = "regardingobjectid";
            public const string processstage_emails = "stageid";
            public const string Queue_Email_EmailSender = "emailsender";
            public const string Quote_Emails = "regardingobjectid";
            public const string SalesOrder_Emails = "regardingobjectid";
            public const string service_emails = "serviceid";
            public const string SystemUser_Email_EmailSender = "emailsender";
            public const string team_email = "owningteam";
            public const string TransactionCurrency_Email = "transactioncurrencyid";
            public const string user_email = "owninguser";
        }

        public const string EntityLogicalName = "email";
    }
}
