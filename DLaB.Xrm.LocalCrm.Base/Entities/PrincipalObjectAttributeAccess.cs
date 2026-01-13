using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class PrincipalObjectAccess
    {
        public static class Fields
        {
            public const string AccessRightsMask = "accessrightsmask";
            public const string ChangedOn = "changedon";
            public const string InheritedAccessRightsMask = "inheritedaccessrightsmask";
            public const string ObjectId = "objectid";
            public const string ObjectTypeCode = "objecttypecode";
            public const string ObjectTypeCodename = "objecttypecodename";
            public const string PrincipalId = "principalid";
            public const string PrincipalObjectAccessId = "principalobjectaccessid";
            public const string Id = "principalobjectaccessid";
            public const string PrincipalTypeCode = "principaltypecode";
            public const string PrincipalTypeCodename = "principaltypecodename";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string UtcConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
        }

        public const string EntityLogicalName = "principalobjectaccess";
    }
}
