using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm
{
    public class EntityHelper
    {
        public static string GetEntityLogicalName<T>() where T : Entity
        {
            return GetEntityLogicalName(typeof(T));
        }

        public static string GetEntityLogicalName(Type type)
        {
            if (type.IsGenericParameter && type.BaseType != null)
            {
                // Handle SomeType<TEntity> where TEntity : Entity
                return GetEntityLogicalName(type.BaseType);
            }

            var field = type.GetField("EntityLogicalName");
            if (field == null)
            {
                if (type == typeof(Entity))
                {
                    return "entity";
                }
                else
                {
                    throw new Exception("Type " + type.FullName + " does not contain an EntityLogicalName Field");
                }
            }
            return (string)field.GetValue(null);
        }

        #region Determine Type

        public static Type GetType<T>(string entityLogicalName) where T : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
        {
            var contextType = typeof(T);
            if (contextType.Name == "OrganizationServiceContext")
            {
                throw new Exception("Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }

            return GetType(contextType.Assembly, contextType.Namespace, entityLogicalName);
        }

        private static ConcurrentDictionary<string, Dictionary<String, Type>> _cache = new ConcurrentDictionary<string, Dictionary<String, Type>>();
        public static Type GetType(Assembly earlyBoundAssembly, string @namespace, string entityLogicalName)
        {
            var key = "LogicalNameToEntityMapping|" + earlyBoundAssembly.FullName + "|" + @namespace;
            var mappings = _cache.GetOrAdd(key, k =>
            {
                var entityType = typeof(Entity);
                return earlyBoundAssembly.GetTypes().
                    Where(t => t.Namespace == @namespace && entityType.IsAssignableFrom(t)).
                    Select(t => new
                    {
                        Key = GetEntityLogicalName(t),
                        Value = t
                    }).
                    ToDictionary(v => v.Key, v => v.Value);
            });

            Type otherType;
            if (!mappings.TryGetValue(entityLogicalName, out otherType))
            {
                throw new Exception(String.Format("Unable to find a Type in assembly \"{0}\", namespace \"{1}\", with a logical name of \"{2}\"", earlyBoundAssembly.FullName, @namespace, entityLogicalName));
            }

            return otherType;
        }

        public static bool IsTypeDefined<T>(string entityLogicalName) where T : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
        {
            var contextType = typeof(T);
            if (contextType.Name == "OrganizationServiceContext")
            {
                throw new Exception("Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }

            return IsTypeDefined(contextType.Assembly, contextType.Namespace, entityLogicalName);
        }

        public static bool IsTypeDefined(Assembly earlyBoundAssembly, string @namespace, string entityLogicalName)
        {
            var key = "LogicalNameToEntityMapping|" + earlyBoundAssembly.FullName + "|" + @namespace;
            var mappings = _cache.GetOrAdd(key, k =>
            {
                var entityType = typeof(Entity);
                return earlyBoundAssembly.GetTypes().
                    Where(t => t.Namespace == @namespace && entityType.IsAssignableFrom(t)).
                    Select(t => new
                    {
                        Key = GetEntityLogicalName(t),
                        Value = t
                    }).
                    ToDictionary(v => v.Key, v => v.Value);
            });

            return mappings.ContainsKey(entityLogicalName);
        }

        #endregion // Determine Type

        #region Determine Id Attribute Name

        /// <summary>
        /// Returns the attribute name of the id of the entity
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public static string GetIdAttributeName(string logicalName)
        {
            return GetIrregularIdAttributeName(logicalName) ?? logicalName + "id";
        }

        /// <summary>
        /// Returns the attribute name of the id of the entity
        /// </summary>
        /// <typeparam name="T">Entity Type to use Reflection to lookup the entity logical name for</typeparam>
        /// <returns></returns>
        public static string GetIdAttributeName<T>() where T : Entity
        {
            return GetIdAttributeName(GetEntityLogicalName<T>());
        }

        /// <summary>
        /// Returns the attribute name of the id of the entity if it doesn't follow the standard (logicalName + id) rule, or null
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public static string GetIrregularIdAttributeName(string logicalName)
        {
            string name = null;

            switch (logicalName)
            {
                case ActivityPointer.EntityLogicalName:
                case Appointment.EntityLogicalName:
                case BulkOperation.EntityLogicalName:
                case CampaignActivity.EntityLogicalName:
                case CampaignResponse.EntityLogicalName:
                case Email.EntityLogicalName:
                case Fax.EntityLogicalName:
                case IncidentResolution.EntityLogicalName:
                case Letter.EntityLogicalName:
                case OpportunityClose.EntityLogicalName:
                case PhoneCall.EntityLogicalName:
                case QuoteClose.EntityLogicalName:
                case RecurringAppointmentMaster.EntityLogicalName:
                case ServiceAppointment.EntityLogicalName:
                case Task.EntityLogicalName:
                    name = ActivityPointer.Fields.ActivityId;
                    break;
            }

            return name;
        }

        /// <summary>
        /// Returns the attribute name of the id of the entity if it doesn't follow the standard (logicalName + id) rule, or null
        /// </summary>
        /// <typeparam name="T">Entity Type to use Reflection to lookup the entity logical name for</typeparam>
        /// <returns></returns>
        public static string GetIrregularIdAttributeName<T>() where T : Entity
        {
            return GetIrregularIdAttributeName(GetEntityLogicalName<T>());
        }

        #endregion // Determine Id Attribute Name

        #region Determine Entity Attribute Name Name

        /// <summary>
        /// Gets the Primary Field (name) info. 
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <returns></returns>
        public static PrimaryFieldInfo GetPrimaryFieldInfo(string logicalName)
        {
            var info = new PrimaryFieldInfo();
            if (logicalName.Length >= 4 && logicalName[3] == '_')
            {
                // Handle Special Cases
                switch (logicalName)
                {
                    default:
                        info.AttributeName = logicalName.Substring(0, 4) + "name";
                        break;
                }
            }
            else
            {
                switch (logicalName)
                {
                    case BusinessUnitNewsArticle.EntityLogicalName:
                        info.AttributeName = BusinessUnitNewsArticle.Fields.ArticleTitle;
                        info.MaximumLength = 300;
                        break;

                    case TransactionCurrency.EntityLogicalName:
                        info.AttributeName = TransactionCurrency.Fields.CurrencyName;
                        break;

                    case CustomerRelationship.EntityLogicalName:
                        info.AttributeName = "customerroleidname";
                        break;

                    case ImportJob.EntityLogicalName:
                        info.AttributeName = ImportJob.Fields.Data;
                        info.MaximumLength = 1073741823;
                        break;

                    case TransformationParameterMapping.EntityLogicalName:
                        info.AttributeName = TransformationParameterMapping.Fields.Data;
                        info.MaximumLength = 500;
                        break;

                    case ActivityMimeAttachment.EntityLogicalName:
                        info.AttributeName = ActivityMimeAttachment.Fields.FileName;
                        info.MaximumLength = 255;
                        break;

                    case Contact.EntityLogicalName:
                    case Lead.EntityLogicalName:
                    case SystemUser.EntityLogicalName:
                        info.AttributeName = Contact.Fields.FullName;
                        break;

                    case Solution.EntityLogicalName:
                    case Publisher.EntityLogicalName:
                        info.AttributeName = Solution.Fields.FriendlyName;
                        break;

                    case Account.EntityLogicalName:
                    case AsyncOperation.EntityLogicalName:
                    case BulkDeleteOperation.EntityLogicalName:
                    case BusinessUnit.EntityLogicalName:
                    case Calendar.EntityLogicalName:
                    case CalendarRule.EntityLogicalName:
                    case Campaign.EntityLogicalName:
                    case Competitor.EntityLogicalName:
                    case Connection.EntityLogicalName:
                    case ConnectionRole.EntityLogicalName:
                    case ConstraintBasedGroup.EntityLogicalName:
                    case ContractTemplate.EntityLogicalName:
                    case ConvertRule.EntityLogicalName:
                    case ConvertRuleItem.EntityLogicalName:
                    case CustomerAddress.EntityLogicalName:
                    case DiscountType.EntityLogicalName:
                    case DuplicateRule.EntityLogicalName:
                    case EmailServerProfile.EntityLogicalName:
                    case Entitlement.EntityLogicalName:
                    case EntitlementChannel.EntityLogicalName:
                    case EntitlementTemplate.EntityLogicalName:
                    case EntitlementTemplateChannel.EntityLogicalName:
                    case Equipment.EntityLogicalName:
                    case FieldSecurityProfile.EntityLogicalName:
                    case GoalRollupQuery.EntityLogicalName:
                    case Import.EntityLogicalName:
                    case ImportFile.EntityLogicalName:
                    case ImportMap.EntityLogicalName:
                    case Invoice.EntityLogicalName:
                    case Mailbox.EntityLogicalName:
                    case MailMergeTemplate.EntityLogicalName:
                    case Metric.EntityLogicalName:
                    case Opportunity.EntityLogicalName:
                    case Organization.EntityLogicalName:
                    case PluginAssembly.EntityLogicalName:
                    case PluginType.EntityLogicalName:
                    case PriceLevel.EntityLogicalName:
                    case Privilege.EntityLogicalName:
                    case ProcessSession.EntityLogicalName:
                    case Product.EntityLogicalName:
                    case PublisherAddress.EntityLogicalName:
                    case Queue.EntityLogicalName:
                    case Quote.EntityLogicalName:
                    case RelationshipRole.EntityLogicalName:
                    case Report.EntityLogicalName:
                    case Resource.EntityLogicalName:
                    case ResourceGroup.EntityLogicalName:
                    case ResourceSpec.EntityLogicalName:
                    case Role.EntityLogicalName:
                    case RoutingRule.EntityLogicalName:
                    case RoutingRuleItem.EntityLogicalName:
                    case SalesLiterature.EntityLogicalName:
                    case SalesOrder.EntityLogicalName:
                    case SavedQuery.EntityLogicalName:
                    case SavedQueryVisualization.EntityLogicalName:
                    case SdkMessage.EntityLogicalName:
                    case SdkMessageProcessingStep.EntityLogicalName:
                    case SdkMessageProcessingStepImage.EntityLogicalName:
                    case Service.EntityLogicalName:
                    case ServiceEndpoint.EntityLogicalName:
                    case SharePointDocumentLocation.EntityLogicalName:
                    case SharePointSite.EntityLogicalName:
                    case Site.EntityLogicalName:
                    case SLA.EntityLogicalName:
                    case SystemForm.EntityLogicalName:
                    case Team.EntityLogicalName:
                    case Territory.EntityLogicalName:
                    case UoM.EntityLogicalName:
                    case UoMSchedule.EntityLogicalName:
                    case UserForm.EntityLogicalName:
                    case UserQuery.EntityLogicalName:
                    case UserQueryVisualization.EntityLogicalName:
                    case WebResource.EntityLogicalName:
                        info.AttributeName = Account.Fields.Name;
                        break;

                    case List.EntityLogicalName:
                        info.AttributeName = List.Fields.ListName;
                        info.MaximumLength = 128;
                        break;

                    case ActivityParty.EntityLogicalName:
                        info.AttributeName = "partyidname";
                        info.MaximumLength = 400;
                        break;

                    case InvoiceDetail.EntityLogicalName:
                    case OpportunityProduct.EntityLogicalName:
                    case ProductPriceLevel.EntityLogicalName:
                    case QuoteDetail.EntityLogicalName:
                    case SalesOrderDetail.EntityLogicalName:
                        info.AttributeName = "productidname";
                        break;

                    case SocialProfile.EntityLogicalName:
                        info.AttributeName = SocialProfile.Fields.ProfileName;
                        break;

                    case PostFollow.EntityLogicalName:
                        info.AttributeName = "regardingobjectidname";
                        info.MaximumLength = 4000;
                        break;

                    case ColumnMapping.EntityLogicalName:
                        info.AttributeName = ColumnMapping.Fields.SourceAttributeName;
                        info.MaximumLength = 160;
                        break;

                    case ProcessStage.EntityLogicalName:
                        info.AttributeName = ProcessStage.Fields.StageName;
                        break;

                    case ActivityPointer.EntityLogicalName:
                    case Annotation.EntityLogicalName:
                    case Appointment.EntityLogicalName:
                    case BulkOperation.EntityLogicalName:
                    case CampaignActivity.EntityLogicalName:
                    case CampaignResponse.EntityLogicalName:
                    case Email.EntityLogicalName:
                    case Fax.EntityLogicalName:
                    case IncidentResolution.EntityLogicalName:
                    case Letter.EntityLogicalName:
                    case OpportunityClose.EntityLogicalName:
                    case OrderClose.EntityLogicalName:
                    case PhoneCall.EntityLogicalName:
                    case QuoteClose.EntityLogicalName:
                    case RecurringAppointmentMaster.EntityLogicalName:
                    case ServiceAppointment.EntityLogicalName:
                    case SocialActivity.EntityLogicalName:
                    case Task.EntityLogicalName:
                        info.AttributeName = ActivityPointer.Fields.Subject;
                        info.MaximumLength = 200;
                        break;

                    case TeamTemplate.EntityLogicalName:
                        info.AttributeName = TeamTemplate.Fields.TeamTemplateName;
                        break;

                    case Post.EntityLogicalName:
                    case PostComment.EntityLogicalName:
                    case TraceLog.EntityLogicalName:
                        info.AttributeName = Post.Fields.Text;
                        info.MaximumLength = 1000;
                        break;

                    case Contract.EntityLogicalName:
                    case ContractDetail.EntityLogicalName:
                    case Goal.EntityLogicalName:
                    case Incident.EntityLogicalName:
                    case KbArticle.EntityLogicalName:
                    case KbArticleComment.EntityLogicalName:
                    case KbArticleTemplate.EntityLogicalName:
                    case QueueItem.EntityLogicalName:
                    case SalesLiteratureItem.EntityLogicalName:
                    case Subject.EntityLogicalName:
                    case Template.EntityLogicalName:
                        info.AttributeName = Contract.Fields.Title;
                        break;

                    default:
                        info.AttributeName = null;
                        break;
                }
            }

            return info;
        }

        public class PrimaryFieldInfo
        {
            public string AttributeName { get; set; }
            public int MaximumLength { get; set; }

            public PrimaryFieldInfo()
            {
                MaximumLength = 100;
            }

        }

        #endregion // Determine Entity Attribute Name Name

    }
}
