using System;
using DLaB.Common.Exceptions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm
{
    public enum ActiveAttributeType
    {
        None,
        IsDisabled,
        StateCode,
    }

    public class ActivePropertyInfo<T> where T : Entity
    {
        public ActiveAttributeType ActiveAttribute { get; set; }
        public string AttributeName { get; set; }
        public int? ActiveState { get; set; }
        public int? NotActiveState { get; set; }

        public ActivePropertyInfo()
        {
            if (typeof(T) == typeof(Entity))
            {
                throw new TypeArgumentException("'Entity' is an invalid type for T.  Please use the LateBoundActivePropertyInfo.");
            }
            SetAttributeNameAndType(EntityHelper.GetEntityLogicalName<T>());
        }

        protected ActivePropertyInfo(string logicalName)
        {
            if (logicalName == null)
            {
                throw new ArgumentNullException("logicalName");
            }
            SetAttributeNameAndType(logicalName);
        }

        private void SetAttributeNameAndType(string logicalName)
        {
            switch (logicalName)
            {
                case BusinessUnit.EntityLogicalName:
                case Equipment.EntityLogicalName:
                case Organization.EntityLogicalName:
                case Resource.EntityLogicalName:
                case SystemUser.EntityLogicalName:
                    ActiveAttribute = ActiveAttributeType.IsDisabled;
                    AttributeName = SystemUser.Fields.IsDisabled;
                    break;

                #region Default CRM Entites with no active flag
                case AccountLeads.EntityLogicalName:
                case ActivityMimeAttachment.EntityLogicalName:
                case ActivityParty.EntityLogicalName:
                case Annotation.EntityLogicalName:
                case AnnualFiscalCalendar.EntityLogicalName:
                case AttributeMap.EntityLogicalName:
                case Audit.EntityLogicalName:
                case BulkDeleteFailure.EntityLogicalName:
                case BulkOperationLog.EntityLogicalName:
                case BusinessUnitNewsArticle.EntityLogicalName:
                case Calendar.EntityLogicalName:
                case CalendarRule.EntityLogicalName:
                case CampaignActivityItem.EntityLogicalName:
                case CampaignItem.EntityLogicalName:
                case Competitor.EntityLogicalName:
                case CompetitorProduct.EntityLogicalName:
                case CompetitorSalesLiterature.EntityLogicalName:
                case ConnectionRoleAssociation.EntityLogicalName:
                case ConnectionRoleObjectTypeCode.EntityLogicalName:
                case ConstraintBasedGroup.EntityLogicalName:
                case ContactInvoices.EntityLogicalName:
                case ContactLeads.EntityLogicalName:
                case ContactOrders.EntityLogicalName:
                case ContactQuotes.EntityLogicalName:
                case ContractTemplate.EntityLogicalName:
                case CustomerAddress.EntityLogicalName:
                case CustomerOpportunityRole.EntityLogicalName:
                case CustomerRelationship.EntityLogicalName:
                case ConvertRuleItem.EntityLogicalName:
                case Dependency.EntityLogicalName:
                case Discount.EntityLogicalName:
                case DisplayString.EntityLogicalName:
                case DuplicateRecord.EntityLogicalName:
                case DuplicateRuleCondition.EntityLogicalName:
                case DynamicPropertyAssociation.EntityLogicalName:
                case DynamicPropertyInstance.EntityLogicalName:
                case DynamicPropertyOptionSetItem.EntityLogicalName:
                case EntitlementChannel.EntityLogicalName:
                case EntitlementContacts.EntityLogicalName:
                case EntitlementProducts.EntityLogicalName:
                case EntitlementTemplate.EntityLogicalName:
                case EntitlementTemplateChannel.EntityLogicalName:
                case EntitlementTemplateProducts.EntityLogicalName:
                case EntityMap.EntityLogicalName:
                case ExchangeSyncIdMapping.EntityLogicalName:
                case FieldPermission.EntityLogicalName:
                case FieldSecurityProfile.EntityLogicalName:
                case FixedMonthlyFiscalCalendar.EntityLogicalName:
                case HierarchyRule.EntityLogicalName:
                case HierarchySecurityConfiguration.EntityLogicalName:
                case ImportJob.EntityLogicalName:
                case InvalidDependency.EntityLogicalName:
                case InvoiceDetail.EntityLogicalName:
                case IsvConfig.EntityLogicalName:
                case KbArticleComment.EntityLogicalName:
                case KbArticleTemplate.EntityLogicalName:
                case LeadAddress.EntityLogicalName:
                case LeadCompetitors.EntityLogicalName:
                case LeadProduct.EntityLogicalName:
                case License.EntityLogicalName:
                case ListMember.EntityLogicalName:
                case MonthlyFiscalCalendar.EntityLogicalName:                                                              
                case OpportunityCompetitors.EntityLogicalName:
                case OpportunityProduct.EntityLogicalName:
                case OrganizationUI.EntityLogicalName:
                case PluginAssembly.EntityLogicalName:
                case PluginType.EntityLogicalName:
                case PluginTypeStatistic.EntityLogicalName:
                case ProcessStage.EntityLogicalName:
                case ProcessTrigger.EntityLogicalName:
                case Post.EntityLogicalName:
                case PostComment.EntityLogicalName:
                case PostFollow.EntityLogicalName:
                case PostLike.EntityLogicalName:
                case PrincipalEntityMap.EntityLogicalName:
                case PrincipalObjectAttributeAccess.EntityLogicalName:
                case Privilege.EntityLogicalName:
                case ProductAssociation.EntityLogicalName:
                case ProductPriceLevel.EntityLogicalName:
                case ProductSalesLiterature.EntityLogicalName:
                case ProductSubstitute.EntityLogicalName:
                case Publisher.EntityLogicalName:
                case PublisherAddress.EntityLogicalName:
                case QuarterlyFiscalCalendar.EntityLogicalName:
                case QueueMembership.EntityLogicalName:
                case QuoteDetail.EntityLogicalName:
                case RecurrenceRule.EntityLogicalName:
                case RelationshipRoleMap.EntityLogicalName:
                case Report.EntityLogicalName:
                case ReportCategory.EntityLogicalName:
                case ReportEntity.EntityLogicalName:
                case ReportLink.EntityLogicalName:
                case ReportVisibility.EntityLogicalName:
                case ResourceGroup.EntityLogicalName:
                case ResourceSpec.EntityLogicalName:
                case RibbonCustomization.EntityLogicalName:
                case Role.EntityLogicalName:
                case RolePrivileges.EntityLogicalName:
                case RoleTemplatePrivileges.EntityLogicalName:
                case RollupField.EntityLogicalName:
                case RoutingRuleItem.EntityLogicalName:
                case SalesLiterature.EntityLogicalName:
                case SalesLiteratureItem.EntityLogicalName:
                case SalesOrderDetail.EntityLogicalName:
                case SavedQueryVisualization.EntityLogicalName:
                case SdkMessage.EntityLogicalName:
                case SdkMessageFilter.EntityLogicalName:
                case SdkMessagePair.EntityLogicalName:
                case SdkMessageProcessingStepImage.EntityLogicalName:
                case SdkMessageProcessingStepSecureConfig.EntityLogicalName:
                case SdkMessageRequest.EntityLogicalName:
                case SdkMessageRequestField.EntityLogicalName:
                case SdkMessageResponse.EntityLogicalName:
                case SdkMessageResponseField.EntityLogicalName:
                case SemiAnnualFiscalCalendar.EntityLogicalName:
                case Service.EntityLogicalName:
                case ServiceContractContacts.EntityLogicalName:
                case ServiceEndpoint.EntityLogicalName:
                case SharePointData.EntityLogicalName:
                case SharePointDocument.EntityLogicalName:
                case Site.EntityLogicalName:
                case SiteMap.EntityLogicalName:
                case SLAItem.EntityLogicalName:
                case SLAKPIInstance.EntityLogicalName:
                case SocialInsightsConfiguration.EntityLogicalName:
                case Solution.EntityLogicalName:
                case SolutionComponent.EntityLogicalName:
                case Subject.EntityLogicalName:
                case SubscriptionTrackingDeletedObject.EntityLogicalName:
                case SubscriptionManuallyTrackedObject.EntityLogicalName:
                case SystemForm.EntityLogicalName:
                case SystemUserLicenses.EntityLogicalName:
                case SystemUserProfiles.EntityLogicalName:
                case SystemUserRoles.EntityLogicalName:
                case SystemUserSyncMappingProfiles.EntityLogicalName:
                case Team.EntityLogicalName:
                case TeamMembership.EntityLogicalName:
                case TeamProfiles.EntityLogicalName:
                case TeamRoles.EntityLogicalName:
                case TeamSyncAttributeMappingProfiles.EntityLogicalName:
                case TeamTemplate.EntityLogicalName:
                case Template.EntityLogicalName:
                case Territory.EntityLogicalName:
                case TimeZoneDefinition.EntityLogicalName:
                case TimeZoneLocalizedName.EntityLogicalName:
                case TimeZoneRule.EntityLogicalName:
                case TraceLog.EntityLogicalName:
                case TransformationParameterMapping.EntityLogicalName:
                case UoM.EntityLogicalName:
                case UserEntityInstanceData.EntityLogicalName:
                case UserEntityUISettings.EntityLogicalName:
                case UserForm.EntityLogicalName:
                case UserQueryVisualization.EntityLogicalName:
                case UserSettings.EntityLogicalName:
                case WebResource.EntityLogicalName:
                case WorkflowDependency.EntityLogicalName:
                case WorkflowLog.EntityLogicalName:
                #endregion // Default CRM Entites with no active flag
                    ActiveAttribute = ActiveAttributeType.None;
                    break;

                default:
                    if (logicalName.Length > 4 && logicalName[3] == '_')
                    {
                        var prefix = logicalName.ToLower().Substring(0, 3);
                        if (logicalName.ToLower().Split(new [] { prefix }, StringSplitOptions.None).Length >= 3 || logicalName.ToLower().EndsWith("_association"))
                        {
                            // N:N Joins or association entities do not contain active flags
                            ActiveAttribute = ActiveAttributeType.None;
                            break;
                        }
                    }
                    SetStateAttributesAndValue(logicalName);
                    break;
            }
        }

        protected void SetStateAttributesAndValue(string logicalName)
        {
            ActiveAttribute = ActiveAttributeType.StateCode;
            AttributeName = "statecode";

            switch (logicalName)
            {
                // Entities with a Canceled State
                case "activitypointer":
                case "appointment":
                case "bulkoperation":
                case "campaignactivity":
                case "contractdetail":
                case "email":
                case "fax":
                case "letter":
                case "orderclose":
                case "phonecall":
                case "quoteclose":
                case "recurringappointmentmaster":
                case "serviceappointment":
                case "taskstate":
                    NotActiveState = 2;
                    break;

                case "duplicaterule": // don't ask me why, but this one is flipped
                    ActiveState = 1;
                    break;

                // Entities with states that can't be grouped into seperate all inclusive active and inactive states
                case "asyncoperation":
                case "bulkdeleteoperation":
                case "contract":
                case "lead":
                case "opportunity":
                case "processsession":
                case "quote":
                case "sdkmessageprocessingstep":
                case "workflow":
                    ActiveAttribute = ActiveAttributeType.None;
                    break;

                default:
                    if (IsJoinEntity(logicalName))
                    {
                        ActiveAttribute = ActiveAttributeType.None;
                    }
                    else
                    {
                        ActiveState = 0;
                    }
                    break;
            }
        }

        private bool IsJoinEntity(string logicalName)
        {
            // Entities of the type new_Foo_Bar are usually Join Entities that don't have a state
            return logicalName.Split('_').Length >= 3;
        }

        public static bool? IsActive(IOrganizationService service, Guid entityId)
        {
            var info = new ActivePropertyInfo<T>();
            var entity = service.GetEntity<T>(entityId, new ColumnSet(info.AttributeName));
            return IsActive(info, entity);
        }

        protected static bool? IsActive(ActivePropertyInfo<T> info, T entity)
        {
            bool? active;
            switch (info.ActiveAttribute)
            {
                case ActiveAttributeType.None:
                    // Unable to determine
                    active = null;
                    break;
                case ActiveAttributeType.IsDisabled:
                    active = !entity.GetAttributeValue<bool>(info.AttributeName);
                    break;
                case ActiveAttributeType.StateCode:
                    var state = entity.GetAttributeValue<OptionSetValue>(info.AttributeName).Value;
                    if (info.ActiveState.HasValue)
                    {
                        active = state == info.ActiveState;
                    }
                    else if (info.NotActiveState.HasValue)
                    {
                        active = state != info.NotActiveState;
                    }
                    else
                    {
                        throw new Exception("ActivePropertyInfo defines Attribute StateCode, but neither ActiveState or NotActiveState is popualted");
                    }
                    break;
                default:
                    throw new EnumCaseUndefinedException<ActiveAttributeType>(info.ActiveAttribute);
            }

            return active;
        }
    }
}
