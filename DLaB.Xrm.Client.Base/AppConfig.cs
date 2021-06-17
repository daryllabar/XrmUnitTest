using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DLaB.Common;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Single place for all config read settings to be performed
    /// </summary>
    public class AppConfig
    {
        private static string _connectionString;

        /// <summary>
        /// The Connection string used to access CRM
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (_connectionString != null)
                {
                    return _connectionString;
                }

                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = Config.GetAppSettingOrDefault(ConnectionPrefix + "ConnectionString", string.Empty)
                };
                builder.Add("Password", Password);

                return builder.ConnectionString;
            }
            set => _connectionString = value;
        }

        private static string _connectionPrefix;

        /// <summary>
        /// The Connection Prefix to use to determine the Connection String to use to connect.
        /// </summary>
        public static string ConnectionPrefix => _connectionPrefix ?? (_connectionPrefix = Config.GetAppSettingOrDefault("ConnectionPrefix", string.Empty));

        private static int? _defaultLanguageCode;

        /// <summary>
        /// Gets or sets the default language code.
        /// </summary>
        /// <value>
        /// The default language code.
        /// </value>
        public static int DefaultLanguageCode
        {
            get => GetValue(ref _defaultLanguageCode, "DefaultLanguageCode", 1033);
            set => _defaultLanguageCode = value;
        }

        private static string _password;

        /// <summary>
        /// Password for the Debug User Account
        /// </summary>
        /// <value>
        /// The debug user account password.
        /// </value>
        public static string Password
        {
            get => _password ?? (_password = Config.GetAppSettingOrDefault(ConnectionPrefix + "Password",
                       Config.GetAppSettingOrDefault("Password", string.Empty)));
            set => _password = value;
        }

        /// <summary>
        /// CrmEntities Settings
        /// </summary>
        public class CrmEntities
        {
            private static string _contextType;
            private static string _primaryNameAttributeName;
            private static bool? _containsPrimaryAttributeName;
            private static Dictionary<string, string> _nonStandardAttributeNamesByEntity;
            private static Dictionary<string, Many2ManyRelationshipDefinition> _many2ManyAssociationsBySchemaName;
            private static List<string> _namelessEntities;

            /// <summary>
            /// The type of the crm context definition.  This is used to determine the assembly of the early bound entities
            /// </summary>
            /// <value>
            /// The type of the context.
            /// </value>
            public static string ContextType
            {
                
                get => _contextType ?? (_contextType = Config.GetAppSettingOrDefault("DLaB.Xrm.Entities.CrmContext, DLaB.Xrm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", string.Empty));
                set => _contextType = value;
            }

            /// <summary>
            /// Determines if the PrimaryNameViaFieldProvider is used (if true or not provided) or PrimaryNameViaNonStandardNamesProvider (if false)
            /// </summary>
            public static bool ContainPrimaryAttributeName
            {
                get
                {
                    if (!_containsPrimaryAttributeName.HasValue)
                    {
                        _containsPrimaryAttributeName = Config.GetAppSettingOrDefault("CrmEntities.TypesContainPrimaryAttributeName", true);
                    }

                    return _containsPrimaryAttributeName.Value;
                }
                set => _containsPrimaryAttributeName = value;
            }

            /// <summary>
            /// Ignored if EarlyBoundTypesContainPrimaryAttributeName is false
            /// </summary>
            public static string PrimaryNameAttributeName
            {
                get => _primaryNameAttributeName ?? (_primaryNameAttributeName = Config.GetAppSettingOrDefault("CrmEntities.PrimaryNameAttributeName", "PrimaryNameAttribute"));
                set => _primaryNameAttributeName = value;
            }

            /// <summary>
            /// Ignored if EarlyBoundTypesContainPrimaryAttributeName is true
            /// </summary>
            public static Dictionary<string,string> NonStandardAttributeNamesByEntity
            {
                get => _nonStandardAttributeNamesByEntity ?? (_nonStandardAttributeNamesByEntity = Config.GetAppSettingOrDefault("CrmEntities.NonStandardAttributeNamesByEntity", "").ToLower().GetDictionary<string, string>());
                set => _nonStandardAttributeNamesByEntity = value;
            }

            /// <summary>
            /// List of Entities that do not have a primary name attribute, in addition to the known entities
            /// </summary>
            public static List<string> NamelessEntities
            {
                get => _namelessEntities ?? (_namelessEntities = Config.GetList("CrmEntities.NamelessEntities", new List<string>()));
                set => _namelessEntities = value;
            }

            /// <summary>
            /// {RelationshipSchemaName1}:{RelationshipEntityLogicalName1},{PrimaryEntityLogicalName1},{PrimaryEntityIdName1},{AssociatedEntityIdName1}
            /// |{RelationshipSchemaName2}:{RelationshipEntityLogicalName2},{PrimaryEntityLogicalName2},{PrimaryEntityIdName2},{AssociatedEntityIdName2}
            /// </summary>
            public static Dictionary<string, Many2ManyRelationshipDefinition> Many2ManyAssociationDefinitions
            {
                get => _many2ManyAssociationsBySchemaName
                    ?? (_many2ManyAssociationsBySchemaName = GetMany2ManyAssociationDefinitionsConfigWithDefaults());
                set => _many2ManyAssociationsBySchemaName = value;
            }
        }

        private static Dictionary<string, Many2ManyRelationshipDefinition> GetMany2ManyAssociationDefinitionsConfigWithDefaults()
        {
            var dictionary = Config.GetDictionaryList("CrmEntities.Many2ManyAssociationDefinitions",
                      new Dictionary<string, List<string>>())
                  .ToDictionary(k => k.Key, v => Many2ManyRelationshipDefinition.Parse(v.Value));
            var @default = new Dictionary<string, string>
            {
                //{RelationshipEntityLogicalName},{PrimaryEntityLogicalName},{PrimaryEntityIdName},{AssociatedEntityIdName}
                //{ "BulkOperation_Accounts", "bulkoperationlog,bulkoperation,bulkoperationid,regardingobjectid"},
                //{ "BulkOperation_Contacts", "bulkoperationlog,bulkoperation,bulkoperationid,regardingobjectid"},
                //{ "BulkOperation_Leads", "bulkoperationlog,bulkoperation,bulkoperationid,regardingobjectid"},
                //{ "CampaignActivity_Accounts", "bulkoperationlog,campaignactivity,campaignactivityid,regardingobjectid"},
                //{ "CampaignActivity_Contacts", "bulkoperationlog,campaignactivity,campaignactivityid,regardingobjectid"},
                //{ "CampaignActivity_Leads", "bulkoperationlog,campaignactivity,campaignactivityid,regardingobjectid"},
                //{ "ChannelAccessProfile_Privilege", "channelaccessprofileentityaccesslevel,privilege,entityaccesslevelid,channelaccessprofileid"},
                //{ "KnowledgeBaseRecord_Incident", "incidentknowledgebaserecord,knowledgebaserecord,knowledgebaserecordid,incidentid"},
                //{ "accountleads_association", "accountleads,account,leadid,accountid"},
                //{ "applicationuserprofile", "applicationuserprofile,applicationuser,applicationuserid,fieldsecurityprofileid"},
                //{ "applicationuserrole", "applicationuserrole,applicationuser,applicationuserid,roleid"},
                //{ "appmoduleroles_association", "appmoduleroles,appmodule,appmoduleid,roleid"},
                //{ "bot_botcomponent", "bot_botcomponent,bot,botid,botcomponentid"},
                //{ "bot_environmentvariabledefinition", "bot_environmentvariabledefinition,bot,botid,environmentvariabledefinitionid"},
                //{ "botcomponent_botcomponent", "botcomponent_botcomponent,botcomponent,botcomponentidone,botcomponentidtwo"},
                //{ "botcomponent_environmentvariabledefinition", "botcomponent_environmentvariabledefinition,botcomponent,botcomponentid,environmentvariabledefinitionid"},
                //{ "botcomponent_workflow", "botcomponent_workflow,botcomponent,botcomponentid,workflowid"},
                //{ "campaignactivitylist_association", "campaignactivityitem,campaignactivity,campaignactivityid,itemid"},
                //{ "campaignactivitysalesliterature_association", "campaignactivityitem,campaignactivity,campaignactivityid,itemid"},
                //{ "campaigncampaign_association", "campaignitem,campaign,campaignid,entityid"},
                //{ "campaignlist_association", "campaignitem,campaign,campaignid,entityid"},
                //{ "campaignproduct_association", "campaignitem,campaign,campaignid,entityid"},
                //{ "campaignsalesliterature_association", "campaignitem,campaign,campaignid,entityid"},
                //{ "competitorproduct_association", "competitorproduct,competitor,competitorid,productid"},
                //{ "competitorsalesliterature_association", "competitorsalesliterature,salesliterature,salesliteratureid,competitorid"},
                { "connectionroleassociation_association", "connectionroleassociation,connectionrole,connectionroleid,connectionrole,associatedconnectionroleid"},
                //{ "contact_subscription_association", "subscriptionmanuallytrackedobject,subscription,subscriptionid,objectid"},
                //{ "contactinvoices_association", "contactinvoices,invoice,invoiceid,contactid"},
                //{ "contactleads_association", "contactleads,contact,contactid,leadid"},
                //{ "contactorders_association", "contactorders,salesorder,salesorderid,contactid"},
                //{ "contactquotes_association", "contactquotes,quote,quoteid,contactid"},
                //{ "entitlementcontacts_association", "entitlementcontacts,contact,contactid,entitlementid"},
                //{ "knowledgearticle_category", "knowledgearticlescategories,knowledgearticle,knowledgearticleid,categoryid"},
                //{ "leadcompetitors_association", "leadcompetitors,lead,leadid,competitorid"},
                //{ "leadproduct_association", "leadproduct,lead,leadid,productid"},
                //{ "listaccount_association", "listmember,list,listid,entityid"},
                //{ "listcontact_association", "listmember,list,listid,entityid"},
                //{ "listlead_association", "listmember,list,listid,entityid"},
                //{ "msdyn_aiodlabel_msdyn_aiconfiguration", "msdyn_aiodlabel_msdyn_aiconfiguration,msdyn_aiodlabel,msdyn_aiodlabelid,msdyn_aiconfigurationid"},
                //{ "msdyn_appconfig_msdyn_channelprovider", "msdyn_appconfig_msdyn_channelprovider,msdyn_appconfiguration,msdyn_appconfigurationid,msdyn_channelproviderid"},
                //{ "msdyn_appconfiguration_applicationextension", "msdyn_appconfiguration_applicationextension,msdyn_appconfiguration,msdyn_appconfigurationid,msdyn_applicationextensionid"},
                //{ "msdyn_appconfiguration_sessiontemplate", "msdyn_appconfiguration_sessiontemplate,msdyn_appconfiguration,msdyn_appconfigurationid,msdyn_sessiontemplateid"},
                //{ "msdyn_appconfiguration_systemuser", "msdyn_appconfiguration_systemuser,msdyn_appconfiguration,msdyn_appconfigurationid,systemuserid"},
                //{ "msdyn_callablecontext_msdyn_playbooktemplate", "msdyn_callablecontext_msdyn_playbooktemplate,msdyn_callablecontext,msdyn_callablecontextid,msdyn_playbooktemplateid"},
                //{ "msdyn_ciprovider_systemuser_membership", "msdyn_ciprovider_systemuser,msdyn_ciprovider,msdyn_ciproviderid,systemuserid"},
                //{ "msdyn_incident_msdyn_customerasset", "msdyn_incident_msdyn_customerasset,incident,incidentid,msdyn_customerassetid"},
                //{ "msdyn_iotdevicecategorycommands", "msdyn_iotdevicecategorycommands,msdyn_iotdevicecategory,msdyn_iotdevicecategoryid,msdyn_iotdevicecommanddefinitionid"},
                //{ "msdyn_iotdevicecommandparameters", "msdyn_iotdevicecommandparameters,msdyn_iotdevicecommanddefinition,msdyn_iotdevicecommanddefinitionid,msdyn_iotpropertydefinitionid"},
                //{ "msdyn_msdyn_cannedmessage_liveworkstream", "msdyn_msdyn_cannedmessage_liveworkstream,msdyn_cannedmessage,msdyn_cannedmessageid,msdyn_liveworkstreamid"},
                //{ "msdyn_msdyn_cannedmessage_msdyn_octag", "msdyn_msdyn_cannedmessage_msdyn_octag,msdyn_cannedmessage,msdyn_cannedmessageid,msdyn_octagid"},
                //{ "msdyn_msdyn_consoleapplicationnotificationtag", "msdyn_msdyn_consoleapplicationnotificationtag,msdyn_consoleapplicationnotificationtemplate,msdyn_consoleapplicationnotificationtemplateid,msdyn_templatetagsid"},
                //{ "msdyn_msdyn_consoleapplicationnotificationtempl", "msdyn_msdyn_consoleapplicationnotificationtem,msdyn_consoleapplicationnotificationtemplate,msdyn_consoleapplicationnotificationtemplateid,msdyn_consoleapplicationnotificationfieldid"},
                //{ "msdyn_msdyn_consoleapplicationsessiontemp_tag", "msdyn_msdyn_consoleapplicationsessiontemp_tag,msdyn_consoleapplicationsessiontemplate,msdyn_consoleapplicationsessiontemplateid,msdyn_templatetagsid"},
                //{ "msdyn_msdyn_consoleapplicationsessiontemplate_m", "msdyn_msdyn_consoleapplicationsessiontemplate,msdyn_consoleapplicationsessiontemplate,msdyn_consoleapplicationsessiontemplateid,msdyn_consoleapplicationtemplateid"},
                //{ "msdyn_msdyn_consoleapplicationtemplate_msdyn_co", "msdyn_msdyn_consoleapplicationtemplate_msdyn_,msdyn_consoleapplicationtemplate,msdyn_consoleapplicationtemplateid,msdyn_consoleapplicationtemplateparameterid"},
                //{ "msdyn_msdyn_consoleapplicationtemplate_tags", "msdyn_msdyn_consoleapplicationtemplate_tags,msdyn_consoleapplicationtemplate,msdyn_consoleapplicationtemplateid,msdyn_templatetagsid"},
                //{ "msdyn_msdyn_consoleapplicationtype_msdyn_consol", "msdyn_msdyn_consoleapplicationtype_msdyn_cons,msdyn_consoleapplicationtype,msdyn_consoleapplicationtypeid,msdyn_consoleappparameterdefinitionid"},
                //{ "msdyn_msdyn_customerasset_msdyn_3dmodel", "msdyn_msdyn_customerasset_msdyn_3dmodel,msdyn_customerasset,msdyn_customerassetid,msdyn_3dmodelid"},
                //{ "msdyn_msdyn_functionallocation_account", "msdyn_msdyn_functionallocation_account,msdyn_functionallocation,msdyn_functionallocationid,accountid"},
                //{ "msdyn_msdyn_journal_msdyn_expense", "msdyn_msdyn_journal_msdyn_expense,msdyn_journal,msdyn_journalid,msdyn_expenseid"},
                //{ "msdyn_msdyn_journal_msdyn_timeentry", "msdyn_msdyn_journal_msdyn_timeentry,msdyn_journal,msdyn_journalid,msdyn_timeentryid"},
                //{ "msdyn_msdyn_liveworkstream_systemuser", "msdyn_msdyn_liveworkstream_systemuser,msdyn_liveworkstream,msdyn_liveworkstreamid,systemuserid"},
                //{ "msdyn_msdyn_ocliveworkitem_knowledgearticle", "msdyn_msdyn_ocliveworkitem_knowledgeartic,msdyn_ocliveworkitem,activityid,knowledgearticleid"},
                //{ "msdyn_msdyn_paneconfig_msdyn_appconfig", "msdyn_msdyn_paneconfig_msdyn_appconfig,msdyn_paneconfiguration,msdyn_paneconfigurationid,msdyn_appconfigurationid"},
                //{ "msdyn_msdyn_personalmessage_msdyn_octag", "msdyn_msdyn_personalmessage_msdyn_octag,msdyn_personalmessage,msdyn_personalmessageid,msdyn_octagid"},
                //{ "msdyn_msdyn_prod_actioninputparameter_msdyn_par", "msdyn_msdyn_prod_actioninputparameter_msdyn_par,msdyn_productivityactioninputparameter,msdyn_productivityactioninputparameterid,msdyn_productivityparameterdefinitionid"},
                //{ "msdyn_msdyn_prod_actionoutputparameter_msdyn_pa", "msdyn_msdyn_prod_actionoutputparameter_msdyn_pa,msdyn_productivityactionoutputparameter,msdyn_productivityactionoutputparameterid,msdyn_productivityparameterdefinitionid"},
                //{ "msdyn_msdyn_prod_agentscript_msdyn_sessiontemplat", "msdyn_msdyn_prod_agentscript_msdyn_sessiontempl,msdyn_productivityagentscript,msdyn_productivityagentscriptid,msdyn_sessiontemplateid"},
                //{ "msdyn_notificationtemplate_notificationfield", "msdyn_notificationtemplate_notificationfield,msdyn_notificationtemplate,msdyn_notificationtemplateid,msdyn_notificationfieldid"},
                //{ "msdyn_ocliveworkitem_knowledgebaserecord", "msdyn_msdyn_ocliveworkitem_knowledgebaser,msdyn_ocliveworkitem,activityid,knowledgebaserecordid"},
                //{ "msdyn_opt_provider_knowledgearticle", "msdyn_opt_provider_knowledgearticle,opt_provider,opt_providerid,knowledgearticleid"},
                //{ "msdyn_organizationalunit_pricelevel", "msdyn_organizationalunit_pricelevel,msdyn_organizationalunit,msdyn_organizationalunitid,pricelevelid"},
                //{ "msdyn_resourcerequirement_bookableresource", "msdyn_resourcerequirement_bookableresource,msdyn_resourcerequirement,msdyn_resourcerequirementid,bookableresourceid"},
                //{ "msdyn_resourcerequirement_bookingheader", "msdyn_resourcerequirement_bookingheader,msdyn_resourcerequirement,msdyn_resourcerequirementid,bookableresourcebookingheaderid"},
                //{ "msdyn_resourcerequirement_systemuser", "msdyn_resourcerequirement_systemuser,msdyn_resourcerequirement,msdyn_resourcerequirementid,systemuserid"},
                //{ "msdyn_sessiontemplate_applicationtab", "msdyn_sessiontemplate_applicationtab,msdyn_sessiontemplate,msdyn_sessiontemplateid,msdyn_applicationtabtemplateid"},
                //{ "msdyn_smartassistconfig_msdyn_appconfig", "msdyn_smartassistconfig_msdyn_appconfig,msdyn_smartassistconfig,msdyn_smartassistconfigid,msdyn_appconfigurationid"},
                //{ "msdyn_systemuser_msdyn_omnichannelqueue", "msdyn_systemuser_msdyn_omnichannelqueue,systemuser,systemuserid,msdyn_omnichannelqueueid"},
                //{ "msdyusd_answer_agentscriptaction", "msdyusd_answer_agentscriptaction,msdyusd_answer,msdyusd_answerid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_auditdiag_tracesourcesetting", "msdyusd_auditdiag_tracesourcesetting,msdyusd_auditanddiagnosticssetting,msdyusd_auditanddiagnosticssettingid,msdyusd_tracesourcesettingid"},
                //{ "msdyusd_configuration_actioncalls", "msdyusd_configuration_actioncalls,msdyusd_configuration,msdyusd_configurationid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_configuration_agentscript", "msdyusd_configuration_agentscript,msdyusd_configuration,msdyusd_configurationid,msdyusd_taskid"},
                //{ "msdyusd_configuration_entitysearch", "msdyusd_configuration_entitysearch,msdyusd_configuration,msdyusd_configurationid,msdyusd_entitysearchid"},
                //{ "msdyusd_configuration_event", "msdyusd_configuration_event,msdyusd_configuration,msdyusd_configurationid,msdyusd_uiieventid"},
                //{ "msdyusd_configuration_form", "msdyusd_configuration_form,msdyusd_configuration,msdyusd_configurationid,msdyusd_formid"},
                //{ "msdyusd_configuration_hostedcontrol", "msdyusd_configuration_hostedcontrol,msdyusd_configuration,msdyusd_configurationid,uii_hostedapplicationid"},
                //{ "msdyusd_configuration_option", "msdyusd_configuration_option,msdyusd_configuration,msdyusd_configurationid,uii_optionid"},
                //{ "msdyusd_configuration_scriptlet", "msdyusd_configuration_scriptlet,msdyusd_configuration,msdyusd_configurationid,msdyusd_scriptletid"},
                //{ "msdyusd_configuration_sessionlines", "msdyusd_configuration_sessionlines,msdyusd_configuration,msdyusd_configurationid,msdyusd_sessioninformationid"},
                //{ "msdyusd_configuration_toolbar", "msdyusd_configuration_toolbar,msdyusd_configuration,msdyusd_configurationid,msdyusd_toolbarstripid"},
                //{ "msdyusd_configuration_windowroute", "msdyusd_configuration_windowroute,msdyusd_configuration,msdyusd_configurationid,msdyusd_windowrouteid"},
                //{ "msdyusd_customizationfiles_configuration", "msdyusd_customizationfiles_configuration,msdyusd_customizationfiles,msdyusd_customizationfilesid,msdyusd_configurationid"},
                //{ "msdyusd_form_hostedapplication", "msdyusd_form_hostedapplication,msdyusd_form,msdyusd_formid,uii_hostedapplicationid"},
                //{ "msdyusd_subactioncalls", "msdyusd_subactioncalls,msdyusd_agentscriptaction,msdyusd_agentscriptactionidone,msdyusd_agentscriptactionidtwo"},
                //{ "msdyusd_task_agentscriptaction", "msdyusd_task_agentscriptaction,msdyusd_task,msdyusd_taskid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_task_answer", "msdyusd_task_answer,msdyusd_task,msdyusd_taskid,msdyusd_answerid"},
                //{ "msdyusd_toolbarbutton_agentscriptaction", "msdyusd_toolbarbutton_agentscriptaction,msdyusd_toolbarbutton,msdyusd_toolbarbuttonid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_toolbarstrip_uii_hostedapplication", "msdyusd_toolbarstrip_uii_hostedapplication,msdyusd_toolbarstrip,msdyusd_toolbarstripid,uii_hostedapplicationid"},
                //{ "msdyusd_tracesourcesetting_hostedcontrol", "msdyusd_tracesourcesetting_hostedcontrol,msdyusd_tracesourcesetting,msdyusd_tracesourcesettingid,uii_hostedapplicationid"},
                //{ "msdyusd_uiievent_agentscriptaction", "msdyusd_uiievent_agentscriptaction,msdyusd_uiievent,msdyusd_uiieventid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_windowroute_agentscriptaction", "msdyusd_windowroute_agentscriptaction,msdyusd_windowroute,msdyusd_windowrouteid,msdyusd_agentscriptactionid"},
                //{ "msdyusd_windowroute_ctisearch", "msdyusd_windowroute_ctisearch,msdyusd_windowroute,msdyusd_windowrouteid,msdyusd_searchid"},
                //{ "opportunitycompetitors_association", "opportunitycompetitors,opportunity,opportunityid,competitorid"},
                //{ "package_solution", "package_solution,package,packageid,solutionid"},
                //{ "product_entitlement_association", "entitlementproducts,product,productid,entitlementid"},
                //{ "product_entitlementtemplate_association", "entitlementtemplateproducts,product,productid,entitlementtemplateid"},
                //{ "productsalesliterature_association", "productsalesliterature,product,productid,salesliteratureid"},
                //{ "queuemembership_association", "queuemembership,queue,queueid,systemuserid"},
                //{ "roleprivileges_association", "roleprivileges,privilege,privilegeid,roleid"},
                //{ "roletemplateprivileges_association", "roletemplateprivileges,roletemplate,roletemplateid,privilegeid"},
                //{ "servicecontractcontacts_association", "servicecontractcontacts,contact,contactid,contractid"},
                //{ "serviceplan_appmodule", "serviceplanappmodules,serviceplan,serviceplanid,appmoduleid"},
                //{ "systemuserprofiles_association", "systemuserprofiles,systemuser,systemuserid,fieldsecurityprofileid"},
                //{ "systemuserroles_association", "systemuserroles,systemuser,systemuserid,roleid"},
                //{ "systemusersyncmappingprofiles_association", "systemusersyncmappingprofiles,systemuser,systemuserid,syncattributemappingprofileid"},
                //{ "task_subscription_association", "subscriptionmanuallytrackedobject,subscription,subscriptionid,objectid"},
                //{ "teammembership_association", "teammembership,team,teamid,systemuserid"},
                //{ "teamprofiles_association", "teamprofiles,team,teamid,fieldsecurityprofileid"},
                //{ "teamroles_association", "teamroles,team,teamid,roleid"},
                //{ "teamsyncattributemappingprofiles_association", "teamsyncattributemappingprofiles,team,teamid,syncattributemappingprofileid"},
            };
            foreach (var kvp in @default.Where(k => !dictionary.ContainsKey(k.Key)))
            {
                dictionary[kvp.Key] = Many2ManyRelationshipDefinition.Parse(kvp.Value.Split(',').ToList());
            }
            return dictionary;
        }

        /// <summary>
        /// Settings for Crm System
        /// </summary>
        public class CrmSystemSettings
        {
            private static string _fullNameFormat;
            private static Guid? _businessUnitId;
            private static Guid? _userId;
            private static Guid? _onBehalfOfId;

            /// <summary>
            /// The id to be used as the id oof top level Business Unit.
            /// </summary>
            public static Guid BusinessUnitId
            {
                get => (_businessUnitId 
                        ?? (_businessUnitId = Config.GetAppSettingOrDefault("CrmSystemSettings.BusinessUnitId",
                                                                            new Guid("88501fd6-90b5-405f-a027-ce9903bc0bb3")))
                        ).GetValueOrDefault();
                set => _businessUnitId = value;
            }

            /// <summary>
            /// Defines the full name format.  Defaults to F I L <para/>
            /// Format of FullName <para/>
            ///   F = First Name <para/>
            ///   M = Middle Name <para/>
            ///   I = Middle Initial <para/>
            ///   L = Last Name 
            /// </summary>
            /// <value>
            /// The full name format (always upper case).
            /// </value>
            public static string FullNameFormat
            {
                get => _fullNameFormat ?? (_fullNameFormat = Config.GetAppSettingOrDefault("CrmSystemSettings.FullNameFormat", "F I L").ToUpper());
                set => _fullNameFormat = value;
            }

            /// <summary>
            /// The id to be used as the id of the current user.
            /// </summary>
            public static Guid OnBehalfOfId
            {
                get => (_onBehalfOfId
                        ?? (_onBehalfOfId = Config.GetAppSettingOrDefault("CrmSystemSettings.OnBehalfOfId", Guid.Empty))
                    ).GetValueOrDefault();
                set => _onBehalfOfId = value;
            }

            /// <summary>
            /// The id to be used as the id of the current user.
            /// </summary>
            public static Guid UserId
            {
                get => (_userId
                        ?? (_userId = Config.GetAppSettingOrDefault("CrmSystemSettings.UserId",
                            new Guid("ba815d1d-f62b-4ea1-912f-0aab76bd7462")))
                    ).GetValueOrDefault();
                set => _userId = value;
            }
        }   

        #region Helpers

        /// <summary>
        /// Helper method to default the field to the config value if it is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The field.</param>
        /// <param name="configName">Name of the configuration.</param>
        /// <param name="default">The default.</param>
        /// <returns></returns>
        protected static T GetValue<T>(ref T? field, string configName, T @default) where T : struct
        {
            if (!field.HasValue)
            {
                field = Config.GetAppSettingOrDefault(configName, @default);
            }
            return field.Value;
        }

        #endregion Helpers
    }
}
