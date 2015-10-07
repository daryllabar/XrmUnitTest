using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using DLaB.Common;
using DLaB.Xrm.Client;
using DLaB.Xrm.Test.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Test.Builders
{
    public sealed class OrganizationServiceBuilder : OrganizationServiceBuilderBase<OrganizationServiceBuilder>
    {
        protected override OrganizationServiceBuilder This
        {
            get { return this; }
        }

        public OrganizationServiceBuilder() : this(TestBase.GetOrganizationService()) {}

        public OrganizationServiceBuilder(IOrganizationService service) : base(service) {}

    }

    public abstract class OrganizationServiceBuilderBase<TDerived> where TDerived : OrganizationServiceBuilderBase<TDerived>
    {
        #region Properties
       
        protected abstract TDerived This { get; }

        private IOrganizationService Service { get; set; }

        /// <summary>
        /// The Entity Ids used to populate Entities without any ids
        /// </summary>
        /// <value>
        /// The new entity default ids.
        /// </value>
        private Dictionary<string, Queue<Guid>> NewEntityDefaultIds { get; set; }

        /// <summary>
        /// The Entities constrained by id to be retrieved when querying CRM
        /// </summary>
        /// <value>
        /// The new entity default ids.
        /// </value>
        private Dictionary<string, List<Guid>> EntityFilter { get; set; }

        #region IOrganizationService Actions and Funcs

        private List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>> AssociateActions { get; set; }
        private List<Func<IOrganizationService, Entity, Guid>> CreateFuncs { get; set; }
        private List<Action<IOrganizationService, string, Guid>> DeleteActions { get; set; }
        private List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>> DisassociateActions { get; set; }
        private List<Func<IOrganizationService, OrganizationRequest, OrganizationResponse>> ExecuteFuncs { get; set; }
        private List<Func<IOrganizationService, QueryBase, EntityCollection>> RetrieveMultipleFuncs { get; set; }
        private List<Func<IOrganizationService, string, Guid, ColumnSet, Entity>> RetrieveFuncs { get; set; }
        private List<Action<IOrganizationService, Entity>> UpdateActions { get; set; }

        #endregion // IOrganizationService Actions and Funcs

        #endregion // Properties

        #region Constructors

        public OrganizationServiceBuilderBase(IOrganizationService service)
        {
            Service = service;
            NewEntityDefaultIds = new Dictionary<string, Queue<Guid>>();
            EntityFilter = new Dictionary<string, List<Guid>>();

            #region IOrganizationService Actions and Funcs

            AssociateActions = new List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>>();
            CreateFuncs = new List<Func<IOrganizationService, Entity, Guid>>();
            DeleteActions = new List<Action<IOrganizationService, string, Guid>>();
            DisassociateActions = new List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>>();
            ExecuteFuncs = new List<Func<IOrganizationService, OrganizationRequest, OrganizationResponse>>();
            RetrieveMultipleFuncs = new List<Func<IOrganizationService, QueryBase, EntityCollection>>();
            RetrieveFuncs = new List<Func<IOrganizationService, string, Guid, ColumnSet, Entity>>();
            UpdateActions = new List<Action<IOrganizationService, Entity>>();


            #endregion // IOrganizationService Actions and Funcs
        }

        #endregion // Constructors

        #region Fleunt Methods

        #region Simple Methods

        public TDerived WithFakeAssociate(params Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] action) { AssociateActions.AddRange(action); return This; }
        public TDerived WithFakeCreate(params Func<IOrganizationService, Entity, Guid>[] func) { CreateFuncs.AddRange(func); return This; }
        public TDerived WithFakeDelete(params Action<IOrganizationService, string, Guid>[] action) { DeleteActions.AddRange(action); return This; }
        public TDerived WithFakeDisassociate(params Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] action) { DisassociateActions.AddRange(action); return This; }
        public TDerived WithFakeExecute(params Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[] func) { ExecuteFuncs.AddRange(func); return This; }
        public TDerived WithFakeRetrieveMultiple(params Func<IOrganizationService, QueryBase, EntityCollection>[] func) { RetrieveMultipleFuncs.AddRange(func); return This; }
        public TDerived WithFakeRetrieve(params Func<IOrganizationService, string, Guid, ColumnSet, Entity>[] func) { RetrieveFuncs.AddRange(func); return This; }
        public TDerived WithFakeUpdate(params Action<IOrganizationService, Entity>[] action) { UpdateActions.AddRange(action); return This; }

        #endregion Simple Methods

        public TDerived AssertIdNonEmptyOnCreate()
        {
            CreateFuncs.Add(AssertIdNonEmptyOnCreate);
            return This;
        }

        [DebuggerHidden]
        private static Guid AssertIdNonEmptyOnCreate(IOrganizationService service, Entity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                throw TestSettings.TestFrameworkProvider.Value.GetFailedException($"An attempt was made to create an entity of type {entity.LogicalName} without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate.");
            }
            return service.Create(entity);
        }

        /// <summary>
        /// Asserts failure whenever an action is requested that would perform an update (Create / Delete / Update) of some sort
        /// </summary>
        /// <returns></returns>
        public TDerived IsReadOnly()
        {
            AssociateActions.Add((s, n, i, r, c) => { TestSettings.TestFrameworkProvider.AssertFail("An attempt was made to Associate Entities with a ReadOnly Service"); });
            WithNoCreates();
            DeleteActions.Add((s, n, i) => { TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Delete a(n) {n} Entity with id {i}, using a ReadOnly Service"); });
            DisassociateActions.Add((s, n, i, r, c) => { TestSettings.TestFrameworkProvider.AssertFail("An attempt was made to Disassociate Entities with a ReadOnly Service"); });
            ExecuteFuncs.Add((s, r) =>
            {
                var nonReadOnlyStartsWithNames = new List<string>
                {
                    "CanBe",
                    "CanManyToManyRequest",
                    "Download",
                    "Execute",
                    "Export",
                    "FetchXmlToQueryExpressionRequest",
                    "FindParentResourceGroupRequest",
                    "Get",
                    "Is",
                    "LocalTimeFromUtcTimeRequest",
                    "Query",
                    "Retrieve",
                    "Search",
                    "UtcTimeFromLocalTimeRequest",
                    "WhoAmIRequest"
                };
                if (nonReadOnlyStartsWithNames.Any(n => r.RequestName.StartsWith(n)))
                {
                    return s.Execute(r);
                }

                throw TestSettings.TestFrameworkProvider.Value.GetFailedException($"An attempt was made to Execute Request {r.RequestName} with a ReadOnly Service");
            });
            UpdateActions.Add((s, e) => { throw TestSettings.TestFrameworkProvider.Value.GetFailedException($"An attempt was made to Update a(n) {e.LogicalName} Entity with id {e.Id}, using a ReadOnly Service"); });
            return This;
        }

        public TDerived WithBusinessUnitDeleteAsDeactivate()
        {
            DeleteActions.Add((s, entityLogicalName, id) =>
            {
                if (entityLogicalName == BusinessUnit.EntityLogicalName)
                {
                    s.SetState(entityLogicalName, id, false);
                }
                
                s.Delete(entityLogicalName, id);
            });

            return This;
        }

        #region WithEntityFilter

        /// <summary>
        /// Adds a condition to all RetrieveMultiple calls that constrains the results to only the entities with ids given for entities with their logical name in the Ids collection.
        /// Entity types not contained in the Ids collection will be unrestrained
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithEntityFilter(params Id[] ids)
        {
            foreach (var id in ids)
            {
                EntityFilter.AddOrAppend(id);
            }

            return This;
        }

        /// <summary>
        /// Adds a condition to all RetrieveMultiple calls that constrains the results to only the entities with ids given for entities with their logical name matching the Type T.
        /// Entities of a Type other than "T" will not be constrainted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The entity ids.</param>
        /// <returns></returns>
        public TDerived WithEntityFilter<T>(params Guid[] ids) where T : Entity
        {
            return WithEntityFilter(EntityHelper.GetEntityLogicalName<T>(), ids);
        }

        /// <summary>
        /// Adds a condition to all RetrieveMultiple calls that constrains the results to only the entities with ids given for entities with their logical name in the entityIds collection.
        /// Entities of a Type other than the given logicalName will not be constrainted
        /// </summary>
        /// <param name="logicalName">Entity Logical Name.</param>
        /// <param name="ids">The entity ids.</param>
        /// <returns></returns>
        public TDerived WithEntityFilter(string logicalName, IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                EntityFilter.AddOrAppend(logicalName, id);
            }
            return This;
        }

        #endregion WithEntityFilter

        public TDerived WithEntityNameDefaulted(Func<Entity, EntityHelper.PrimaryFieldInfo,string> getName)
        {
            CreateFuncs.Add((s, e) =>
            {
                var logicalName = e.LogicalName;
                if (!String.IsNullOrWhiteSpace(logicalName))
                {
                    var info = EntityHelper.GetPrimaryFieldInfo(logicalName);

                    if (info.AttributeName != null && !e.Attributes.ContainsKey(info.AttributeName))
                    {
                        var name = getName(e, info);
                        e[info.AttributeName] = name.PadRight(info.MaximumLength).Substring(0, info.MaximumLength).TrimEnd();
                    }
                }
                return s.Create(e);
                });
            return This;
        }

        #region WithIdsDefaultedForCreate

        /// <summary>
        /// When an entity is attempted to be created without an id, and an Id was passed in for that particular entity type, the Guid of the Id will be used to populate the entity
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithIdsDefaultedForCreate(params Id[] ids)
        {

            WithIdsDefaultedForCreate((IEnumerable<Id>)ids);

            return This;
        }

        /// <summary>
        /// When an entity is attempted to be created without an id, and an Id was passed in for that particular entity type, the Guid of the Id will be used to populate the entity
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithIdsDefaultedForCreate(IEnumerable<Id> ids)
        {
            foreach (var id in ids)
            {
                NewEntityDefaultIds.AddOrEnqueue(id);
            }

            return This;
        }

        #endregion WithIdsDefaultedForCreate

        /// <summary>
        /// Defaults the Parent Businessunit Id of all business units to the root BU if not already populated
        /// </summary>
        /// <returns></returns>
        public TDerived WithDefaultParentBu()
        {
            CreateFuncs.Add((s, e) =>
            {
                if (e.LogicalName == BusinessUnit.EntityLogicalName && e.GetAttributeValue<EntityReference>(BusinessUnit.Fields.ParentBusinessUnitId) == null)
                {
                    var qe = QueryExpressionFactory.Create(BusinessUnit.EntityLogicalName, new ColumnSet(BusinessUnit.Fields.BusinessUnitId), BusinessUnit.Fields.ParentBusinessUnitId, null);
                    e[BusinessUnit.Fields.ParentBusinessUnitId] = s.GetFirst<Entity>(qe).ToEntityReference();
                }

                return s.Create(e);
            });
            return This;
        }


        #region WithReturnedEntities

        /// <summary>
        /// Defines the entities that will be returned when the particular entity type is queried for
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public TDerived WithReturnedEntities(Dictionary<string, List<Entity>> entities)
        {
            RetrieveMultipleFuncs.Add((s, q) =>
            {
                var qe = q as QueryExpression;
                if (qe == null || !entities.ContainsKey(qe.EntityName))
                {
                    return s.RetrieveMultiple(q);
                }

                return new EntityCollection(entities[qe.EntityName]);
            });

            return This;
        }

        public static EntityCollection WithReturnedEntities(IOrganizationService s, QueryBase qb, string logicalName, params Entity[] entities)
        {
            return s.MockOrDefault(qb, q => q.EntityName == logicalName, entities);
        }

        #endregion WithReturnedEntities

        public TDerived WithLocalOptionSetsRetrievedFromEnum(int? defaultLangaugeCode = null)
        {
            defaultLangaugeCode = defaultLangaugeCode ?? Config.GetAppSettingOrDefault("DefaultLanguageCode", 1033);
            ExecuteFuncs.Add((s, r) =>
            {
                var attRequest = r as RetrieveAttributeRequest;
                if (attRequest == null)
                {
                    return s.Execute(r);
                }

                var response = new RetrieveAttributeResponse();

                var optionSet = new PicklistAttributeMetadata
                {
                    OptionSet = new OptionSetMetadata()
                };

                response.Results["AttributeMetadata"] = optionSet;

                var enumExpression = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetTypes().Where(t =>
                    (t.Name == attRequest.EntityLogicalName + "_" + attRequest.LogicalName ||
                     t.Name == attRequest.LogicalName + "_" + attRequest.EntityLogicalName) &&
                    t.GetCustomAttributes(typeof (DataContractAttribute), false).Length > 0 &&
                    t.GetCustomAttributes(typeof (System.CodeDom.Compiler.GeneratedCodeAttribute), false).Length > 0);

                // Return EntityLogicalName_Logical Name first
                var enumType = enumExpression.OrderBy(t => t.Name != attRequest.EntityLogicalName + "_" + attRequest.LogicalName).FirstOrDefault();

                if (enumType == null)
                {
                    throw new Exception(String.Format("Unable to find local optionset enum for entity: {0}, attribute: {1}", attRequest.EntityLogicalName, attRequest.LogicalName));
                }

                foreach (var value in Enum.GetValues(enumType))
                {
                    optionSet.OptionSet.Options.Add(
                        new OptionMetadata
                        {
                            Value = (int) value,
                            Label = new Label
                            {
                                UserLocalizedLabel = new LocalizedLabel(value.ToString(), defaultLangaugeCode.Value),
                            },
                        });
                }

                return response;
            })
                ;
            return This;
        }

        /// <summary>
        /// Causes the Organization Service to throw an exception if an attempt is made to create an entity
        /// </summary>
        /// <returns></returns>
        public TDerived WithNoCreates()
        {
            CreateFuncs.Add((s, e) =>
            {
                TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Create a(n) {e.LogicalName} Entity with a ReadOnly Service{Environment.NewLine + Environment.NewLine}Entity Attributes:{e.ToStringAttributes()}");
                throw new Exception("AssertFail Failed to through an Exception");
            });
            return This;
        }

        public TDerived WithWebResourcePulledFromPath(string webResourceName, string path = null)
        {
            RetrieveMultipleFuncs.Add((s, q) =>
            {
                var qe = q as QueryExpression;
                if (qe == null || qe.EntityName != WebResource.EntityLogicalName || !s.IsLocalCrmService())
                {
                    return s.RetrieveMultiple(q);
                }

                var condition = qe.Criteria.Conditions.FirstOrDefault(c => c.AttributeName == "name" && c.Values != null && c.Values.Count == 1 && c.Values[0].ToString().Contains(webResourceName));
                if (condition == null)
                {
                    return s.RetrieveMultiple(q);
                }

                var name = condition.Values[0].ToString();
                if (path == null)
                {
                    // No Path given, Combine WebResource Name with TestSettingsPath
                    path = Path.Combine(TestSettings.WebResourcePath.Value, webResourceName);
                    if (!File.Exists(path))
                    {
                        throw new Exception("Path " + path + " does not exist!");
                    }
                }

                if (path != null && !File.Exists(path))
                {
                    // Path Doesn't exist, attempt to prepend the Web Resource Path
                    path = Path.Combine(TestSettings.WebResourcePath.Value, path);
                }

                try
                {
                    // Default Web Resource
                    var result = new EntityCollection();
                    result.Entities.Add(new Entity(WebResource.EntityLogicalName)
                    {
                        [WebResource.Fields.Name] = name,
                        [WebResource.Fields.Content] = File.ReadAllText(path).ToBase64(Encoding.UTF8, true)
                    });
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to load Web Resource from " + path, ex);
                }
            });
            return This;
        }

        #endregion // Fleunt Methods

        public IOrganizationService Build()
        {
            ApplyNewEntityDefaultIds();
            ApplyEntityFilter();

            return new FakeIOrganizationService(Service)
            {
                AssociateActions = AssociateActions.ToArray(),
                CreateFuncs = CreateFuncs.ToArray(),
                DeleteActions = DeleteActions.ToArray(),
                DisassociateActions = DisassociateActions.ToArray(),
                ExecuteFuncs = ExecuteFuncs.ToArray(),
                RetrieveMultipleFuncs = RetrieveMultipleFuncs.ToArray(),
                RetrieveFuncs = RetrieveFuncs.ToArray(),
                UpdateActions = UpdateActions.ToArray()
            };
        }

        private void ApplyEntityFilter()
        {
            if (EntityFilter.Any())
            {
                RetrieveMultipleFuncs.Add((s, q) =>
                {
                    var qe = q as QueryExpression;

                    if (qe == null)
                    {
                        return s.RetrieveMultiple(q);
                    }

                    foreach (var entityGroup in EntityFilter)
                    {
                        var idLogicalName = EntityHelper.GetIdAttributeName(entityGroup.Key);
                        foreach (var filter in qe.GetEntityFilters(entityGroup.Key))
                        {
                            filter.AddConditionEnforceAndFilterOperator(new ConditionExpression(idLogicalName, ConditionOperator.In, entityGroup.Value.Select(i => (object) i).ToArray()));
                        }
                    }
                    return s.RetrieveMultiple(q);
                });
            }
        }

        private void ApplyNewEntityDefaultIds()
        {
            if (!NewEntityDefaultIds.Any())
            {
                return;
            }

            CreateFuncs.Add((s, e) =>
            {
                DefaultIdForEntity(e);
                return s.Create(e);
            } );
            ExecuteFuncs.Add((s, r) =>
            {
                var email = r as SendEmailFromTemplateRequest;
                if (email != null)
                {
                    DefaultIdForEntity(email.Target);
                }
                return s.Execute(r);
            });
        }

        /// <summary>
        /// Defaults the id an any entity created using the Dictionary string, without an already defined Guid, to the default Id.  Doesn't actually create the Entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void DefaultIdForEntity(Entity entity)
        {
            Queue<Guid> ids;
            if (entity.Id != Guid.Empty || !NewEntityDefaultIds.TryGetValue(entity.LogicalName, out ids))
            {
                return;
            }
            if (ids.Count == 0)
            {
                throw new Exception(
                    String.Format("An attempt was made to create an entity of type {0}, but no id exists in the NewEntityDefaultIds Collection for it.{1}" +
                                  "Either the entity's Id was not populated as a part of initialization, or a call is needs to be added to to OrganizationServiceBuilder.WithIdsDefaultedForCreate(id)", entity.LogicalName,
                        Environment.NewLine));
            }
            entity.Id = ids.Dequeue();
        }

    }
}
