using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using DLaB.Common;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
#if NET
using DLaB.Xrm;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Threading;
using System.Threading.Tasks;

namespace DataverseUnitTest
#else

namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Class that wraps an IOrganizationService under the covers, but allows for injection of
    /// different code for faking data in the database.
    ///  
    /// Example Use Case that will return a new empty system user for the QueryExpression that is for System Users
    /// else, executes the query and returns the results as normal:                
    /// using (var proxy = GetOrganizationServiceProxy())
    /// {
    ///     var service = new FakeIOrganizationService(proxy)
    ///     {
    ///         RetrieveMultipleFunc = (s, qb) => { 
    ///             if(((qb) as QueryExpression).EntityName == SystemUser.EntityLogicalName)
    ///             {
    ///                 var c = new EntityCollection();
    ///                 c.Entities.Add(new SystemUser());
    ///                 return c;
    ///             }
    ///             else
    ///             {
    ///                 return s.RetrieveMultiple(qb);    
    ///             }
    ///         }
    ///     };
    /// }
    /// </summary>
#if !DEBUG_XRM_UNIT_TEST_CODE
    [DebuggerNonUserCode]
#endif
#if NET
    public class FakeIOrganizationService : ClientSideOrganizationService, IServiceFaked<IOrganizationService>, IFakeService, IOrganizationServiceAsync2
#else
    public class FakeIOrganizationService : ClientSideOrganizationService, IServiceFaked<IOrganizationService>, IFakeService
#endif
    {
        private Func<IOrganizationService, Entity, Guid>[]? _createFuncs;
        private Action<IOrganizationService, string, Guid>[]? _deleteActions;
        private Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[]? _associateActions;
        private Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[]? _disassociateActions;
        private Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[]? _executeFuncs;
        private Func<IOrganizationService, QueryBase, EntityCollection>[]? _retrieveMultipleFuncs;
        private Func<IOrganizationService, string, Guid, ColumnSet, Entity>[]? _retrieveFuncs;
        private Action<IOrganizationService, Entity>[]? _updateActions;

        #region IOrganizationService Mocks

        /// <summary>
        /// Gets or sets the associate action.
        /// </summary>
        /// <value>
        /// The associate action.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>? AssociateAction { get; set; }

        /// <summary>
        /// Gets or sets the associate actions.
        /// </summary>
        /// <value>
        /// The associate actions.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[]? AssociateActions
        {
            get => _associateActions;
            set
            {
                _associateActions = value;
                AssociateCache = null;
            }
        }

        private IOrganizationService? AssociateCache { get; set; }
        /// <summary>
        /// Gets or sets the create function.
        /// </summary>
        /// <value>
        /// The create function.
        /// </value>
        public Func<IOrganizationService, Entity, Guid>? CreateFunc { get; set; }

        /// <summary>
        /// Gets or sets the create funcs.
        /// </summary>
        /// <value>
        /// The create funcs.
        /// </value>
        public Func<IOrganizationService, Entity, Guid>[]? CreateFuncs
        {
            get => _createFuncs;
            set {
                _createFuncs = value;
                CreateCache = null;
            }
        }

        private IOrganizationService? CreateCache { get; set; }
        /// <summary>
        /// Gets or sets the delete action.
        /// </summary>
        /// <value>
        /// The delete action.
        /// </value>
        public Action<IOrganizationService, string, Guid>? DeleteAction { get; set; }

        /// <summary>
        /// Gets or sets the delete actions.
        /// </summary>
        /// <value>
        /// The delete actions.
        /// </value>
        public Action<IOrganizationService, string, Guid>[]? DeleteActions
        {
            get => _deleteActions;
            set
            {
                _deleteActions = value;
                DeleteCache = null;
            }
        }

        private IOrganizationService? DeleteCache { get; set; }
        /// <summary>
        /// Gets or sets the disassociate action.
        /// </summary>
        /// <value>
        /// The disassociate action.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>? DisassociateAction { get; set; }

        /// <summary>
        /// Gets or sets the disassociate actions.
        /// </summary>
        /// <value>
        /// The disassociate actions.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[]? DisassociateActions
        {
            get => _disassociateActions;
            set
            {
                _disassociateActions = value;
                DisassociateCache = null;
            }
        }

        private IOrganizationService? DisassociateCache { get; set; }
        /// <summary>
        /// Gets or sets the execute function.
        /// </summary>
        /// <value>
        /// The execute function.
        /// </value>
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse>? ExecuteFunc { get; set; }

        /// <summary>
        /// Gets or sets the execute funcs.
        /// </summary>
        /// <value>
        /// The execute funcs.
        /// </value>
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[]? ExecuteFuncs
        {
            get => _executeFuncs;
            set
            {
                _executeFuncs = value;
                ExecuteCache = null;
            }
        }

        private IOrganizationService? ExecuteCache { get; set; }
        /// <summary>
        /// Gets or sets the retrieve multiple function.
        /// </summary>
        /// <value>
        /// The retrieve multiple function.
        /// </value>
        public Func<IOrganizationService, QueryBase, EntityCollection>? RetrieveMultipleFunc { get; set; }

        /// <summary>
        /// Gets or sets the retrieve multiple funcs.
        /// </summary>
        /// <value>
        /// The retrieve multiple funcs.
        /// </value>
        public Func<IOrganizationService, QueryBase, EntityCollection>[]? RetrieveMultipleFuncs
        {
            get => _retrieveMultipleFuncs;
            set
            {
                _retrieveMultipleFuncs = value;
                RetrieveMultipleCache = null;
            }
        }

        private IOrganizationService? RetrieveMultipleCache { get; set; }
        /// <summary>
        /// Gets or sets the retrieve function.
        /// </summary>
        /// <value>
        /// The retrieve function.
        /// </value>
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity>? RetrieveFunc { get; set; }

        /// <summary>
        /// Gets or sets the retrieve funcs.
        /// </summary>
        /// <value>
        /// The retrieve funcs.
        /// </value>
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity>[]? RetrieveFuncs
        {
            get => _retrieveFuncs;
            set
            {
                _retrieveFuncs = value;
                RetrieveCache = null;
            }
        }

        private IOrganizationService? RetrieveCache { get; set; }
        /// <summary>
        /// Gets or sets the update action.
        /// </summary>
        /// <value>
        /// The update action.
        /// </value>
        public Action<IOrganizationService, Entity>? UpdateAction { get; set; }

        /// <summary>
        /// Gets or sets the update actions.
        /// </summary>
        /// <value>
        /// The update actions.
        /// </value>
        public Action<IOrganizationService, Entity>[]? UpdateActions
        {
            get => _updateActions;
            set
            {
                _updateActions = value;
                UpdateCache = null;
            }
        }

        private IOrganizationService? UpdateCache { get; set; }

        #endregion IOrganizationService Mocks

        /// <summary>
        /// Determines if the FakeIOrganizationService can be rearranged via an insert call.  If it can't, all other nested FakIOrganizationServices are disallowed from another service being inserted before itself.
        /// </summary>
        private bool AllowRearrangeViaInsert { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether [execution tracing enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [execution tracing enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ExecutionTracingEnabled => Timer != null;
        private TestActionTimer? Timer { get; }
        /// <summary>
        /// Changes calls to Execute with a RetrieveMultipleRequest to be a RetrieveMultiple Call
        /// </summary>
        /// <value>
        /// <c>true</c> if [redirect execute retrieve multiple to retrieve multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool RedirectExecuteRequestsToOrganizationServiceRequest { get; set; }

        /// <summary>
        /// Recursively walks the Service Properties to find the first IOrganizationService that is not a FakeIOrganizationService
        /// Returns the highest level FakeIOrganizationService if every Service is of type FakeIOrganizationService
        /// </summary>
        public IOrganizationService ActualService => GetActualService();


        /// <summary>
        /// Recursively walks the Service Properties to find the first IOrganizationService that is a FakeIOrganizationService
        /// Sames as ActualService if the ActualService is a FakeIOrganizationService
        /// </summary>
        public FakeIOrganizationService PrimaryFakeService => GetPrimaryFakeService();

        /// <summary>
        /// The wrapping service, i.e. the Parent Service
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        protected FakeIOrganizationService? WrappingService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeIOrganizationService" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public FakeIOrganizationService(IOrganizationService service): this (service, true) { }

        /// <summary>
        /// Used when creating "on-the-fly" FakeIOrganizationServices that don't allow for Insertion logic.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="setWrappingService"></param>
        protected FakeIOrganizationService(IOrganizationService service, bool setWrappingService)
            : base(service)
        {
            if (service is FakeIOrganizationService fake)
            {
                Timer = fake.Timer;
                if (setWrappingService)
                {
                    if (fake.WrappingService != null)
                    {
                        throw new Exception("FakeIOrganizationService can not be wrapped by more than one service since this can cause issues with Insert functionality.");
                    }
                    fake.WrappingService = this;
                }
            }

            RedirectExecuteRequestsToOrganizationServiceRequest = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeIOrganizationService" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="logger">The logger.</param>
        public FakeIOrganizationService(IOrganizationService service, ITestLogger logger)
            : base(service)
        {
            Timer = new TestActionTimer(logger);
            RedirectExecuteRequestsToOrganizationServiceRequest = true;
        }

        private IOrganizationService GetActualService()
        {
            var service = Service;
            while (service is FakeIOrganizationService fakeService)
            {
                service = fakeService.Service;
            }
            return service;
        }

        private FakeIOrganizationService GetPrimaryFakeService()
        {
            var service = Service;
            var finalFake = this;
            while (service is FakeIOrganizationService fakeService)
            {
                finalFake = fakeService;
                service = fakeService.Service;
            }
            return finalFake;
        }

        #region IOrganizationService Members

        /// <summary>
        /// Creates a link between records.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (AssociateAction != null)
            {
                AssociateAction(Service, entityName, entityId, relationship, relatedEntities);
            }
            else if (AssociateActions != null)
            {
                if (AssociateCache == null)
                {
                    AssociateCache = Service;
                    foreach (var action in AssociateActions)
                    {
                        AssociateCache = new FakeIOrganizationService(AssociateCache, false) { AssociateAction = action };
                    }
                }
                AssociateCache.Associate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer!.Time(AssociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Associate {0}:{1} using relationship {2} to related Entities {3}: {4}",
                        entityName, entityId, relationship, relatedEntities.Select(e => e.GetNameId()).ToCsv());
                }
                else
                {
                    Service.Associate(entityName, entityId, relationship, relatedEntities);
                }
            }
        }

#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerHidden]
#endif
        private object? AssociateInternal(Tuple<string, Guid, Relationship, EntityReferenceCollection> value)
        {
            Service.Associate(value.Item1, value.Item2, value.Item3, value.Item4);
            return null;
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override Guid Create(Entity entity)
        {
            Guid id;
            if (CreateFunc != null)
            {
                id = CreateFunc(Service, entity);
            }
            else if (CreateFuncs != null)
            {
                if (CreateCache == null)
                {
                    CreateCache = Service;
                    foreach (var create in CreateFuncs)
                    {
                        CreateCache = new FakeIOrganizationService(CreateCache, false) { CreateFunc = create };
                    }
                }
                id = CreateCache.Create(entity);
            }
            else
            {
                id = ExecutionTracingEnabled ? Timer!.Time(CreateInternal, entity, "Create {0}: {1}", entity.GetNameId()) : Service.Create(entity);
            }
            return id;
        }

        [DebuggerHidden]
        private Guid CreateInternal(Entity entity)
        {
            return Service.Create(entity);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override void Delete(string entityName, Guid id)
        {
            if (DeleteAction != null)
            {
                DeleteAction(Service, entityName, id);
            }
            else if (DeleteActions != null)
            {
                if (DeleteCache == null)
                {
                    DeleteCache = Service;
                    foreach (var delete in DeleteActions)
                    {
                        DeleteCache = new FakeIOrganizationService(DeleteCache, false) { DeleteAction = delete };
                    }
                }
                DeleteCache.Delete(entityName, id);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer!.Time(DeleteInternal, new Tuple<string, Guid>(entityName, id), "Delete {0}({1}): {2}", entityName, id);
                }
                else
                {
                    Service.Delete(entityName, id);
                }
            }
        }

#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerHidden]
#endif
        private object? DeleteInternal(Tuple<string, Guid> value)
        {
            Service.Delete(value.Item1, value.Item2);
            return null;
        }

        /// <summary>
        /// Removes a link between records.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (DisassociateAction != null)
            {
                DisassociateAction(Service, entityName, entityId, relationship, relatedEntities);
            }
            else if (DisassociateActions != null)
            {
                if (DisassociateCache == null)
                {
                    DisassociateCache = Service;
                    foreach (var disassociate in DisassociateActions)
                    {
                        DisassociateCache = new FakeIOrganizationService(DisassociateCache, false) { DisassociateAction = disassociate };
                    }
                }
                DisassociateCache.Disassociate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer!.Time(DisassociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Disassociate {0}:{1} using relationship {2} to related Entities {3}: {4}",
                        entityName, entityId, relationship, relatedEntities.Select(e => e.GetNameId()).ToCsv());
                }
                else
                {
                    Service.Disassociate(entityName, entityId, relationship, relatedEntities);
                }
            }
        }

#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerHidden]
#endif
        private object? DisassociateInternal(Tuple<string, Guid, Relationship, EntityReferenceCollection> value)
        {
            Service.Disassociate(value.Item1, value.Item2, value.Item3, value.Item4);
            return null;
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override OrganizationResponse Execute(OrganizationRequest request)
        {
            OrganizationResponse? response;
            if (RedirectExecuteRequestsToOrganizationServiceRequest)
            {
                response = CallOrganizationServiceRequestForExecuteRequest(request);

                if (response != null)
                {
                    return response;
                }
            }

            if (ExecuteFunc != null)
            {
                response = ExecuteFunc(Service, request);
            }
            else if (ExecuteFuncs != null)
            {
                if (ExecuteCache == null)
                {
                    ExecuteCache = Service;
                    foreach (var execute in ExecuteFuncs)
                    {
                        ExecuteCache = new FakeIOrganizationService(ExecuteCache, false) { ExecuteFunc = execute };
                    }
                }
                response = ExecuteCache.Execute(request);
            }
            else
            {
                response = ExecutionTracingEnabled ? Timer!.Time(ExecuteInternal, request, "Execute {0}: {1}", request.RequestName) : Service.Execute(request);
            }
            return response;
        }

        /// <summary>
        /// Rather than having to define mocks separately for both RetrieveRequest and Retrieve, this will map the call to the correct OrganizationService Method.
        /// </summary>
        /// <param name="request">The request.</param>
        [DebuggerHidden]
        private OrganizationResponse? CallOrganizationServiceRequestForExecuteRequest(OrganizationRequest request)
        {
            OrganizationResponse? response = null;
            if (request is AssociateRequest associate)
            {
                response = new AssociateResponse();
                Associate(associate.Target.LogicalName, associate.Target.Id, associate.Relationship, associate.RelatedEntities);
            }

            if (request is CreateRequest create)
            {
                response = new CreateResponse { ["id"] = Create(create.Target) };
            }

            if (request is DeleteRequest delete)
            {
                response = new DeleteResponse();
                Delete(delete.Target.LogicalName, delete.Target.Id);
            }

            if (request is DisassociateRequest disassociate)
            {
                response = new AssociateResponse();
                Disassociate(disassociate.Target.LogicalName, disassociate.Target.Id, disassociate.Relationship, disassociate.RelatedEntities);
            }

            if (request is RetrieveRequest retrieve)
            {
                var target = retrieve.Target;
                if (target.Id == Guid.Empty && target.KeyAttributes.Count > 0)
                {
                    var original = target;
                    target = this.GetEntityOrDefault(target.LogicalName, target.KeyAttributes, retrieve.ColumnSet)?.ToEntityReference() ?? original;
                }
                response = new RetrieveResponse { ["Entity"] = Retrieve(target.LogicalName, target.Id, retrieve.ColumnSet) };
            }

            if (request is RetrieveMultipleRequest retrieveMultiple)
            {
                response = new RetrieveMultipleResponse { ["EntityCollection"] = RetrieveMultiple(retrieveMultiple.Query) };
            }

            if (request is UpdateRequest update)
            {
                response = new UpdateResponse();
                Update(update.Target);
            }
            return response;
        }


        [DebuggerHidden]
        private OrganizationResponse ExecuteInternal(OrganizationRequest request)
        {
            return Service.Execute(request);
        }

        /// <summary>
        /// Retrieves the specified entity name.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="columnSet">The column set.</param>
        /// <returns></returns>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            Entity entity;
            if (RetrieveFunc != null)
            {
                entity = RetrieveFunc(Service, entityName, id, columnSet);
            }
            else if (RetrieveFuncs != null)
            {
                if (RetrieveCache == null)
                {
                    RetrieveCache = Service;
                    foreach (var retrieve in RetrieveFuncs)
                    {
                        RetrieveCache = new FakeIOrganizationService(RetrieveCache, false) { RetrieveFunc = retrieve };
                    }
                }
                entity = RetrieveCache.Retrieve(entityName, id, columnSet);
            }
            else
            {
                entity = ExecutionTracingEnabled ? Timer!.Time(RetrieveInternal, new Tuple<string, Guid, ColumnSet>(entityName, id, columnSet), "Retrieve {0}({1}): {2}", entityName, id) : Service.Retrieve(entityName, id, columnSet);
            }
            return entity;
        }

        [DebuggerHidden]
        private Entity RetrieveInternal(Tuple<string, Guid, ColumnSet> value)
        {
            return Service.Retrieve(value.Item1, value.Item2, value.Item3);
        }

        /// <summary>
        /// Retrieves the entities defined by the Query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override EntityCollection RetrieveMultiple(QueryBase query)
        {
            EntityCollection entities;
            if (RetrieveMultipleFunc != null)
            {
                entities = RetrieveMultipleFunc(Service, query);
            }
            else if (RetrieveMultipleFuncs != null)
            {
                if (RetrieveMultipleCache == null)
                {
                    RetrieveMultipleCache = Service;
                    foreach (var retrieveMultiple in RetrieveMultipleFuncs)
                    {
                        RetrieveMultipleCache = new FakeIOrganizationService(RetrieveMultipleCache, false) { RetrieveMultipleFunc = retrieveMultiple };
                    }
                }
                entities = RetrieveMultipleCache.RetrieveMultiple(query);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    entities = query is QueryExpression qe
                        ? Timer!.Time(RetrieveMultipleInternal, query, "RetrieveMultiple: {2}{0}{1}", Environment.NewLine, qe.GetSqlStatement())
                        : Timer!.Time(RetrieveMultipleInternal, query, "RetrieveMultiple {0}: {1}", query);
                }
                else
                {
                    entities = Service.RetrieveMultiple(query);
                }
            }
            return entities;
        }

        [DebuggerHidden]
        private EntityCollection RetrieveMultipleInternal(QueryBase query)
        {
            return Service.RetrieveMultiple(query);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerNonUserCode]
#endif
        public override void Update(Entity entity)
        {
            if (UpdateAction != null)
            {
                UpdateAction(Service, entity);
            }
            else if (UpdateActions != null)
            {
                if (UpdateCache == null)
                {
                    UpdateCache = Service;
                    foreach (var update in UpdateActions)
                    {
                        UpdateCache = new FakeIOrganizationService(UpdateCache, false) { UpdateAction = update };
                    }
                }
                UpdateCache.Update(entity);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer!.Time(UpdateInternal, entity, "Update Entity {0}: {1}", entity.GetNameId());
                }
                else
                {
                    Service.Update(entity);
                }
            }
        }

#if !DEBUG_XRM_UNIT_TEST_CODE
        [DebuggerHidden]
#endif
        private object? UpdateInternal(Entity entity)
        {
            Service.Update(entity);
            return null;
        }

        #endregion

        #region IServiceFaked<IOrganizationService> Members
#if NET
        /// <inheritdoc/>
        public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return Task.Run(() => Associate(entityName, entityId, relationship, relatedEntities));
        }

        /// <inheritdoc/>
        public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
        {
            return Task.Run(() => Associate(entityName, entityId, relationship, relatedEntities), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
        {
            entity.Id = Create(entity);
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public Task<Guid> CreateAsync(Entity entity)
        {
            return Task.FromResult(Create(entity));
        }

        /// <inheritdoc/>
        public Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
        {
            return Task.FromResult(Create(entity));
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string entityName, Guid id)
        {
            return Task.Run(() => Delete(entityName, id));
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
        {
            return Task.Run(() => Delete(entityName, id), cancellationToken);
        }

        /// <inheritdoc/>
        public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return Task.Run(() => Disassociate(entityName, entityId, relationship, relatedEntities));
        }

        /// <inheritdoc/>
        public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => Disassociate(entityName, entityId, relationship, relatedEntities), cancellationToken);
        }


        /// <inheritdoc/>
        public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
        {
            return Task.FromResult(Execute(request));
        }

        /// <inheritdoc/>
        public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute(request));
        }

        /// <inheritdoc/>
        public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet)
        {
            return Task.FromResult(Retrieve(entityName, id, columnSet));
        }

        /// <inheritdoc/>
        public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
        {
            return Task.FromResult(Retrieve(entityName, id, columnSet));
        }

        /// <inheritdoc/>
        public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query)
        {
            return Task.FromResult(RetrieveMultiple(query));
        }

        /// <inheritdoc/>
        public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
        {
            return Task.FromResult(RetrieveMultiple(query));
        }


        /// <inheritdoc/>
        public Task UpdateAsync(Entity entity)
        {
            return Task.Run(() => Update(entity));
        }

        /// <inheritdoc/>
        public Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
        {
            return Task.Run(() => Update(entity), cancellationToken);
        }
#endif
        #endregion IServiceFaked<IOrganizationService> Members

        /// <summary>
        /// Recursively walks the Service Path to inject this FakeIOrganizationService at the specified level
        /// The 0 index is the first highest most Service Property that is still a FakeIOrganizationService, which is the ActualService if all are FakeIOrganizationServices
        /// This was created to allow for Fakes defined later in the hierarchy, to be used by fakes defined later.  For example, defining the Id assigned in a Create Request
        /// </summary>
        private void InsertAt(int levelIndex)
        {
            var parents = new List<FakeIOrganizationService>
            {
                this
            };
            var parent = Service;
            while (parent is FakeIOrganizationService { AllowRearrangeViaInsert: true } fake)
            {
                parents.Add(fake);
                parent = fake.Service;
            }
            parents.Reverse();

            var currentIndex = parents.IndexOf(this);
            if (levelIndex == currentIndex)
            {
                return;
            }

            if (levelIndex >= parents.Count)
            {
                throw new IndexOutOfRangeException($"The requested injection index of {levelIndex} is outside the hierarchy count of {parents.Count}");
            }
            
            var current = parents[levelIndex];

            // First, remove this by pointing the Wrapping service to the Service, and the Service to the Wrapping Service
            if (WrappingService != null)
            {
                WrappingService.Service = Service;
            }
            if (Service is FakeIOrganizationService fakeEnd)
            {
                fakeEnd.WrappingService = WrappingService;
            }

            // Second, Insert this into the new spot
            Service = current.Service;
            if (Service is FakeIOrganizationService fakeStart)
            {
                fakeStart.WrappingService = this;
            }
            current.Service = this;
            WrappingService = current;
        }
    }
}