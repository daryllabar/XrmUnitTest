using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
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
            var response = new CreateResponse();
            response.Results["id"] = Create(request.Target);
            return response;
        }

        private DeleteResponse ExecuteInternal(DeleteRequest request)
        {
            Delete(request.Target.LogicalName, request.Target.Id);
            return new DeleteResponse();
        }

        private ExecuteMultipleResponse ExecuteInternal(ExecuteMultipleRequest request)
        {
            var settings = request.Settings;
            var response = new ExecuteMultipleResponse();
            response.Results["Responses"] = new ExecuteMultipleResponseItemCollection();

            for(int i=0; i<request.Requests.Count; i++)
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
                    fault = new OrganizationServiceFault {Message = ex.Message, Timestamp = DateTime.UtcNow};
                    fault.ErrorDetails["CallStack"] = ex.StackTrace;
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

        private UpdateResponse ExecuteInternal(UpdateRequest request)
        {
            Update(request.Target);
            return new UpdateResponse();
        }

        private FetchXmlToQueryExpressionResponse ExecuteInternal(FetchXmlToQueryExpressionRequest request)
        {
            var s = new XmlSerializer(typeof (FetchType));
            FetchType fetch;
            using (var r = new StringReader(request.FetchXml))
            {
                fetch = (FetchType) s.Deserialize(r);
                r.Close();
            }
            var qe = LocalCrmDatabase.ConvertFetchToQueryExpression(this, fetch);
            var response = new FetchXmlToQueryExpressionResponse();
            response["FetchXml"] = qe;
            return response;
        }

        private QueryExpressionToFetchXmlResponse ExecuteInternal(QueryExpressionToFetchXmlRequest request)
        {
            var response = new QueryExpressionToFetchXmlResponse();
            response["FetchXml"] = LocalCrmDatabase.ConvertQueryExpressionToFetchXml(request.Query as QueryExpression);
            return response;
        }

        private RetrieveAttributeResponse ExecuteInternal(RetrieveAttributeRequest request)
        {
            var response = new RetrieveAttributeResponse();

            var optionSet = new PicklistAttributeMetadata
            {
                OptionSet = new OptionSetMetadata()
            };

            response.Results["AttributeMetadata"] = optionSet;

            var enumExpression = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetTypes().Where(t =>
                t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                t.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0 &&
                t.Name.Contains(request.LogicalName)).ToList();

            // Search By EntityLogicalName_LogicalName
            // Then By LogicName_EntityLogicalName
            // Then By LogicaName
            var enumType = enumExpression.FirstOrDefault(t => t.Name == request.EntityLogicalName + "_" + request.LogicalName) ??
                       enumExpression.FirstOrDefault(t => t.Name == request.LogicalName + "_" + request.EntityLogicalName) ??
                       enumExpression.FirstOrDefault(t => t.Name == request.LogicalName);

            AddEnumTypeValues(optionSet.OptionSet, enumType,
                String.Format("Unable to find local optionset enum for entity: {0}, attribute: {1}", request.EntityLogicalName, request.LogicalName));

            return response;
        }

        private RetrieveOptionSetResponse ExecuteInternal(RetrieveOptionSetRequest request)
        {
            var response = new RetrieveOptionSetResponse();
            var optionSet = new OptionSetMetadata(new OptionMetadataCollection());

            response.Results["OptionSetMetadata"] = optionSet;

            var types = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetTypes();
            var enumType = types.FirstOrDefault(t => IsEnumType(t, request.Name)) ??
                           types.FirstOrDefault(t => IsEnumType(t, request.Name + "Enum"));

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

            }else{
                throw new InvalidOperationException("Expected Email Request to be of type email not " + entity.LogicalName);
            }
            
            return response;
        }


        #endregion // Execute Internal

        private static bool IsEnumType(Type t, string name)
        {
            return t.IsEnum &&
                   t.Name.Equals(name) &&
                   t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                   t.GetCustomAttributes(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0;
        }

        private void AddEnumTypeValues(OptionSetMetadata options, Type enumType, string error)
        {
            if (enumType == null)
            {
                throw new Exception(error);
            }

            foreach (var value in Enum.GetValues(enumType))
            {
                options.Options.Add(
                    new OptionMetadata
                    {
                        Value = (int) value,
                        Label = new Label
                        {
                            UserLocalizedLabel = new LocalizedLabel(value.ToString(), Info.LanguageCode),
                        },
                    });
            }
        }

        private RetrieveEntityResponse ExecuteInternal(RetrieveEntityRequest request)
        {
            var name = new LocalizedLabel(request.LogicalName, Info.LanguageCode);
            var response = new RetrieveEntityResponse();
            response.Results["EntityMetadata"] = new EntityMetadata
            {
                DisplayCollectionName = new Label(name, new [] { name }),
            };

            return response;
        }


        private RetrieveMultipleResponse ExecuteInternal(RetrieveMultipleRequest request)
        {
            var response = new RetrieveMultipleResponse();
            response.Results["EntityCollection"] = RetrieveMultiple(request.Query);
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

        // ReSharper disable once UnusedParameter.Local
        private WhoAmIResponse ExecuteInternal(WhoAmIRequest request)
        {
            var response = new WhoAmIResponse();
            response.Results["BusinessUnitId"] = Info.BusinessUnit.Id;
            response.Results["UserId"] = Info.User.Id;
            response.Results["OrganizationId"] = Info.OrganizationId;
            return response;
        }
    }
}
