using DLaB.Common;
using DLaB.Xrm.Client;
#if !PRE_MULTISELECT
using DLaB.Xrm.CrmSdk;
#endif
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
#if !XRM_2013
using Microsoft.Xrm.Sdk.Organization;
#endif



namespace DLaB.Xrm.LocalCrm
{
#if !DEBUG_XRM_UNIT_TEST_CODE
    [System.Diagnostics.DebuggerNonUserCode]
#endif
    partial class LocalCrmDatabaseOrganizationService
    {
        #region Execute Internal

        /// <summary>
        /// Fail-safe method.  Execute type not implemented, throw exception
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private OrganizationResponse ExecuteInternal(OrganizationRequest request)
        {
            throw new NotImplementedException(request.RequestName);
        }

        private AssociateResponse ExecuteInternal(AssociateRequest request)
        {
            return AssociateInternal(request.Target.LogicalName, request.Target.Id, request.Relationship, request.RelatedEntities);
        }

        private AssignResponse ExecuteInternal(AssignRequest request)
        {
            GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, request.Target.LogicalName, "Assign", this, request.Target, request.Assignee);
            return new AssignResponse();
        }

#if !PRE_MULTISELECT
        private CreateMultipleResponse ExecuteInternal(CreateMultipleRequest request)
        {
            AssertEntityNamePopulated(request);

            return new CreateMultipleResponse
            {
                Results =
                {
                    [nameof(CreateMultipleResponse.Ids)] = request.Targets.Entities.Select(Create).ToArray()
                }
            };
        }
#endif

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

        private DisassociateResponse ExecuteInternal(DisassociateRequest request)
        {
            Disassociate(request.Target.LogicalName, request.Target.Id, request.Relationship, request.RelatedEntities);
            return new DisassociateResponse();
        }
#if !PRE_KEYATTRIBUTE
        private ExecuteTransactionResponse ExecuteInternal(ExecuteTransactionRequest request)
        {
            var response = new ExecuteTransactionResponse
            {
                Results =
                {
                    ["Responses"] = new OrganizationResponseCollection()
                }
            };

            for (var i = 0; i < request.Requests.Count; i++)
            {
                var childRequest = request.Requests[i];
                OrganizationResponse childResponse = null;
                try
                {
                    if (childRequest.RequestName == "ExecuteMultiple")
                    {
                        throw new Exception("ExecuteMultipleRequest cannot contain an ExecuteMultipleRequest");
                    }

                    childResponse = ExecuteInternal((dynamic)childRequest);
                    if (request.ReturnResponses != true)
                    {
                        childResponse = null;
                    }
                }
                catch (NotImplementedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new FaultException<OrganizationServiceFault>(new ExecuteTransactionFault
                    {
                        FaultedRequestIndex = i,
                        Message = ex.Message,
                        Timestamp = DateTime.UtcNow,
                        ErrorDetails =
                        {
                            ["CallStack"] = ex.StackTrace
                        }
                    }, new FaultReason(ex.Message));
                }
                finally
                {
                    if (childResponse != null)
                    {
                        response.Responses.Add(childResponse);
                    }
                }
            }

            return response;
        }
#endif

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
            InitializeFromLogic.GetPropertiesByAttributeWithMatchingRelationships(GetType(request.TargetEntityName),
                request.EntityMoniker.LogicalName,
                out Dictionary<string, PropertyInfo> propertiesByAttribute,
                out List<string> relationshipProperties);

            var prefixlessKeys = propertiesByAttribute.Keys
#if NET
                                                      .Where(k => k.Contains('_'))
#else
                                                      .Where(k => k.Contains("_"))
#endif
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
                var sourceKey = (kvp.Key.Contains('_') 
                                    && (   kvp.NameMatching == InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToDestination
                                        || kvp.NameMatching == InitializeFromLogic.AttributeNameMatching.PrefixlessSourceToPrefixlessDestionation )
                                ? kvp.Key.SubstringByString("_")
                                : kvp.Key) ?? string.Empty;

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

        private static LocalTimeFromUtcTimeResponse ExecuteInternal(LocalTimeFromUtcTimeRequest request)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneCodeMappings.Value[request.TimeZoneCode]);
            var localTime = TimeZoneInfo.ConvertTime(request.UtcTime, timeZone);
            return new LocalTimeFromUtcTimeResponse
            {
                Results = {["LocalTime"] = localTime}
            };
        }

        private static readonly Lazy<Dictionary<int, string>> TimeZoneCodeMappings = new Lazy<Dictionary<int, string>>(() => new Dictionary<int, string>
        {
                {0, "Dateline Standard Time"},
                {1, "Samoa Standard Time"},
                {2, "Hawaiian Standard Time"},
                {3, "Alaskan Standard Time"},
                {4, "Pacific Standard Time"},
                {5, "Pacific Standard Time (Mexico)"},
                {6, "UTC-11"},
                {7, "Aleutian Standard Time"},
                {8, "Marquesas Standard Time"},
                {9, "UTC-09"},
                {10, "Mountain Standard Time"},
                {11, "UTC-08"},
                {12, "Mountain Standard Time (Mexico)"},
                {13, "Mexico Standard Time 2"},
                {15, "US Mountain Standard Time"},
                {20, "Central Standard Time"},
                {25, "Canada Central Standard Time"},
                {29, "Central Standard Time (Mexico)"},
                {30, "Mexico Standard Time"},
                {33, "Central America Standard Time"},
                {34, "Easter Island Standard Time"},
                {35, "Eastern Standard Time"},
                {40, "US Eastern Standard Time"},
                {43, "Haiti Standard Time"},
                {44, "Cuba Standard Time"},
                {45, "SA Pacific Standard Time"},
                {47, "Venezuela Standard Time"},
                {50, "Atlantic Standard Time"},
                {51, "Turks And Caicos Standard Time"},
                {55, "SA Western Standard Time"},
                {56, "Pacific SA Standard Time"},
                {58, "Central Brazilian Standard Time"},
                {59, "Paraguay Standard Time"},
                {60, "Newfoundland Standard Time"},
                {65, "E. South America Standard Time"},
                {69, "Argentina Standard Time"},
                {70, "SA Eastern Standard Time"},
                {71, "Bahia Standard Time"},
                {72, "Saint Pierre Standard Time"},
                {73, "Greenland Standard Time"},
                {74, "Montevideo Standard Time"},
                {75, "Mid-Atlantic Standard Time"},
                {76, "UTC-02"},
                {77, "Tocantins Standard Time"},
                {80, "Azores Standard Time"},
                {83, "Cape Verde Standard Time"},
                {84, "Morocco Standard Time"},
                {85, "GMT Standard Time"},
                {90, "Greenwich Standard Time"},
                {92, "UTC"},
                {95, "Central Europe Standard Time"},
                {100, "Central European Standard Time"},
                {105, "Romance Standard Time"},
                {110, "W. Europe Standard Time"},
                {113, "W. Central Africa Standard Time"},
                {115, "E. Europe Standard Time"},
                {120, "Egypt Standard Time"},
                {125, "FLE Standard Time"},
                {129, "Jordan Standard Time"},
                {130, "GTB Standard Time"},
                {131, "Middle East Standard Time"},
                {133, "Syria Standard Time"},
                {134, "Turkey Standard Time"},
                {135, "Jerusalem Standard Time"},
                {140, "South Africa Standard Time"},
                {141, "Namibia Standard Time"},
                {142, "West Bank Gaza Standard Time"},
                {145, "Russian Standard Time"},
                {150, "Arab Standard Time"},
                {151, "Belarus Standard Time"},
                {155, "E. Africa Standard Time"},
                {158, "Arabic Standard Time"},
                {159, "Kaliningrad Standard Time"},
                {160, "Iran Standard Time"},
                {165, "Arabian Standard Time"},
                {169, "Azerbaijan Standard Time"},
                {170, "Caucasus Standard Time"},
                {172, "Mauritius Standard Time"},
                {173, "Georgian Standard Time"},
                {174, "Russia Time Zone 3"},
                {175, "Afghanistan Standard Time"},
                {176, "Astrakhan Standard Time"},
                {180, "Ekaterinburg Standard Time"},
                {184, "Pakistan Standard Time"},
                {185, "West Asia Standard Time"},
                {190, "India Standard Time"},
                {193, "Nepal Standard Time"},
                {195, "Central Asia Standard Time"},
                {196, "Bangladesh Standard Time"},
                {197, "Omsk Standard Time"},
                {200, "Sri Lanka Standard Time"},
                {201, "N. Central Asia Standard Time"},
                {203, "Myanmar Standard Time"},
                {205, "SE Asia Standard Time"},
                {207, "North Asia Standard Time"},
                {208, "Altai Standard Time"},
                {209, "W. Mongolia Standard Time"},
                {210, "China Standard Time"},
                {211, "Tomsk Standard Time"},
                {215, "Singapore Standard Time"},
                {220, "Taipei Standard Time"},
                {225, "W. Australia Standard Time"},
                {227, "North Asia East Standard Time"},
                {228, "Ulaanbaatar Standard Time"},
                {229, "North Korea Standard Time"},
                {230, "Korea Standard Time"},
                {231, "Aus Central W. Standard Time"},
                {235, "Tokyo Standard Time"},
                {240, "Yakutsk Standard Time"},
                {241, "Transbaikal Standard Time"},
                {245, "AUS Central Standard Time"},
                {250, "Cen. Australia Standard Time"},
                {251, "Adelaide (Commonwealth Games 2006)"},
                {255, "AUS Eastern Standard Time"},
                {256, "Canberra, Melbourne, Sydney (Commonwealth Games 2006)"},
                {260, "E. Australia Standard Time"},
                {265, "Tasmania Standard Time"},
                {266, "Hobart (Commonwealth Games 2006)"},
                {270, "Vladivostok Standard Time"},
                {274, "Lord Howe Standard Time"},
                {275, "West Pacific Standard Time"},
                {276, "Bougainville Standard Time"},
                {277, "Norfolk Standard Time"},
                {278, "Sakhalin Standard Time"},
                {279, "Russia Time Zone 10"},
                {280, "Central Pacific Standard Time"},
                {281, "Magadan Standard Time"},
                {284, "UTC+12"},
                {285, "Fiji Standard Time"},
                {290, "New Zealand Standard Time"},
                {295, "Russia Time Zone 11"},
                {299, "Chatham Islands Standard Time"},
                {300, "Tonga Standard Time"},
                {301, "Eastern Standard Time (Mexico)"},
                {302, "Sudan Standard Time"}
        });

        private static QueryExpressionToFetchXmlResponse ExecuteInternal(QueryExpressionToFetchXmlRequest request)
        {
            return new QueryExpressionToFetchXmlResponse
            {
                ["FetchXml"] = LocalCrmDatabase.ConvertQueryExpressionToFetchXml(request.Query as QueryExpression)
            };
        }

        private RetrieveAttributeResponse ExecuteInternal(RetrieveAttributeRequest request)
        {
            var response = new RetrieveAttributeResponse();
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType(request.EntityLogicalName);

            var propertyTypes = entityType?.GetProperties()
                .Where(p =>
                    p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == request.LogicalName
                ).Select(p => p.PropertyType.IsGenericType
                    ? p.PropertyType.GenericTypeArguments.First()
                    : p.PropertyType).ToList();

            var propertyType = propertyTypes?.Count == 1
                ? propertyTypes[0]
                : propertyTypes?.FirstOrDefault(p => p != typeof(OptionSetValue) 
                                                  && p != typeof(EntityReference)); // Handle OptionSets/EntityReferences that may have multiple properties

            if (propertyType is null)
            {
                throw new Exception($"Unable to find a property for Entity {request.EntityLogicalName} and property {request.LogicalName} in {CrmServiceUtility.GetEarlyBoundProxyAssembly().FullName}");
            }

            AttributeMetadata metadata;
            if (propertyType.IsEnum || propertyTypes.Any(p => p == typeof(OptionSetValue)))
            {
                metadata = CreateOptionSetAttributeMetadata(request, propertyType);
            }
            else if (propertyType == typeof(string))
            {
                metadata = new StringAttributeMetadata(request.LogicalName);
            }
            else if (propertyTypes.Any(p => p == typeof(EntityReference)))
            {
                metadata = new LookupAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
#if !XRM_2013
            else if (propertyType == typeof(Guid))
            {
                metadata = new UniqueIdentifierAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
#endif
            else if (propertyType == typeof(bool))
            {
                metadata = new BooleanAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(Money))
            {
                metadata = new MoneyAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(int))
            {
                metadata = new IntegerAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(long))
            {
                metadata = new BigIntAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(DateTime))
            {
                metadata = new DateTimeAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(double))
            {
                metadata = new DoubleAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else if (propertyType == typeof(decimal))
            {
                metadata = new DecimalAttributeMetadata
                {
                    LogicalName = request.LogicalName
                };
            }
            else
            {
                throw new NotImplementedException($"Attribute Type of {propertyType.FullName} is not implemented.");
            }
            response.Results["AttributeMetadata"] = metadata;
            return response;
        }

        private PicklistAttributeMetadata CreateOptionSetAttributeMetadata(RetrieveAttributeRequest request, Type propertyType)
        {

            if (propertyType == typeof(OptionSetValue))
            {
                var enumExpression =
                    CrmServiceUtility.GetEarlyBoundProxyAssembly().GetTypes().Where(
                        t =>
                            t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                            t.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0 &&
                            t.Name.Contains(request.LogicalName)).ToList();

                // Search By EntityLogicalName_LogicalName
                // Then By LogicName_EntityLogicalName
                // Then By LogicalName
                propertyType = enumExpression.FirstOrDefault(t => t.Name == request.EntityLogicalName + "_" + request.LogicalName) ??
                               enumExpression.FirstOrDefault(t => t.Name == request.LogicalName + "_" + request.EntityLogicalName) ??
                               enumExpression.FirstOrDefault(t => t.Name == request.LogicalName);
            }

            var optionSet = new PicklistAttributeMetadata
            {
                OptionSet = new OptionSetMetadata()
            };
            AddEnumTypeValues(optionSet.OptionSet,
                propertyType,
                $"Unable to find local OptionSet enum for entity: {request.EntityLogicalName}, attribute: {request.LogicalName}");

            return optionSet;
        }

#if !XRM_2013
        private RetrieveCurrentOrganizationResponse ExecuteInternal(RetrieveCurrentOrganizationRequest _)
        {
            var detail = new OrganizationDetail
            {
#if !PRE_MULTISELECT
                DatacenterId = Info.DataCenterId,
                EnvironmentId = Info.EnvironmentId.ToString(),
                Geo = Info.Geo,
                OrganizationType = Info.OrganizationType,
                SchemaType = Info.SchemaType,
                TenantId = Info.TenantId.ToString(),
#endif
                FriendlyName = Info.FriendlyName,
                OrganizationId = Info.OrganizationId,
                OrganizationVersion = Info.OrganizationVersion,
                State = Info.State,
                UniqueName = Info.DatabaseName,
                UrlName = Info.UrlName,
            };

            typeof(OrganizationDetail).GetField("_endpoints", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(detail, Info.Endpoints);

            return new RetrieveCurrentOrganizationResponse
            {
                Results =
                {
                    [nameof(RetrieveCurrentOrganizationResponse.Detail)] = detail
                }
            };
        }
#endif

        private RetrieveEntityResponse ExecuteInternal(RetrieveEntityRequest request)
        {
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType(request.LogicalName);
            var metadata = TypeToMetadataGenerator.Generate(entityType, request.LogicalName, Info.PrimaryNameProvider.GetPrimaryName(request.LogicalName), Info.LanguageCode);

            return new RetrieveEntityResponse
            {
                Results =
                {
                    ["EntityMetadata"] = metadata
                }
            };
        }

        private RetrieveRelationshipResponse ExecuteInternal(RetrieveRelationshipRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new NotImplementedException("Unable to process a RetrieveRelationshipRequest without a Name");
            }

            PropertyInfo property = null;
            foreach (var type in CrmServiceUtility.GetEarlyBoundProxyAssembly(Info.EarlyBoundEntityAssembly).GetTypes())
            {
                property = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                               .FirstOrDefault(a =>
                               {
                                   if (a.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), false).Length == 0)
                                   {
                                       return false;
                                   }
                                   
                                   var att = a.GetCustomAttributes(typeof(RelationshipSchemaNameAttribute), false);
                                   return att.Length != 0 && ((RelationshipSchemaNameAttribute)att.FirstOrDefault())?.SchemaName == request.Name;
                               });
                if (property != null)
                {
                    break;
                }
            }

            if (property == null)
            {
                throw new KeyNotFoundException($"Unable to find a relationship {request.Name}.");
            }

            RelationshipMetadataBase relationship;
            if (typeof(Entity).IsAssignableFrom(property.PropertyType))
            {
                var referencedType = EntityHelper.GetEntityLogicalName(property.PropertyType);
                relationship = new OneToManyRelationshipMetadata
                {
                    ReferencedEntity = referencedType,
                    ReferencedAttribute = EntityHelper.GetIdAttributeName(property.PropertyType),
                    ReferencingEntity = EntityHelper.GetEntityLogicalName(property.DeclaringType!),
                    ReferencingAttribute = property.GetAttributeLogicalName()
                };
            }
            else
            {
                var att = property.GetAttributeLogicalName() ?? throw new Exception($"Property {property.DeclaringType?.FullName}.{property.Name} is missing AttributeLogicalNameAttribute");
                relationship = new ManyToManyRelationshipMetadata
                {
                    IntersectEntityName = att.Substring(0, att.Length - "_association".Length)
                };
            }
            var response = new RetrieveRelationshipResponse
            {
                Results =
                {
                    ["RelationshipMetadata"] = relationship
                }
            };
            return response;    
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

            var types = CrmServiceUtility.GetEarlyBoundProxyAssembly(Info.EarlyBoundEntityAssembly).GetTypes();
            var enumType = types.FirstOrDefault(t => IsEnumType(t, request.Name)) ?? types.FirstOrDefault(t => IsEnumType(t, request.Name + "Enum"));

            AddEnumTypeValues(optionSet, enumType, "Unable to find global optionset enum " + request.Name);

            return response;
        }

        private RetrieveResponse ExecuteInternal(RetrieveRequest request)
        {
            var response = new RetrieveResponse();
#if PRE_KEYATTRIBUTE
            var entity = Retrieve(request.Target.LogicalName, request.Target.Id, request.ColumnSet);
#else
            var entity = RetrieveEntityViaKeyAttributes(request.Target, request.ColumnSet);
#endif
            response.Results.Add("Entity", entity);
            
            if (request.RelatedEntitiesQuery != null)
            {
                foreach (var kvp in request.RelatedEntitiesQuery)
                {
                    var related = RetrieveMultiple(kvp.Value);
                    entity.RelatedEntities.Add(kvp.Key, related);
                }
            }

            return response;
        }

#if !PRE_KEYATTRIBUTE
        private RetrieveTotalRecordCountResponse ExecuteInternal(RetrieveTotalRecordCountRequest request)
        {
            var result = new EntityRecordCountCollection();

            foreach (var entityName in request.EntityNames)
            {
                result.Add(entityName, this.GetAllEntities(entityName).Count());
            }
            var response = new RetrieveTotalRecordCountResponse
            {
                Results =
                {
                    [nameof(EntityRecordCountCollection)] = result
                }
            };
            return response;
        }
#endif

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

#if !PRE_MULTISELECT
        private UpdateMultipleResponse ExecuteInternal(UpdateMultipleRequest request)
        {
            AssertEntityNamePopulated(request);

            foreach (var target in request.Targets.Entities)
            {
                Update(target);
            }

            return new UpdateMultipleResponse();
        }
#endif

        private UpdateResponse ExecuteInternal(UpdateRequest request)
        {
            Update(request.Target);
            return new UpdateResponse();
        }

#if !PRE_MULTISELECT
        private UpsertMultipleResponse ExecuteInternal(UpsertMultipleRequest request)
        {
            AssertEntityNamePopulated(request);

            var response = new UpsertMultipleResponse();
            ((OrganizationResponse)response).Results[nameof(UpsertMultipleResponse.Results)] = request.Targets.Entities.Select(t => ExecuteInternal(new UpsertRequest { Target = t })).ToArray();

            return response;
        }
#endif

#if !PRE_KEYATTRIBUTE
        private UpsertResponse ExecuteInternal(UpsertRequest request)
        {
            var target = request.Target.Clone();

            var existing = RetrieveEntityViaKeyAttributes(target);

            if (target.KeyAttributes?.Count > 0)
            {
                foreach (var kvp in target.KeyAttributes)
                {
                    // Key Attributes get added to the values if they don't exist
                    if (!target.Contains(kvp.Key))
                    {
                        target[kvp.Key] = kvp.Value;
                    }
                }
            }

            var recordCreated = false;
            if(existing == null)
            {
                target.Id = Create(target);
                recordCreated = true;
            }
            else
            {
                target.Id = existing.Id;
                Update(target);
                target = existing;
            }

            return new UpsertResponse
            {
                [nameof(UpsertResponse.RecordCreated)] = recordCreated,
                [nameof(UpsertResponse.Target)] = target.ToEntityReference()
            };
        }

        private Entity RetrieveEntityViaKeyAttributes(Entity target, ColumnSet cs = null)
        {
            var eRef = target.ToEntityReference();
            if (eRef.Id == Guid.Empty
                && target.KeyAttributes != null)
            {
                eRef.KeyAttributes = target.KeyAttributes;
            }

            return RetrieveEntityViaKeyAttributes(eRef, cs);
        }

        private Entity RetrieveEntityViaKeyAttributes(EntityReference target, ColumnSet cs = null)
        {
            cs ??= new ColumnSet(false);
            if (target.Id != Guid.Empty)
            {
                // Retrieve will use the GUID if it exists over the Key Attributes
                return this.GetEntitiesById(target.LogicalName, cs, target.Id).FirstOrDefault();
            }

            return this.GetEntityOrDefault(target.LogicalName, target.KeyAttributes, cs);
        }
#endif

        // ReSharper disable once UnusedParameter.Local
        private WhoAmIResponse ExecuteInternal(WhoAmIRequest _)
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

#if !PRE_MULTISELECT
        private static void AssertEntityNamePopulated(OrganizationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Parameters.GetParameterValue<EntityCollection>(nameof(CreateMultipleRequest.Targets)).EntityName)){
                return;
            }

            var message = $"The '{request.RequestName}' method does not support entities of type 'none'. MessageProcessorCache returned MessageProcessor.Empty. ";
            throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault
            {
                ErrorCode = ErrorCodes.SdkEntityDoesNotSupportMessage,
                Message = message,
                Timestamp = DateTime.UtcNow,
            }, new FaultReason(message))
            {
#if net
                    HResult = ErrorCodes.SdkEntityDoesNotSupportMessage,
#endif
                Source = "Microsoft.PowerPlatform.Dataverse.Client"
            };
        }
#endif

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
            public static void GetPropertiesByAttributeWithMatchingRelationships(Type type,
                                                                                 string matchingRelationshipEntityLogicalName,
                                                                                 out Dictionary<string, PropertyInfo> propertiesByAttribute,
                                                                                 out List<string> relationshipProperties)
            {
                propertiesByAttribute = new Dictionary<string, PropertyInfo>();
                relationshipProperties = new List<string>();
                foreach (var property in EntityPropertiesCache.Instance.For(type).PropertiesByName.Values.Where(p => p.Name != "Id"))
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
                var prefixlessSource = sourceKey.Contains('_') ? sourceKey.SubstringByString("_") : null;
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

            private static readonly HashSet<string> InvalidForCreate = new HashSet<string> {
                Template.Fields.CreatedBy,
                Template.Fields.CreatedOn,
                Template.Fields.ModifiedBy,
                Template.Fields.ModifiedOn,
                Template.Fields.OwningBusinessUnit,
                Template.Fields.OwningUser,
                Template.Fields.OwningTeam,
                Email.Fields.StateCode,
                Email.Fields.StatusCode
            };


            private static readonly HashSet<string> InvalidForUpdate = new HashSet<string> {
                Template.Fields.CreatedBy,
                Template.Fields.CreatedOn,
                Template.Fields.CreatedOnBehalfBy,
                Template.Fields.ModifiedBy,
                Template.Fields.ModifiedOn,
                Template.Fields.OwningBusinessUnit,
                Template.Fields.OwningUser,
                Template.Fields.OwningTeam,
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
                            throw new DLaB.Common.Exceptions.EnumCaseUndefinedException<TargetFieldType>(targetFieldType);
                    }
                }
            }
        }
    }
}
