
using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Template
    {
        public struct Fields
        {
            public const string Body = "body";
            public const string ComponentState = "componentstate";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string Description = "description";
            public const string GenerationTypeCode = "generationtypecode";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string IntroducedVersion = "introducedversion";
            public const string IsCustomizable = "iscustomizable";
            public const string IsManaged = "ismanaged";
            public const string IsPersonal = "ispersonal";
            public const string LanguageCode = "languagecode";
            public const string MimeType = "mimetype";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string OverwriteTime = "overwritetime";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningTeam = "owningteam";
            public const string OwningUser = "owninguser";
            public const string PresentationXml = "presentationxml";
            public const string SolutionId = "solutionid";
            public const string Subject = "subject";
            public const string SubjectPresentationXml = "subjectpresentationxml";
            public const string TemplateId = "templateid";
            public const string Id = "templateid";
            public const string TemplateIdUnique = "templateidunique";
            public const string TemplateTypeCode = "templatetypecode";
            public const string Title = "title";
            public const string VersionNumber = "versionnumber";
            public const string business_unit_templates = "owningbusinessunit";
            public const string lk_templatebase_createdby = "createdby";
            public const string lk_templatebase_createdonbehalfby = "createdonbehalfby";
            public const string lk_templatebase_modifiedby = "modifiedby";
            public const string lk_templatebase_modifiedonbehalfby = "modifiedonbehalfby";
            public const string system_user_email_templates = "owninguser";
            public const string team_email_templates = "owningteam";
        }

        public const string EntityLogicalName = "template";
    }
}
