#nullable enable
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using DLaB.Common;
using DLaB.Xrm.Client;
using DLaB.Xrm.LocalCrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
#if NET
using DataverseUnitTest.Entities;
using DLaB.Xrm;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseUnitTest.Builders
#else
using DLaB.Xrm.Test.Entities;

namespace DLaB.Xrm.Test.Builders
#endif
{ 
    /// <summary>
    /// Concrete Implementation of the OrganizationServiceBuilderBase
    /// </summary>
    public sealed class OrganizationServiceBuilder : OrganizationServiceBuilderBase<OrganizationServiceBuilder>
    {
        /// <summary>
        /// Gets the derived version of the class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override OrganizationServiceBuilder This => this;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationServiceBuilder" /> class.
        /// </summary>
        public OrganizationServiceBuilder() : this(TestBase.GetOrganizationService()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationServiceBuilder" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public OrganizationServiceBuilder(IOrganizationService service) : base(service) { }
    }

    /// <summary>
    /// Base class for Organization Service Builder Typed to OrganizationServiceBuilderBuildConfig
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    public abstract class OrganizationServiceBuilderBase<TDerived> : OrganizationServiceBuilderBase<TDerived, OrganizationServiceBuilderBuildConfig>
        where TDerived : OrganizationServiceBuilderBase<TDerived>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationServiceBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        protected OrganizationServiceBuilderBase(IOrganizationService service) : base(service) { }
    }

    /// <summary>
    /// Base class for Organization Service Builder
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    /// <typeparam name="TBuildConfig">The Build config type.</typeparam>
    public abstract class OrganizationServiceBuilderBase<TDerived, TBuildConfig> : IAgnosticServiceBuilder
        where TDerived : OrganizationServiceBuilderBase<TDerived, TBuildConfig>
        where TBuildConfig : IOrganizationServiceBuilderBuildConfig, new()
    {
        #region Properties

        /// <summary>
        /// Gets the Organization Service Builder of the derived Class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected abstract TDerived This { get; }

        private IOrganizationService Service { get; }

        /// <summary>
        /// The Entity Ids used to populate Entities without any ids
        /// </summary>
        /// <value>
        /// The new entity default ids.
        /// </value>
        private Dictionary<string, Queue<Guid>> NewEntityDefaultIds { get;}
        private bool NewEntityThrowErrorOnContextCreation { get; set; }

        /// <summary>
        /// The Entities constrained by id to be retrieved when querying CRM
        /// </summary>
        /// <value>
        /// The new entity default ids.
        /// </value>
        private Dictionary<string, List<Guid>> EntityFilter { get; }

        /// <summary>
        /// Defines a builder that will be inserted at lowest level allowed in the Fake IOrganizationService hierarchy
        /// </summary>
        public TDerived? PrimaryBuilder { get; set; }

        #region IOrganizationService Actions and Funcs

        private List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>> AssociateActions { get; }
        private List<Func<IOrganizationService, Entity, Guid>> CreateFuncs { get; }
        private List<Action<IOrganizationService, string, Guid>> DeleteActions { get; }
        private List<Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>> DisassociateActions { get; }
        private List<Func<IOrganizationService, OrganizationRequest, OrganizationResponse>> ExecuteFuncs { get; }
        private List<Func<IOrganizationService, QueryBase, EntityCollection>> RetrieveMultipleFuncs { get; }
        private List<Func<IOrganizationService, string, Guid, ColumnSet, Entity>> RetrieveFuncs { get; }
        private List<Action<IOrganizationService, Entity>> UpdateActions { get; }

        #endregion IOrganizationService Actions and Funcs

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationServiceBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        protected OrganizationServiceBuilderBase(IOrganizationService service)
        {
            Service = service;
            NewEntityDefaultIds = new Dictionary<string, Queue<Guid>>();
            NewEntityThrowErrorOnContextCreation = true;
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


            #endregion IOrganizationService Actions and Funcs
        }

        #endregion Constructors

        #region Fluent Methods

        #region Simple Methods

        /// <summary>
        /// Adds the fake associate.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public TDerived WithFakeAssociate(params Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] action) { AssociateActions.AddRange(action); return This; }
        /// <summary>
        /// Adds the fake create.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public TDerived WithFakeCreate(params Func<IOrganizationService, Entity, Guid>[] func) { CreateFuncs.AddRange(func); return This; }
        /// <summary>
        /// Adds the fake delete.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public TDerived WithFakeDelete(params Action<IOrganizationService, string, Guid>[] action) { DeleteActions.AddRange(action); return This; }
        /// <summary>
        /// Adds the fake disassociate.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public TDerived WithFakeDisassociate(params Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] action) { DisassociateActions.AddRange(action); return This; }
        /// <summary>
        /// Adds the fake execute.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public TDerived WithFakeExecute(params Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[] func) { ExecuteFuncs.AddRange(func); return This; }
        /// <summary>
        /// Adds the fake retrieve multiple.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultiple(params Func<IOrganizationService, QueryBase, EntityCollection>[] func) { RetrieveMultipleFuncs.AddRange(func); return This; }
        /// <summary>
        /// Adds the fake retrieve.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieve(params Func<IOrganizationService, string, Guid, ColumnSet, Entity>[] func) { RetrieveFuncs.AddRange(func); return This; }

        /// <summary>
        /// Simplifies the WithFakeRetrieve to a boolean return value and the value to return.
        /// </summary>
        /// <param name="shouldFakeRetrieve"></param>
        /// <param name="fakedValue"></param>
        /// <returns></returns>
        public TDerived WithFakeRetrieve(Func<IOrganizationService, string, Guid, ColumnSet, bool> shouldFakeRetrieve, Entity fakedValue)
        {
            WithFakeRetrieve((s, n, id, cs) => shouldFakeRetrieve(s, n, id, cs) ? fakedValue : s.Retrieve(n, id, cs)); return This;
        }
        /// <summary>
        /// Adds the fake update.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public TDerived WithFakeUpdate(params Action<IOrganizationService, Entity>[] action) { UpdateActions.AddRange(action); return This; }
        #endregion Simple Methods

        /// <summary>
        /// Asserts that any create of an entity has the id populated.  Useful to ensure that all entities can be deleted after they have been created since the id is known.
        /// </summary>
        /// <returns></returns>
        public TDerived AssertIdNonEmptyOnCreate()
        {
            CreateFuncs.Add(AssertIdNonEmptyOnCreate);
            ExecuteFuncs.Add(AssertIdNonEmptyOnExecuteMultiple);
            ExecuteFuncs.Add(AssertIdNonEmptyOnExecuteTransaction);
            ExecuteFuncs.Add(AssertIdNonEmptyOnUpsert);
            return This;
        }

        [DebuggerHidden]
        private static Guid AssertIdNonEmptyOnCreate(IOrganizationService service, Entity entity)
        {
            AssertIdNonEmptyOnCreate(entity);
            return service.Create(entity);
        }

        [DebuggerHidden]
        private static void AssertIdNonEmptyOnCreate(Entity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                throw TestSettings.TestFrameworkProvider.Value.GetFailedException($"An attempt was made to create an entity of type {entity.LogicalName} without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate.");
            }
        }

        [DebuggerHidden]
        private static OrganizationResponse AssertIdNonEmptyOnExecuteMultiple(IOrganizationService service, OrganizationRequest orgRequest)
        {
            if (orgRequest is ExecuteMultipleRequest requests)
            {
                AssertIdNonEmptyOnRequests(requests.Requests);
            }
            return service.Execute(orgRequest);
        }

        [DebuggerHidden]
        private static OrganizationResponse AssertIdNonEmptyOnExecuteTransaction(IOrganizationService service, OrganizationRequest orgRequest)
        {
#if !PRE_KEYATTRIBUTE
            if (orgRequest is ExecuteTransactionRequest requests)
            {
                AssertIdNonEmptyOnRequests(requests.Requests);
            }
#endif
            return service.Execute(orgRequest);
        }

        private static void AssertIdNonEmptyOnRequests(OrganizationRequestCollection requests)
        {
            foreach (var request in requests)
            {
                if (request is CreateRequest createRequest)
                {
                    AssertIdNonEmptyOnCreate(createRequest.Target);
                    continue;
                }

                AssertIdNonEmptyOnUpsert(request);
            }
        }

        [DebuggerHidden]
        private static OrganizationResponse AssertIdNonEmptyOnUpsert(IOrganizationService service, OrganizationRequest request)
        {
            AssertIdNonEmptyOnUpsert(request);
            return service.Execute(request);
        }
        [DebuggerHidden]
        private static void AssertIdNonEmptyOnUpsert(OrganizationRequest request)
        {
#if !PRE_KEYATTRIBUTE
            if (request is UpsertRequest upsert
                && upsert.Target.Id == Guid.Empty)
            {
                throw TestSettings.TestFrameworkProvider.Value.GetFailedException($"An attempt was made to create an entity of type {upsert.Target.LogicalName} without defining it's id.  Either use WithIdsDefaultedForCreate, or don't use the AssertIdNonEmptyOnCreate.");
            }
#endif
        }


        /// <summary>
        /// Asserts failure whenever an action is requested that would perform an update (Create / Delete / Update) of some sort
        /// </summary>
        /// <returns></returns>
        public TDerived IsReadOnly()
        {
            AssociateActions.Add((_, n, i, r, _) => { ReadOnlyFail.OnAssociate(n, i, r); });
            WithNoCreates();
            DeleteActions.Add((_, n, i) => { ReadOnlyFail.OnDelete(n, i); });
            DisassociateActions.Add((_, n, i, r, _) => { ReadOnlyFail.OnDisassociate(n, i, r); });
            ExecuteFuncs.Add((s, r) =>
            {
                ReadOnlyFail.OnExecute(r);
                return s.Execute(r);
            });
            UpdateActions.Add((_, e) => { ReadOnlyFail.OnUpdate(e); });
            return This;
        }

        /// <summary>
        /// Changes the Delete of a business unit to be a deactivate.  Allows for normal Deletion of all entities by the Test builder framework
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Defaults the Parent BusinessUnitId of all business units to the root BU if not already populated
        /// </summary>
        /// <returns></returns>
        public TDerived WithDefaultParentBu()
        {
            CreateFuncs.Add((s, e) =>
            {
                if (e.LogicalName == BusinessUnit.EntityLogicalName && e.GetAttributeValue<EntityReference>(BusinessUnit.Fields.ParentBusinessUnitId) == null)
                {
                    var qe = QueryExpressionFactory.Create(BusinessUnit.EntityLogicalName, new ColumnSet(BusinessUnit.Fields.BusinessUnitId), BusinessUnit.Fields.ParentBusinessUnitId, null!);
                    e[BusinessUnit.Fields.ParentBusinessUnitId] = s.GetFirst<Entity>(qe).ToEntityReference();
                }

                return s.Create(e);
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

        #region WithFakeAction

        /// <summary>
        /// Fakes any call to the given action with the given action response.
        /// </summary>
        /// <typeparam name="T">The early bound action response type.</typeparam>
        /// <param name="response">The response to return</param>
        /// <param name="actionLogicalName">Required unless the OrganizationResponse type has a field of name "ActionLogicalName" that contains the logical name for the action.</param>
        /// <returns></returns>
        public TDerived WithFakeAction<T>(T response, string? actionLogicalName = null) where T : OrganizationResponse
        {
            actionLogicalName = GetActionName<T>(actionLogicalName);

            WithFakeExecute((s, r) =>
            {
                if (r.RequestName == actionLogicalName)
                {
                    return response;
                }

                return s.Execute(r);
            });

            return This;
        }

        /// <summary>
        /// Fakes any call to the given action with the given action response.
        /// </summary>
        /// <typeparam name="TRequest">The early bound action request type.</typeparam>
        /// <typeparam name="TResponse">The early bound action response type.</typeparam>
        /// <param name="actionFake">The function to be called when the action is being executed.</param>
        /// <param name="actionLogicalName">Required unless the OrganizationResponse type has a field of name "ActionLogicalName" that contains the logical name for the action.</param>
        /// <returns></returns>
        public TDerived WithFakeAction<TRequest, TResponse>(Func<IOrganizationService, TRequest, TResponse> actionFake, string? actionLogicalName = null) 
            where TRequest: OrganizationRequest, new() 
            where TResponse: OrganizationResponse
        {
            actionLogicalName = GetActionName<TResponse>(actionLogicalName);

            WithFakeExecute((s, r) =>
            {
                if (r.RequestName == actionLogicalName)
                {
                    if(r is TRequest request) {
                        return actionFake(s, request);
                    }
                    else
                    {
                        // Request is not EarlyBound.  Convert it to an Early Bound Version
                        var localRequest = new TRequest()
                        {
                            RequestName = r.RequestName,
                            RequestId = r.RequestId,
                            ExtensionData = r.ExtensionData,
                            Parameters = r.Parameters
                        };

                        return actionFake(s, localRequest);
                    }
                }

                return s.Execute(r);
            });

            return This;
        }

        private static string? GetActionName<T>(string? actionLogicalName) where T : OrganizationResponse
        {
            if (!string.IsNullOrWhiteSpace(actionLogicalName))
            {
                return actionLogicalName;
            }

            if (typeof(T) == typeof(OrganizationRequest))
            {
                throw new ArgumentException("If the actionLogicalName is not populated, the request type must be a type derived from OrganizationResponse.");
            }
            var field = typeof(T).GetField("ActionLogicalName");
            actionLogicalName = (string?)field?.GetValue(null);
            if (string.IsNullOrWhiteSpace(actionLogicalName))
            {
                throw new ArgumentException($"Unable to retrieve the ActionLogicalName from the type {typeof(T).FullName}.  Either add a constant field named \"ActionLogicalName\", or populate the \"ActionName\" argument!");
            }

            return actionLogicalName;
        }


        #endregion WithFakeAction

        #region WithFakeRetrieve

        private readonly Dictionary<string, Entity> _fakeEntitiesToReturn = new Dictionary<string, Entity>();
        /// <summary>
        /// Forces any retrieve call of the particular entity type to return the given entity.  Does not apply to any other calls i.e. RetrieveMultiple.
        /// </summary>
        /// <param name="entity">The entity to return.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieve(Entity entity)
        {
            if (_fakeEntitiesToReturn.Count == 0)
            {
                WithFakeRetrieve((s, n, id, cs) =>{
                    if (_fakeEntitiesToReturn.TryGetValue(n, out var value))
                    {
                        return value;
                    }

                    return s.Retrieve(n, id, cs);
                });
            }

            _fakeEntitiesToReturn[entity.LogicalName] = entity;
            return This;
        }

        #endregion WithFakeRetrieve

        #region WithFakeRetrieveMultiple

        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple to a boolean return value and the value to return.
        /// </summary>
        /// <param name="shouldFakeRetrieveMultiple"></param>
        /// <param name="fakedValue"></param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultiple(Func<IOrganizationService, QueryBase, bool> shouldFakeRetrieveMultiple, EntityCollection fakedValue)
        {
            WithFakeRetrieveMultiple((s, qb) => shouldFakeRetrieveMultiple(s, qb) ? fakedValue : s.RetrieveMultiple(qb)); return This;
        }
        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple to a boolean return value and the value to return.
        /// </summary>
        /// <param name="shouldFakeRetrieveMultiple"></param>
        /// <param name="fakedValue"></param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultiple<T>(Func<IOrganizationService, QueryBase, bool> shouldFakeRetrieveMultiple, IEnumerable<T> fakedValue) where T : Entity
        {
            WithFakeRetrieveMultiple((s, qb) => shouldFakeRetrieveMultiple(s, qb) ? new EntityCollection(fakedValue.ToList<Entity>()) : s.RetrieveMultiple(qb)); return This;
        }
        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple to a boolean return value and the value to return.
        /// </summary>
        /// <param name="shouldFakeRetrieveMultiple"></param>
        /// <param name="fakedValue"></param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultiple<T>(Func<IOrganizationService, QueryBase, bool> shouldFakeRetrieveMultiple, params T[] fakedValue) where T : Entity
        {
            WithFakeRetrieveMultiple((s, qb) => shouldFakeRetrieveMultiple(s, qb) ? new EntityCollection(fakedValue.ToList<Entity>()) : s.RetrieveMultiple(qb)); return This;
        }

        #endregion WithFakeRetrieveMultiple

        #region WithFakeRetrieveMultipleForEntity

        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple by returning the given collection for the given logical name.
        /// </summary>
        /// <param name="logicalName">The logical name of the entity to fake the RetrieveMultiple for.</param>
        /// <param name="fakedValue">The value to return when the query is for the given entity type.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultipleForEntity(string logicalName, EntityCollection fakedValue)
        {
            return WithFakeRetrieveMultiple((s, qb) => 
                qb is QueryExpression qe && qe.EntityName == logicalName
                || qb is FetchExpression fe && fe.GetEntityName() == logicalName 
                    ? fakedValue 
                    : s.RetrieveMultiple(qb));
        }

        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple by returning the given collection for the given logical name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fakedValue">The entities to return when the query is for the given entity type.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultipleForEntity<T>(IEnumerable<T> fakedValue) where T : Entity
        {
            var logicalName = EntityHelper.GetEntityLogicalName<T>();
            if (logicalName == "entity")
            {
                throw new NotSupportedException("Type Parameter \"Entity\" is not supported.");
            }

            return WithFakeRetrieveMultipleForEntity(logicalName, new EntityCollection(fakedValue.ToList<Entity>()));
        }

        /// <summary>
        /// Simplifies the WithFakeRetrieveMultiple by returning the given collection for the given logical name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fakedValue">The entities to return when the query is for the given entity type.</param>
        /// <returns></returns>
        public TDerived WithFakeRetrieveMultipleForEntity<T>(params T[] fakedValue) where T : Entity
        {
            return WithFakeRetrieveMultipleForEntity((IEnumerable<T>)fakedValue);
        }

        #endregion WithFakeRetrieveMultipleForEntity

        #region WithFakeSetStatusForEntity

        /// <summary>
        /// Performs custom function for SetState of entity
        /// </summary>
        /// <param name="entityToFakeSetStateFor">The entity to fake set state for.</param>
        /// <param name="setStateAction">The set state function.</param>
        /// <returns></returns>
        public TDerived WithFakeSetStateForEntity(EntityReference entityToFakeSetStateFor, Action<SetStateRequest>? setStateAction = null)
        {
            WithFakeExecute((s, r) =>
            {
                if (r is not SetStateRequest setState || !setState.EntityMoniker.Equals(entityToFakeSetStateFor))
                {
                    return s.Execute(r);
                }
                setStateAction?.Invoke(setState);
                return new SetStateResponse();
            });
            return This;
        }

        #endregion WithFakeSetStatusForEntity

        #region WithFakeUpdateForEntity

        /// <summary>
        /// Performs custom action for update, rather than default update
        /// </summary>
        /// <param name="entityToMock">The entity to mock.</param>
        /// <param name="action">The action.</param>
        public TDerived WithFakeUpdateForEntity(EntityReference entityToMock, Action<Entity>? action = null)
        {
            WithFakeUpdate((s, e) =>
            {
                if (e.LogicalName == entityToMock.LogicalName && e.Id == entityToMock.Id)
                {
                    action?.Invoke(e);
                }
                else
                {
                    s.Update(e);
                }
            });

            return This;
        }

        #endregion WithFakeUpdateForEntity

        /// <summary>
        /// Defaults the entity name of all created entities.
        /// </summary>
        /// <param name="getName">function to call to get the name for the given Entity, and it's Primary Field Info.</param>
        /// <param name="config">Entity Help settings to define primary attributes</param>
        /// <returns></returns>
        public TDerived WithEntityNameDefaulted(Func<Entity, PrimaryFieldInfo, string> getName, IEntityHelperConfig? config = null)
        {
            CreateFuncs.Add((s, e) =>
            {
                var logicalName = e.LogicalName;
                if (!string.IsNullOrWhiteSpace(logicalName))
                {
                    var info = GetPrimaryFieldInfo(logicalName, config);

                    SetName(e, info, getName);
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
        /// <param name="ignoreContextCreation">Since OrganizationServiceContext.SaveChanges will populate ids if they aren't already populated, controls whether an error should be thrown when this occurs.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithIdsDefaultedForCreate(bool ignoreContextCreation, params Id[] ids)
        {
            WithIdsDefaultedForCreate(ids, ignoreContextCreation);

            return This;
        }

        /// <summary>
        /// When an entity is attempted to be created without an id, and an Id was passed in for that particular entity type, the Guid of the Id will be used to populate the entity
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="ignoreContextCreation">Since OrganizationServiceContext.SaveChanges will populate ids if they aren't already populated, controls whether an error should be thrown when this occurs.</param>
        /// <returns></returns>
        public TDerived WithIdsDefaultedForCreate(IEnumerable<Id> ids, bool ignoreContextCreation = false)
        {
            if (ignoreContextCreation)
            {
                NewEntityThrowErrorOnContextCreation = false;
            }
            foreach (var id in ids)
            {
                NewEntityDefaultIds.AddOrEnqueue(id);
            }

            return This;
        }

        #endregion WithIdsDefaultedForCreate

        /// <summary>
        /// Fakes out calls to RetrieveAttribute Requests, using enums to generate the OptionSetMetaData.  Useful for mocking out any calls to determine the text values of an optionset.
        /// </summary>
        /// <param name="defaultLanguageCode">The default language code.  Defaults to reading DefaultLanguageCode from the config, or 1033 if not found</param>
        /// <returns></returns>
        public TDerived WithLocalOptionSetsRetrievedFromEnum(int? defaultLanguageCode = null)
        {
            defaultLanguageCode = defaultLanguageCode ?? DLaB.Xrm.Client.AppConfig.DefaultLanguageCode;
            ExecuteFuncs.Add((s, r) =>
            {
                if (!(r is RetrieveAttributeRequest attRequest))
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
                    t.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0 &&
                    t.GetCustomAttributes(typeof(GeneratedCodeAttribute), false).Length > 0);

                // Return EntityLogicalName_Logical Name first
                var enumType = enumExpression.OrderBy(t => t.Name != attRequest.EntityLogicalName + "_" + attRequest.LogicalName).FirstOrDefault();

                if (enumType == null)
                {
                    throw new Exception($"Unable to find local optionset enum for entity: {attRequest.EntityLogicalName}, attribute: {attRequest.LogicalName}");
                }

                foreach (var value in Enum.GetValues(enumType))
                {
                    optionSet.OptionSet.Options.Add(
                        new OptionMetadata
                        {
                            Value = (int) value,
                            Label = new Label
                            {
                                UserLocalizedLabel = new LocalizedLabel(value.ToString(), defaultLanguageCode.Value)
                            }
                        });
                }

                return response;
            });
            return This;
        }

        /// <summary>
        /// Causes the Organization Service to throw an exception if an attempt is made to create an entity
        /// </summary>
        /// <returns></returns>
        public TDerived WithNoCreates()
        {
            CreateFuncs.Add((_, e) =>
            {
                ReadOnlyFail.OnCreate(e);
                throw new Exception("AssertFail Failed to throw an Exception");
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
                if (!(q is QueryExpression qe) || !entities.ContainsKey(qe.EntityName))
                {
                    return s.RetrieveMultiple(q);
                }

                return new EntityCollection(entities[qe.EntityName]);
            });

            RetrieveFuncs.Add((s, name, id, cs) =>
            {
                if (entities.TryGetValue(name, out var list))
                {
                    var entity = list.FirstOrDefault(e => e.Id == id);
                    if (entity != null)
                    {
                        return entity;
                    }
                }
                return s.Retrieve(name, id, cs);

            });

            return This;
        }

        /// <summary>
        /// Specifies that the returned entities are returned whenever the given entity type is queried for.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="qb">The qb.</param>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public static EntityCollection WithReturnedEntities(IOrganizationService s, QueryBase qb, string logicalName, params Entity[] entities)
        {
            return s.MockOrDefault(qb, q => q.EntityName == logicalName, entities);
        }

        #endregion WithReturnedEntities

        /// <summary>
        /// Fakes RetrieveMultiples that are requesting a WebResource, to read the file from the given path.  Useful when settings are stored in a config web resource, and so can be tested with local modifications.
        /// </summary>
        /// <param name="webResourceName">Name of the web resource.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public TDerived WithWebResourcePulledFromPath(string webResourceName, string? path = null)
        {
            RetrieveMultipleFuncs.Add((s, q) =>
            {
                if (!(q is QueryExpression qe) || qe.EntityName != WebResource.EntityLogicalName || !s.IsLocalCrmService())
                {
                    return s.RetrieveMultiple(q);
                }

                var condition = qe.Criteria.Conditions.FirstOrDefault(c => c.AttributeName == "name" && c.Values is { Count: 1 } && c.Values[0]?.ToString().Contains(webResourceName) == true);
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

                if (!File.Exists(path))
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

        #endregion Fleunt Methods

        /// <summary>
        /// Builds this IOrganizationService.
        /// </summary>
        /// <returns></returns>
#if NET
        public IOrganizationServiceAsync2 Build()
#else
        public IOrganizationService Build()
#endif
        {
            return Build(default);
        }

        /// <summary>
        /// Builds this IOrganizationService utilizing the build config
        /// </summary>
        /// <returns></returns>
#if NET
        public virtual IOrganizationServiceAsync2 Build(TBuildConfig? config)
#else
        public virtual IOrganizationService Build(TBuildConfig? config)
#endif
        {
            config ??= new TBuildConfig();
            ApplyNewEntityDefaultIds(config);
            ApplyEntityFilter(config);

            var service = Service;
            if (PrimaryBuilder != null)
            {
                if (PrimaryBuilder.Build() is FakeIOrganizationService primaryService){
                    primaryService.InsertAt(0);
                    if (!primaryService.HasWrappingService)
                    {
                        // There were no child services, so use the primary service as the service to wrap
                        service = primaryService;
                    }
                }
                else
                {
                    throw new Exception($"Unable to cast Primary Service to {nameof(FakeIOrganizationService)}.");
                }
            }

            return new FakeIOrganizationService(service)
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

        private void ApplyEntityFilter(TBuildConfig config)
        {
            if (!EntityFilter.Any())
            {
                return;
            }

            var builder = (TDerived)this;
            if (config.UsePrimaryBuilderForEntityFilter)
            {
                PrimaryBuilder ??= CreateBuilder(Service);
                builder = PrimaryBuilder;
            }

            builder ??= CreateBuilder(Service);

            builder.RetrieveMultipleFuncs.Add((s, q) =>
            {
                if (q is not QueryExpression qe)
                {
                    return s.RetrieveMultiple(q);
                }

                foreach (var entityGroup in EntityFilter)
                {
                    var entityType = EntityHelper.GetType(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, entityGroup.Key);
                    var idLogicalName = EntityHelper.GetIdAttributeName(entityType);
                    foreach (var filter in qe.GetEntityFilters(entityGroup.Key))
                    {
                        filter.AddConditionEnforceAndFilterOperator(new ConditionExpression(idLogicalName, ConditionOperator.In, entityGroup.Value.Select(i => (object)i).ToArray()));
                    }
                }
                return s.RetrieveMultiple(q);
            });
        }

        private void ApplyNewEntityDefaultIds(TBuildConfig config)
        {
            if (!NewEntityDefaultIds.Any())
            {
                return;
            }

            var builder = (TDerived)this;
            if (config.UsePrimaryBuilderForNewEntityDefaultIds)
            {
                PrimaryBuilder ??= CreateBuilder(Service);
                builder = PrimaryBuilder;
            }

            builder.CreateFuncs.Add((s, e) =>
            {
                DefaultIdForEntity(e);
                return s.Create(e);
            });
            builder.ExecuteFuncs.Add((s, r) =>
            {
                DefaultIdsForExecuteRequests(r, s);
                return s.Execute(r);
            });
        }

        /// <summary>
        /// Creates the derived builder
        /// </summary>
        /// <param name="parentService">The Parent Service</param>
        protected virtual TDerived CreateBuilder(IOrganizationService parentService)
        {
            var constructor = typeof(TDerived).GetConstructor([typeof(IOrganizationService)]);
            if (constructor != null)
            {
                return (TDerived)constructor.Invoke([parentService]);
            }

            constructor = typeof(TDerived).GetConstructor([]);
            if (constructor == null)
            {
                throw new Exception($"Constructor for {typeof(TDerived).FullName} must have either an empty constructor, or a constructor with an IOrganizationService parameter, or the {nameof(CreateBuilder)} method must be overriden");
            }
            return (TDerived)constructor.Invoke([]);
            
        }

        private void DefaultIdsForExecuteRequests(OrganizationRequest r, IOrganizationService s)
        {
            if (r is SendEmailFromTemplateRequest email)
            {
                DefaultIdForEntity(email.Target);
            }
            else
            {
                ApplyNewEntityDefaultIdsForUpsert(r, s);
                ApplyNewEntityDefaultIdsForExecuteMultiple(r, s);
                ApplyNewEntityDefaultIdsForExecuteTransaction(r, s);
            }
        }

        private void ApplyNewEntityDefaultIdsForUpsert(OrganizationRequest r, IOrganizationService s)
        {
#if !PRE_KEYATTRIBUTE
            if (!(r is UpsertRequest upsert))
            {
                return;
            }

            var target = upsert.Target;
            if (target.Id == Guid.Empty)
            {
                if (target.KeyAttributes?.Count > 0)
                {
                    var entityByKvp = s.GetEntityOrDefault(target.LogicalName, target.KeyAttributes, new ColumnSet(false));
                    
                    if (entityByKvp == null)
                    {
                        DefaultIdForEntity(upsert.Target);
                    }
                    else
                    {
                        target.Id = entityByKvp.Id;
                    }
                }
                else
                {
                    DefaultIdForEntity(upsert.Target);
                }
            }
#endif
        }

        private void ApplyNewEntityDefaultIdsForExecuteMultiple(OrganizationRequest r, IOrganizationService s)
        {
            if (r is ExecuteMultipleRequest multipleRequest)
            {
                ApplyNewEntityDefaultIdsForRequests(s, multipleRequest.Requests);
            }
        }



        private void ApplyNewEntityDefaultIdsForExecuteTransaction(OrganizationRequest r, IOrganizationService s)
        {
#if !PRE_KEYATTRIBUTE
            if (r is ExecuteTransactionRequest transaction)
            {
                ApplyNewEntityDefaultIdsForRequests(s, transaction.Requests);
            }
#endif
        }

        private void ApplyNewEntityDefaultIdsForRequests(IOrganizationService s, OrganizationRequestCollection requests)
        {
            foreach (var request in requests)
            {
                if (request is CreateRequest create)
                {
                    DefaultIdForEntity(create.Target);
                }
                else
                {
                    DefaultIdsForExecuteRequests(request, s);
                }
            }
        }

        /// <summary>
        /// Defaults the id an any entity created using the Dictionary string, without an already defined Guid, to the default Id.  Doesn't actually create the Entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void DefaultIdForEntity(Entity entity)
        {
            // throw an error if from context, and NewEntityThrowErrorOnContextCreation is true, and not in the ids collection
            if (entity.Id != Guid.Empty)
            {
                if (entity.EntityState != EntityState.Created)
                {
                    return;
                }

                var hasIds = NewEntityDefaultIds.TryGetValue(entity.LogicalName, out var ids);

                if (NewEntityThrowErrorOnContextCreation)
                {
                    if (!hasIds || !ContainsAndRemoved(ids!, entity.Id))
                    {
                        throw new Exception(
                            $"An attempt was made to create an entity of type {entity.LogicalName} with the EntityState set to created which normally means it comes from an OrganizationServiceContext.SaveChanges call.{Environment.NewLine}"
                            +
                            "Either set ignoreContextCreation to true on the WithIdsDefaultedForCreate call, or define the id before calling SaveChanges, and add the id with the WithIdsDefaultedForCreate method.");
                    }
                }
                else if(hasIds)
                {
                    ContainsAndRemoved(ids!, entity.Id);
                }
            }
            else
            {
                if(!NewEntityDefaultIds.TryGetValue(entity.LogicalName, out var ids))
                {
                    return;
                }
                if (ids.Count == 0)
                {
                    throw new Exception(
                        $"An attempt was made to create an entity of type {entity.LogicalName}, but no id exists in the NewEntityDefaultIds Collection for it.{Environment.NewLine}" +
                        "Either the entity's Id was not populated as a part of initialization, or a call is needs to be added to to OrganizationServiceBuilder.WithIdsDefaultedForCreate(id)");
                }
                entity.Id = ids.Dequeue();
            }
        }

        private bool ContainsAndRemoved(Queue<Guid> ids, Guid id)
        {
            if (!ids.ToArray().Contains(id))
            {
                return false;
            }
            var subQueue = new Queue<Guid>(ids.Count);
            while (ids.Count > 0)
            {
                var current = ids.Dequeue();
                if (id == current)
                {
                    break;
                }
                subQueue.Enqueue(current);
            }

            while (subQueue.Count > 0)
            {
                ids.Enqueue(subQueue.Dequeue());
            }

            return true;
        }

        /// <summary>
        /// Gets the primary field information.
        /// </summary>
        /// <param name="logicalName">Logical name of the entity.</param>
        /// <param name="config">Entity Help settings to define primary attributes</param>
        /// <returns></returns>
        public virtual PrimaryFieldInfo GetPrimaryFieldInfo(string logicalName, IEntityHelperConfig? config = null)
        {
            return EntityHelper.GetPrimaryFieldInfo(logicalName, config ?? new PrimaryNameProviderConfig());
        }

        private static void SetName(Entity e, PrimaryFieldInfo info, Func<Entity, PrimaryFieldInfo, string> getName)
        {
            if (string.IsNullOrWhiteSpace(info.AttributeName)
                || info.IsAttributeOf
                || (info.ReadOnly && info.BaseAttributes.Count == 0))
            {
                return;
            }

            var name = getName(e, info).PadRight(info.MaximumLength).Substring(0, info.MaximumLength).TrimEnd();
            if (info.ReadOnly)
            {
                if (info.BaseAttributes.Count == 1)
                {

                    e[info.BaseAttributes[0]] = name;
                }

                // Split name amoungst first two attributes.  If odd, subtract 1 to have equal lengths
                var length = name.Length % 2 == 0 ? name.Length : name.Length - 1;
                length /= 2;
                SetIfNotDefined(e, info.BaseAttributes[0], name.Substring(0, length));
                SetIfNotDefined(e, info.BaseAttributes[1], name.Substring(length, length));
            }
            else
            {
                SetIfNotDefined(e, info.AttributeName!, name);
            }
        }

        private static void SetIfNotDefined(Entity e, string attributeName, object value)
        {
            if (e.Attributes.ContainsKey(attributeName))
            {
                return;
            }
            e[attributeName] = value;
        }

        IAgnosticServiceBuilder IAgnosticServiceBuilder.WithDefaultParentBu()
        {
            return WithDefaultParentBu();
        }

        IAgnosticServiceBuilder IAgnosticServiceBuilder.WithEntityNameDefaulted(Func<Entity, PrimaryFieldInfo, string> getName)
        {
            return WithEntityNameDefaulted(getName);
        }

        IAgnosticServiceBuilder IAgnosticServiceBuilder.AssertIdNonEmptyOnCreate()
        {
            return AssertIdNonEmptyOnCreate();
        }

        IOrganizationService IAgnosticServiceBuilder.Build()
        {
            return Build();
        }

        private class PrimaryNameProviderConfig : IEntityHelperConfig
        {
            public string? GetIrregularIdAttributeName(string logicalName)
            {
                return null;
            }

            public PrimaryFieldInfo GetIrregularPrimaryFieldInfo(string logicalName, PrimaryFieldInfo? defaultInfo = null)
            {
                defaultInfo = defaultInfo ?? new PrimaryFieldInfo();
                defaultInfo.AttributeName = PrimaryNameFieldProviderBase.GetConfiguredProvider(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace).GetPrimaryName(logicalName);
                return defaultInfo;
            }
        }
    }
}
