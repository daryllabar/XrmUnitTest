using System;
using System.Linq;
using System.Diagnostics;
using DLaB.Common;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest
#else

namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Class that uses a real OrganizationServiceProxy under the covers, but allows for injection of
    /// different code for faking data in the database.
    ///  
    /// Example Use Case that will return a new empty system user for queryexpression that is for System Users
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
    public class FakeIOrganizationService : ClientSideOrganizationService, IServiceFaked<IOrganizationService>, IFakeService
    {
        #region IOrganizationService Mocks

        /// <summary>
        /// Gets or sets the associate action.
        /// </summary>
        /// <value>
        /// The associate action.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection> AssociateAction { get; set; }
        /// <summary>
        /// Gets or sets the associate actions.
        /// </summary>
        /// <value>
        /// The associate actions.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] AssociateActions { get; set; }
        private IOrganizationService AssociateCache { get; set; }
        /// <summary>
        /// Gets or sets the create function.
        /// </summary>
        /// <value>
        /// The create function.
        /// </value>
        public Func<IOrganizationService, Entity, Guid> CreateFunc { get; set; }
        /// <summary>
        /// Gets or sets the create funcs.
        /// </summary>
        /// <value>
        /// The create funcs.
        /// </value>
        public Func<IOrganizationService, Entity, Guid>[] CreateFuncs { get; set; }
        private IOrganizationService CreateCache { get; set; }
        /// <summary>
        /// Gets or sets the delete action.
        /// </summary>
        /// <value>
        /// The delete action.
        /// </value>
        public Action<IOrganizationService, string, Guid> DeleteAction { get; set; }
        /// <summary>
        /// Gets or sets the delete actions.
        /// </summary>
        /// <value>
        /// The delete actions.
        /// </value>
        public Action<IOrganizationService, string, Guid>[] DeleteActions { get; set; }
        private IOrganizationService DeleteCache { get; set; }
        /// <summary>
        /// Gets or sets the disassociate action.
        /// </summary>
        /// <value>
        /// The disassociate action.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection> DisassociateAction { get; set; }
        /// <summary>
        /// Gets or sets the disassociate actions.
        /// </summary>
        /// <value>
        /// The disassociate actions.
        /// </value>
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] DisassociateActions { get; set; }
        private IOrganizationService DisassociateCache { get; set; }
        /// <summary>
        /// Gets or sets the execute function.
        /// </summary>
        /// <value>
        /// The execute function.
        /// </value>
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse> ExecuteFunc { get; set; }
        /// <summary>
        /// Gets or sets the execute funcs.
        /// </summary>
        /// <value>
        /// The execute funcs.
        /// </value>
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[] ExecuteFuncs { get; set; }
        private IOrganizationService ExecuteCache { get; set; }
        /// <summary>
        /// Gets or sets the retrieve multiple function.
        /// </summary>
        /// <value>
        /// The retrieve multiple function.
        /// </value>
        public Func<IOrganizationService, QueryBase, EntityCollection> RetrieveMultipleFunc { get; set; }
        /// <summary>
        /// Gets or sets the retrieve multiple funcs.
        /// </summary>
        /// <value>
        /// The retrieve multiple funcs.
        /// </value>
        public Func<IOrganizationService, QueryBase, EntityCollection>[] RetrieveMultipleFuncs { get; set; }
        private IOrganizationService RetrieveMultipleCache { get; set; }
        /// <summary>
        /// Gets or sets the retrieve function.
        /// </summary>
        /// <value>
        /// The retrieve function.
        /// </value>
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity> RetrieveFunc { get; set; }
        /// <summary>
        /// Gets or sets the retrieve funcs.
        /// </summary>
        /// <value>
        /// The retrieve funcs.
        /// </value>
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity>[] RetrieveFuncs { get; set; }
        private IOrganizationService RetrieveCache { get; set; }
        /// <summary>
        /// Gets or sets the update action.
        /// </summary>
        /// <value>
        /// The update action.
        /// </value>
        public Action<IOrganizationService, Entity> UpdateAction { get; set; }
        /// <summary>
        /// Gets or sets the update actions.
        /// </summary>
        /// <value>
        /// The update actions.
        /// </value>
        public Action<IOrganizationService, Entity>[] UpdateActions { get; set; }
        private IOrganizationService UpdateCache { get; set; }

        #endregion IOrganizationService Mocks

        /// <summary>
        /// Gets or sets a value indicating whether [execution tracing enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [execution tracing enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ExecutionTracingEnabled => Timer != null;
        private TestActionTimer Timer { get; }
        /// <summary>
        /// Changes calls to Execute with a RetrieveMultipleRequest to be a RetrieveMultple Call
        /// </summary>
        /// <value>
        /// <c>true</c> if [redirect execute retrieve multiple to retrieve multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool RedirectExecuteRequestsToOrganizationServiceRequest { get; set; }

        /// <summary>
        /// Reflectively walks the Service Path to find the first IOrganizationService that is not a FakeIOrganizationService
        /// Returns the highest level FakeIOrganizationService if every Service is of type FakeIOrganizationService
        /// </summary>
        public IOrganizationService ActualService => GetActualService();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeIOrganizationService" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public FakeIOrganizationService(IOrganizationService service)
                    : base(service)
        {

            if (service is FakeIOrganizationService fake)
            {
                Timer = fake.Timer;
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
            if (Service == null)
            {
                return this;
            }
            return !(Service is FakeIOrganizationService parent) ? Service : parent.GetActualService();
        }

        #region IOrganizationService Members

        /// <summary>
        /// Creates a link between records.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
        [DebuggerStepThrough]
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
                        AssociateCache = new FakeIOrganizationService(AssociateCache) { AssociateAction = action };
                    }
                }
                AssociateCache.Associate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer.Time(AssociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Associate {0}:{1} using relationship {2} to releated Entities {3}: {4}",
                        entityName, entityId, relationship, relatedEntities.Select(e => e.GetNameId()).ToCsv());
                }
                else
                {
                    Service.Associate(entityName, entityId, relationship, relatedEntities);
                }
            }
        }

        [DebuggerHidden]
        private object AssociateInternal(Tuple<string, Guid, Relationship, EntityReferenceCollection> value)
        {
            Service.Associate(value.Item1, value.Item2, value.Item3, value.Item4);
            return null;
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
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
                        CreateCache = new FakeIOrganizationService(CreateCache) { CreateFunc = create };
                    }
                }
                id = CreateCache.Create(entity);
            }
            else
            {
                id = ExecutionTracingEnabled ? Timer.Time(CreateInternal, entity, "Create {0}: {1}", entity.GetNameId()) : Service.Create(entity);
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
        [DebuggerStepThrough]
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
                        DeleteCache = new FakeIOrganizationService(DeleteCache) { DeleteAction = delete };
                    }
                }
                DeleteCache.Delete(entityName, id);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer.Time(DeleteInternal, new Tuple<string, Guid>(entityName, id), "Delete {0}({1}): {2}", entityName, id);
                }
                else
                {
                    Service.Delete(entityName, id);
                }
            }
        }

        [DebuggerHidden]
        private object DeleteInternal(Tuple<string, Guid> value)
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
        [DebuggerStepThrough]
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
                        DisassociateCache = new FakeIOrganizationService(DisassociateCache) { DisassociateAction = disassociate };
                    }
                }
                DisassociateCache.Disassociate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer.Time(DisassociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Disassociate {0}:{1} using relationship {2} to releated Entities {3}: {4}",
                        entityName, entityId, relationship, relatedEntities.Select(e => e.GetNameId()).ToCsv());
                }
                else
                {
                    Service.Disassociate(entityName, entityId, relationship, relatedEntities);
                }
            }
        }

        [DebuggerHidden]
        private object DisassociateInternal(Tuple<string, Guid, Relationship, EntityReferenceCollection> value)
        {
            Service.Disassociate(value.Item1, value.Item2, value.Item3, value.Item4);
            return null;
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public override OrganizationResponse Execute(OrganizationRequest request)
        {
            OrganizationResponse response;
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
                        ExecuteCache = new FakeIOrganizationService(ExecuteCache) { ExecuteFunc = execute };
                    }
                }
                response = ExecuteCache.Execute(request);
            }
            else
            {
                response = ExecutionTracingEnabled ? Timer.Time(ExecuteInternal, request, "Execute {0}: {1}", request.RequestName) : Service.Execute(request);
            }
            return response;
        }

        /// <summary>
        /// Rather than having to define mocks seperately for both RetrieveREquest and Retrieve, this will map the call to the correct OrganizationService Method.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [DebuggerHidden]
        private OrganizationResponse CallOrganizationServiceRequestForExecuteRequest(OrganizationRequest request)
        {
            OrganizationResponse response = null;
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
#if !PRE_KEYATTRIBUTE
                if (target.Id == Guid.Empty && target.KeyAttributes.Count > 0)
                {
                    target = this.GetEntityOrDefault(target.LogicalName, target.KeyAttributes, retrieve.ColumnSet).ToEntityReference();
                }
#endif
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
        [DebuggerStepThrough]
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
                        RetrieveCache = new FakeIOrganizationService(RetrieveCache) { RetrieveFunc = retrieve };
                    }
                }
                entity = RetrieveCache.Retrieve(entityName, id, columnSet);
            }
            else
            {
                entity = ExecutionTracingEnabled ? Timer.Time(RetrieveInternal, new Tuple<string, Guid, ColumnSet>(entityName, id, columnSet), "Retrieve {0}({1}): {2}", entityName, id) : Service.Retrieve(entityName, id, columnSet);
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
        [DebuggerStepThrough]
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
                    foreach (var retrievemultiple in RetrieveMultipleFuncs)
                    {
                        RetrieveMultipleCache = new FakeIOrganizationService(RetrieveMultipleCache) { RetrieveMultipleFunc = retrievemultiple };
                    }
                }
                entities = RetrieveMultipleCache.RetrieveMultiple(query);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    entities = query is QueryExpression qe
                        ? Timer.Time(RetrieveMultipleInternal, query, "RetrieveMultiple: {2}{0}{1}", Environment.NewLine, qe.GetSqlStatement())
                        : Timer.Time(RetrieveMultipleInternal, query, "RetrieveMultiple {0}: {1}", query);
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
        [DebuggerStepThrough]
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
                        UpdateCache = new FakeIOrganizationService(UpdateCache) { UpdateAction = update };
                    }
                }
                UpdateCache.Update(entity);
            }
            else
            {
                if (ExecutionTracingEnabled)
                {
                    Timer.Time(UpdateInternal, entity, "Update Entity {0}: {1}", entity.GetNameId());
                }
                else
                {
                    Service.Update(entity);
                }
            }
        }

        [DebuggerHidden]
        private object UpdateInternal(Entity entity)
        {
            Service.Update(entity);
            return null;
        }

        #endregion
    }
}