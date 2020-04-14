using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Team
    {
        public struct Fields
        {
            public const string AdministratorId = "administratorid";
            public const string AzureActiveDirectoryObjectId = "azureactivedirectoryobjectid";
            public const string BusinessUnitId = "businessunitid";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string Description = "description";
            public const string EMailAddress = "emailaddress";
            public const string ExchangeRate = "exchangerate";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string IsDefault = "isdefault";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string Name = "name";
            public const string OrganizationId = "organizationid";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string ProcessId = "processid";
            public const string QueueId = "queueid";
            public const string RegardingObjectId = "regardingobjectid";
            public const string StageId = "stageid";
            public const string SystemManaged = "systemmanaged";
            public const string TeamId = "teamid";
            public const string Id = "teamid";
            public const string TeamTemplateId = "teamtemplateid";
            public const string TeamType = "teamtype";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string TraversedPath = "traversedpath";
            public const string VersionNumber = "versionnumber";
            public const string YomiName = "yominame";
		}

        public const string EntityLogicalName = "team";
    }
}
