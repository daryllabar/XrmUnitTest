using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Test.Entities
{
    internal class WebResource
    {
        public struct Fields
        {
            public const string CanBeDeleted = "canbedeleted";
            public const string ComponentState = "componentstate";
            public const string Content = "content";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string Description = "description";
            public const string DisplayName = "displayname";
            public const string IntroducedVersion = "introducedversion";
            public const string IsCustomizable = "iscustomizable";
            public const string IsEnabledForMobileClient = "isenabledformobileclient";
            public const string IsHidden = "ishidden";
            public const string IsManaged = "ismanaged";
            public const string LanguageCode = "languagecode";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string Name = "name";
            public const string OrganizationId = "organizationid";
            public const string OverwriteTime = "overwritetime";
            public const string SilverlightVersion = "silverlightversion";
            public const string SolutionId = "solutionid";
            public const string VersionNumber = "versionnumber";
            public const string WebResourceId = "webresourceid";
            public const string Id = "webresourceid";
            public const string WebResourceIdUnique = "webresourceidunique";
            public const string WebResourceType = "webresourcetype";
            public const string lk_webresourcebase_createdonbehalfby = "createdonbehalfby";
            public const string lk_webresourcebase_modifiedonbehalfby = "modifiedonbehalfby";
            public const string webresource_createdby = "createdby";
            public const string webresource_modifiedby = "modifiedby";
            public const string webresource_organization = "organizationid";
        }

        public const string EntityLogicalName = "webresource";
    }
}
