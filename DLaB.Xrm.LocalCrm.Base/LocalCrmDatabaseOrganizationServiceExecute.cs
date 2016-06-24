using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using DLaB.Common;
using DLaB.Common.Exceptions;
using DLaB.Xrm.Client;
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;


namespace DLaB.Xrm.LocalCrm
{
    [DebuggerNonUserCode]
    partial class LocalCrmDatabaseOrganizationService
    {
        #region Execute Internal

        /// <summary>
        /// Fail safe method.  Execute type not implemented, throw exception
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private OrganizationResponse ExecuteInternal(OrganizationRequest request)
        {
            throw new NotImplementedException(request.RequestName);
        }

        private AssignResponse ExecuteInternal(AssignRequest request)
        {
            InvokeStaticGenericMethod(request.Target.LogicalName, "Assign", this, request.Target, request.Assignee);
            return new AssignResponse();
        }

        private CreateResponse ExecuteInternal(CreateRequest request)
        {
            return new CreateResponse
            {
                Results =
                {
                    ["id"] = Create(request.Target)
                }
            };
        }

        private CloseIncidentResponse ExecuteInternal(CloseIncidentRequest request)
        {
            Create(request.IncidentResolution);
            Update(new Entity
            {
                Id = request.IncidentResolution.GetAttributeValue<EntityReference>(Incident.Fields.IncidentId).GetIdOrDefault(),
                LogicalName = Incident.EntityLogicalName,
                [Incident.Fields.StatusCode] = request.Status,
                [Incident.Fields.StateCode] = new OptionSetValue((int) IncidentState.Resolved)
            });
            return new CloseIncidentResponse();
        }

        private DeleteResponse ExecuteInternal(DeleteRequest request)
        {
            Delete(request.Target.LogicalName, request.Target.Id);
            return new DeleteResponse();
        }

        private ExecuteMultipleResponse ExecuteInternal(ExecuteMultipleRequest request)
        {
            var settings = request.Settings;
            var response = new ExecuteMultipleResponse
            {
                Results =
                {
                    ["Responses"] = new ExecuteMultipleResponseItemCollection()
                }
            };

            for (int i = 0; i < request.Requests.Count; i++)
            {
                var childRequest = request.Requests[i];
                OrganizationServiceFault fault = null;
                OrganizationResponse childResponse = null;
                try
                {
                    if (childRequest.RequestName == "ExecuteMultiple")
                    {
                        throw new Exception("ExecuteMultipleRequest cannot contain an ExecuteMultipleRequest");
                    }

                    childResponse = ExecuteInternal((dynamic) childRequest);
                    if (!settings.ReturnResponses)
                    {
                        childResponse = null;
                    }
                }
                catch (NotImplementedException)
                {
                    throw;
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    response["IsFaulted"] = true;
                    fault = ex.Detail;
                    fault.ErrorDetails["CallStack"] = ex.StackTrace;
                    if (!settings.ContinueOnError)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    response["IsFaulted"] = true;
                    fault = new OrganizationServiceFault
                    {
                        Message = ex.Message,
                        Timestamp = DateTime.UtcNow,
                        ErrorDetails =
                        {
                            ["CallStack"] = ex.StackTrace
                        }
                    };
                    if (!settings.ContinueOnError)
                    {
                        break;
                    }
                }
                finally
                {
                    if (childResponse != null || fault != null)
                    {
                        response.Responses.Add(new ExecuteMultipleResponseItem
                        {
                            Fault = fault,
                            RequestIndex = i,
                            Response = childResponse
                        });
                    }
                }
            }
            return response;
        }

        private FetchXmlToQueryExpressionResponse ExecuteInternal(FetchXmlToQueryExpressionRequest request)
        {
            var s = new XmlSerializer(typeof(FetchType));
            FetchType fetch;
            using (var r = new StringReader(request.FetchXml))
            {
                fetch = (FetchType) s.Deserialize(r);
                r.Close();
            }
            var qe = LocalCrmDatabase.ConvertFetchToQueryExpression(this, fetch);
            return new FetchXmlToQueryExpressionResponse
            {
                ["FetchXml"] = qe
            };
        }

        /// <summary>
        /// Auto Map all values that are the same and properties that are of the same type as the Source Entity
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private InitializeFromResponse ExecuteInternal(InitializeFromRequest request)
        {
            var entity = new Entity(request.TargetEntityName);
            Dictionary<string, PropertyInfo> propertiesByAttribute;
            List<string> relationshipProperties;
            InitializeFromLogic.GetPropertiesByAttributeWithMatchingRelationships(GetType(request.TargetEntityName),
                request.EntityMoniker.LogicalName,
                out propertiesByAttribute,
                out relationshipProperties);

            var prefixlessKeys = propertiesByAttribute.Keys
                                                      .Where(k => k.Contains("_"))
                                                      .Select(k => new
                                                      {
                                                          PrefixlessName = k.SubstringByString("_"),
                                                          AttributeName = k
                                                      })
                                                      .ToDictionaryList(k => k.PrefixlessName, k => k.AttributeName);

            var source = Retrieve(request.EntityMoniker.LogicalName, request.EntityMoniker.Id, new ColumnSet(true));

            InitializeFromLogic.RemoveInvalidAttributes(source, request.TargetFieldType);

            foreach (var kvp in source.Attributes.Select(v => new
            {
                v.Key,
                v.Value,
                NameMatching = InitializeFromLogic.GetAttributeNameMatching(v.Key, propertiesByAttribute, prefixlessKeys)
            })
                                      .Where(v => v.NameMatching != InitializeFromLogic.AttributeNameMatching.None)
                                      .OrderBy(v => v.NameMatching))
            {
                var sourceKey = kvp.Key.Contains("_") 
                                    && (   kvp.NameMatching == InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToDestination
                                        || kvp.NameMatching == InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToPrefixlessDestionation )
                                ? kvp.Key.SubstringByString("_")
                                : kvp.Key;

                switch (kvp.NameMatching)
                {
                    case InitializeFromLogic.AttributeNameMatching.Exact:
                    case InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToDestination:
                        if (!entity.Attributes.ContainsKey(sourceKey))
                        {
                            entity[sourceKey] = kvp.Value;
                        } // else it has already been set by a better match, skip
                        break;
                    case InitializeFromLogic.AttributeNameMatching.SourceToPrefixlessDestination:
                    case InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToPrefixlessDestionation:
                        foreach (var destinationKey in prefixlessKeys[sourceKey])
                        {
                            if (!entity.Attributes.ContainsKey(destinationKey))
                            {
                                entity[destinationKey] = kvp.Value;
                            } // else it has already been set by a better match, skip 
                        }
                        break;
                }
            }

            foreach (var relationship in relationshipProperties)
            {
                entity[relationship] = request.EntityMoniker;
            }

            return new InitializeFromResponse
            {
                ["Entity"] = entity
            };
        }

        private QueryExpressionToFetchXmlResponse ExecuteInternal(QueryExpressionToFetchXmlRequest request)
        {
            return new QueryExpressionToFetchXmlResponse
            {
                ["FetchXml"] = LocalCrmDatabase.ConvertQueryExpressionToFetchXml(request.Query as QueryExpression)
            };
        }

        private RetrieveAttributeResponse ExecuteInternal(RetrieveAttributeRequest request)
        {
            var response = new RetrieveAttributeResponse();

            var optionSet = new PicklistAttributeMetadata
            {
                OptionSet = new OptionSetMetadata()
            };

            response.Results["AttributeMetadata"] = optionSet;

            var enumExpression =
                CrmServiceUtility.GetEarlyBoundProxyAssembly().
                                  GetTypes().
                                  Where(
                                      t =>
                                          t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                                          t.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0 &&
                                          t.Name.Contains(request.LogicalName)).
                                  ToList();

            // Search By EntityLogicalName_LogicalName
            // Then By LogicName_EntityLogicalName
            // Then By LogicaName
            var enumType = enumExpression.FirstOrDefault(t => t.Name == request.EntityLogicalName + "_" + request.LogicalName) ??
                           enumExpression.FirstOrDefault(t => t.Name == request.LogicalName + "_" + request.EntityLogicalName) ??
                           enumExpression.FirstOrDefault(t => t.Name == request.LogicalName);

            AddEnumTypeValues(optionSet.OptionSet,
                enumType,
                $"Unable to find local optionset enum for entity: {request.EntityLogicalName}, attribute: {request.LogicalName}");

            return response;
        }

        private RetrieveEntityResponse ExecuteInternal(RetrieveEntityRequest request)
        {
            var name = new LocalizedLabel(request.LogicalName, Info.LanguageCode);
            return new RetrieveEntityResponse
            {
                Results =
                {
                    ["EntityMetadata"] = new EntityMetadata
                    {
                        DisplayCollectionName = new Label(name,
                            new[]
                            {
                                name
                            }),
                    }
                }
            };
        }

        private RetrieveMultipleResponse ExecuteInternal(RetrieveMultipleRequest request)
        {
            return new RetrieveMultipleResponse
            {
                Results =
                {
                    ["EntityCollection"] = RetrieveMultiple(request.Query)
                }
            };
        }

        private RetrieveOptionSetResponse ExecuteInternal(RetrieveOptionSetRequest request)
        {
            var response = new RetrieveOptionSetResponse();
            var optionSet = new OptionSetMetadata(new OptionMetadataCollection());

            response.Results["OptionSetMetadata"] = optionSet;

            var types = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetTypes();
            var enumType = types.FirstOrDefault(t => IsEnumType(t, request.Name)) ?? types.FirstOrDefault(t => IsEnumType(t, request.Name + "Enum"));

            AddEnumTypeValues(optionSet, enumType, "Unable to find global optionset enum " + request.Name);

            return response;
        }


        private SendEmailFromTemplateResponse ExecuteInternal(SendEmailFromTemplateRequest request)
        {
            var response = new SendEmailFromTemplateResponse();
            var entity = request.Target;

            if (entity.LogicalName == Email.EntityLogicalName)
            {
                // Copy Email
                var email = entity.Clone();

                if (request.RegardingId != Guid.Empty)
                {
                    email[Email.Fields.RegardingObjectId] = new EntityReference(request.RegardingType, request.RegardingId);
                }
                var template = Service.Retrieve(Template.EntityLogicalName, request.TemplateId, new ColumnSet(true));
                email[Email.Fields.Description] = template[Template.Fields.Body];
                email[Email.Fields.Subject] = template[Template.Fields.Subject];
                response.Results["Id"] = Create(email);
            }
            else
            {
                throw new InvalidOperationException("Expected Email Request to be of type email not " + entity.LogicalName);
            }

            return response;
        }

        private SetStateResponse ExecuteInternal(SetStateRequest request)
        {
            var entity = Service.Retrieve(request.EntityMoniker.LogicalName, request.EntityMoniker.Id, new ColumnSet());
            var info = new LateBoundActivePropertyInfo(request.EntityMoniker.LogicalName);
            switch (info.ActiveAttribute)
            {
                case ActiveAttributeType.IsDisabled:
                    entity["isdisabled"] = request.State.GetValueOrDefault() == 1;

                    break;
                case ActiveAttributeType.None:
                case ActiveAttributeType.StateCode:
                    entity["statecode"] = request.State;
                    entity["statuscode"] = request.Status;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Service.Update(entity);
            return new SetStateResponse();
        }

        private UpdateResponse ExecuteInternal(UpdateRequest request)
        {
            Update(request.Target);
            return new UpdateResponse();
        }

        // ReSharper disable once UnusedParameter.Local
        private WhoAmIResponse ExecuteInternal(WhoAmIRequest request)
        {
            var response = new WhoAmIResponse
            {
                Results =
                {
                    ["BusinessUnitId"] = Info.BusinessUnit.Id,
                    ["UserId"] = Info.User.Id,
                    ["OrganizationId"] = Info.OrganizationId
                }
            };
            return response;
        }

        #endregion Execute Internal

        private void AddEnumTypeValues(OptionSetMetadata options, Type enumType, string error)
        {
            if (enumType == null)
            {
                throw new Exception(error);
            }

            foreach (var value in Enum.GetValues(enumType))
            {
                options.Options.Add(new OptionMetadata
                {
                    Value = (int) value,
                    Label = new Label
                    {
                        UserLocalizedLabel = new LocalizedLabel(value.ToString(), Info.LanguageCode),
                    },
                });
            }
        }

        private static bool IsEnumType(Type t, string name)
        {
            return t.IsEnum && t.Name.Equals(name) && t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                   t.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0;
        }

        private static class InitializeFromLogic
        {
            public enum AttributeNameMatching
            {
                None,
                Exact,
                PrefixlessSourceToDestination,
                SourceToPrefixlessDestination,
                PrefixlessSourceToPrefixlessDestionation
            }


            /// <summary>
            /// Gets the properties by attribute and relationship attributes where the referencing type is of the matching relationship entity.
            /// </summary>
            /// <param name="type">Type of the entity to lookup that properties of.</param>
            /// <param name="matchingRelationshipEntityLogicalName">Name of the matching relationship entity logical.</param>
            /// <param name="propertiesByAttribute">The properties by attribute.</param>
            /// <param name="relationshipProperties">The relationship properties.</param>
            public static void GetPropertiesByAttributeWithMatchingRelationships(IReflect type,
                                                                                 string matchingRelationshipEntityLogicalName,
                                                                                 out Dictionary<string, PropertyInfo> propertiesByAttribute,
                                                                                 out List<string> relationshipProperties)
            {
                propertiesByAttribute = new Dictionary<string, PropertyInfo>();
                relationshipProperties = new List<string>();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name != "Id"))
                {
                    var att = property.GetCustomAttribute<AttributeLogicalNameAttribute>();
                    if (att == null || (property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments[0].IsEnum && property.Name.EndsWith("Enum")))
                    {
                        // No AttributeLogicalName parameter, or the property is for a nullable Enum
                        continue;
                    }

                    var relationship = property.GetCustomAttribute<RelationshipSchemaNameAttribute>();
                    if (relationship == null)
                    {
                        propertiesByAttribute.Add(att.LogicalName, property);
                    }
                    else if (EntityHelper.GetEntityLogicalName(property.PropertyType) == matchingRelationshipEntityLogicalName)
                    {
                        relationshipProperties.Add(att.LogicalName);
                    }
                }
            }

            /// <summary>
            /// Gets the attribute name matching.
            /// Order is determined by
            ///     where the            source matches the            destination, then
            ///     where the prefixless source matches the            destination, then
            ///     where the            source matches the prefixless destination, then
            ///     where the prefixless source matches the prefixless destination
            /// </summary>
            /// <param name="sourceKey">The source key.</param>
            /// <param name="propertiesByAttribute">The properties by attribute.</param>
            /// <param name="prefixlessKeys">The prefixless keys.</param>
            /// <returns></returns>
            public static AttributeNameMatching GetAttributeNameMatching(string sourceKey,
                                                                         Dictionary<string, PropertyInfo> propertiesByAttribute,
                                                                         Dictionary<string, List<string>> prefixlessKeys)
            {
                var prefixlessSource = sourceKey.Contains("_") ? sourceKey.SubstringByString("_") : null;
                var matchingType = AttributeNameMatching.None;
                if (propertiesByAttribute.ContainsKey(sourceKey))
                {
                    matchingType = AttributeNameMatching.Exact;
                }
                else if (prefixlessSource != null && propertiesByAttribute.ContainsKey(prefixlessSource))
                {
                    matchingType = AttributeNameMatching.PrefixlessSourceToDestination;
                }
                else if (prefixlessKeys.ContainsKey(sourceKey))
                {
                    matchingType = AttributeNameMatching.SourceToPrefixlessDestination;
                }
                else if (prefixlessSource != null && prefixlessKeys.ContainsKey(prefixlessSource))
                {
                    matchingType = AttributeNameMatching.PrefixlessSourceToPrefixlessDestionation;
                }
                return matchingType;
            }

            private static HashSet<string> InvalidForCreate = new HashSet<string> {
                Template.Fields.CreatedBy,
                Template.Fields.CreatedOn,
                Template.Fields.ModifiedBy,
                Template.Fields.ModifiedOn,
                Template.Fields.OwningBusinessUnit,
                Email.Fields.StateCode,
                Email.Fields.StatusCode
            };


            private static HashSet<string> InvalidForUpdate = new HashSet<string> {
                Template.Fields.CreatedBy,
                Template.Fields.CreatedOn,
                Template.Fields.CreatedOnBehalfBy,
                Template.Fields.ModifiedBy,
                Template.Fields.ModifiedOn,
                Template.Fields.OwningBusinessUnit,
                Email.Fields.StateCode,
                Email.Fields.StatusCode
            };

            public static void RemoveInvalidAttributes(Entity source, TargetFieldType targetFieldType)
            {
                foreach (var attribute in source.Attributes.ToArray())
                {
                    switch (targetFieldType)
                    {
                        case TargetFieldType.All:
                        case TargetFieldType.ValidForRead:
                            break;
                        case TargetFieldType.ValidForCreate:
                            if (InvalidForCreate.Contains(attribute.Key))
                            {
                                source.Attributes.Remove(attribute.Key);
                            }
                            break;
                        case TargetFieldType.ValidForUpdate:
                            if (InvalidForUpdate.Contains(attribute.Key))
                            {
                                source.Attributes.Remove(attribute.Key);
                            }
                            break;
                        default:
                            throw new EnumCaseUndefinedException<TargetFieldType>(targetFieldType);
                    }
                }
            }
        }
    }
}
