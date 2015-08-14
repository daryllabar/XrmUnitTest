using System;
using System.Linq;
using System.Diagnostics;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Test
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
    public class FakeIOrganizationService : ClientSideOrganizationService
    {
        #region IOrganizationService Mocks

        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection> AssociateAction { get; set; }
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] AssociateActions { get; set; }
        private IOrganizationService AssociateCache { get; set; }
        public Func<IOrganizationService, Entity, Guid> CreateFunc { get; set; }
        public Func<IOrganizationService, Entity, Guid>[] CreateFuncs { get; set; }
        private IOrganizationService CreateCache { get; set; }
        public Action<IOrganizationService, string, Guid> DeleteAction { get; set; }
        public Action<IOrganizationService, string, Guid>[] DeleteActions { get; set; }
        private IOrganizationService DeleteCache { get; set; }
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection> DisassociateAction { get; set; }
        public Action<IOrganizationService, string, Guid, Relationship, EntityReferenceCollection>[] DisassociateActions { get; set; }
        private IOrganizationService DisassociateCache { get; set; }
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse> ExecuteFunc { get; set; }
        public Func<IOrganizationService, OrganizationRequest, OrganizationResponse>[] ExecuteFuncs { get; set; }
        private IOrganizationService ExecuteCache { get; set; }
        public Func<IOrganizationService, QueryBase, EntityCollection> RetrieveMultipleFunc { get; set; }
        public Func<IOrganizationService, QueryBase, EntityCollection>[] RetrieveMultipleFuncs { get; set; }
        private IOrganizationService RetrieveMultipleCache { get; set; }
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity> RetrieveFunc { get; set; }
        public Func<IOrganizationService, string, Guid, ColumnSet, Entity>[] RetrieveFuncs { get; set; }
        private IOrganizationService RetrieveCache { get; set; }
        public Action<IOrganizationService, Entity> UpdateAction { get; set; }
        public Action<IOrganizationService, Entity>[] UpdateActions { get; set; }
        private IOrganizationService UpdateCache { get; set; }

        #endregion // IOrganizationService Mocks

        public bool EnableExecutionTracing { get; set; }
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
        public IOrganizationService ActualService
        {
            get { return GetActualService(); }
        }

        public FakeIOrganizationService(IOrganizationService service)
            : base(service)
        {
            EnableExecutionTracing = false;
            RedirectExecuteRequestsToOrganizationServiceRequest = true;
        }

        private IOrganizationService GetActualService()
        {
            if (Service == null)
            {
                return this;
            }
            var parent = Service as FakeIOrganizationService;
            return parent == null ? Service : parent.GetActualService();
        }

        #region IOrganizationService Members

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
                        AssociateCache = new FakeIOrganizationService(AssociateCache) {AssociateAction = action};
                    }
                }
                AssociateCache.Associate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (EnableExecutionTracing)
                {
                    DebugLog.Time(AssociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Associate {0}:{1} using relationship {2} to releated Entities {3}: {4}",
                        entityName, entityId, relationship, String.Join(", ", relatedEntities.Select(e => e.GetNameId())));
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
                        CreateCache = new FakeIOrganizationService(CreateCache) {CreateFunc = create};
                    }
                }
                id = CreateCache.Create(entity);
            }
            else
            {
                id = EnableExecutionTracing ? DebugLog.Time(CreateInternal, entity, "Create {0}: {1}", entity.GetNameId()) : Service.Create(entity);
            }
            return id;
        }

        [DebuggerHidden]
        private Guid CreateInternal(Entity entity)
        {
            return Service.Create(entity);
        }

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
                        DeleteCache = new FakeIOrganizationService(DeleteCache) {DeleteAction = delete};
                    }
                }
                DeleteCache.Delete(entityName, id);
            }
            else
            {
                if (EnableExecutionTracing)
                {
                    DebugLog.Time(DeleteInternal, new Tuple<string, Guid>(entityName, id), "Delete {0}({1}): {2}", entityName, id);
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
                        DisassociateCache = new FakeIOrganizationService(DisassociateCache) {DisassociateAction = disassociate};
                    }
                }
                DisassociateCache.Disassociate(entityName, entityId, relationship, relatedEntities);
            }
            else
            {
                if (EnableExecutionTracing)
                {
                    DebugLog.Time(DisassociateInternal, new Tuple<string, Guid, Relationship, EntityReferenceCollection>(entityName, entityId, relationship, relatedEntities),
                        "Disassociate {0}:{1} using relationship {2} to releated Entities {3}: {4}",
                        entityName, entityId, relationship, String.Join(", ", relatedEntities.Select(e => e.GetNameId())));
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

        [DebuggerStepThrough]
        public override OrganizationResponse Execute(OrganizationRequest request)
        {
            OrganizationResponse response;
            if (RedirectExecuteRequestsToOrganizationServiceRequest)
            {
                response = CallOrganizationServiceRequestForExecuteRequest(request);

                if(response != null)
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
                        ExecuteCache = new FakeIOrganizationService(ExecuteCache) {ExecuteFunc = execute};
                    }
                }
                response = ExecuteCache.Execute(request);
            }
            else
            {
                response = EnableExecutionTracing ? DebugLog.Time(ExecuteInternal, request, "Execute {0}: {1}", request.RequestName) : Service.Execute(request);
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
            var associate = request as AssociateRequest;
            if (associate != null)
            {
                response = new AssociateResponse();
                Associate(associate.Target.LogicalName, associate.Target.Id, associate.Relationship, associate.RelatedEntities);
            }

            var create = request as CreateRequest;
            if (create != null)
            {
                response = new CreateResponse();
                response["id"] = Create(create.Target);
            }

            var delete = request as DeleteRequest;
            if (delete != null)
            {
                response = new DeleteResponse();
                Delete(delete.Target.LogicalName, delete.Target.Id);
            }

            var disassociate = request as DisassociateRequest;
            if (disassociate != null)
            {
                response = new AssociateResponse();
                Disassociate(disassociate.Target.LogicalName, disassociate.Target.Id, disassociate.Relationship, disassociate.RelatedEntities);
            }

            var retrieve = request as RetrieveRequest;
            if (retrieve != null)
            {
                response = new RetrieveResponse();
                response["Entity"] = Retrieve(retrieve.Target.LogicalName, retrieve.Target.Id, retrieve.ColumnSet);
            }

            var retrieveMultiple = request as RetrieveMultipleRequest;
            if (retrieveMultiple != null)
            {
                response = new RetrieveMultipleResponse();
                response["EntityCollection"] = RetrieveMultiple(retrieveMultiple.Query);
            }

            var update = request as UpdateRequest;
            if (update != null)
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
                        RetrieveCache = new FakeIOrganizationService(RetrieveCache) {RetrieveFunc = retrieve};
                    }
                }
                entity = RetrieveCache.Retrieve(entityName, id, columnSet);
            }
            else
            {
                entity = EnableExecutionTracing ? DebugLog.Time(RetrieveInternal, new Tuple<string, Guid, ColumnSet>(entityName, id, columnSet), "Retrieve {0}({1}): {2}", entityName, id) : Service.Retrieve(entityName, id, columnSet);
            }
            return entity;
        }

        [DebuggerHidden]
        private Entity RetrieveInternal(Tuple<string, Guid, ColumnSet> value)
        {
            return Service.Retrieve(value.Item1, value.Item2, value.Item3);
        }

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
                        RetrieveMultipleCache = new FakeIOrganizationService(RetrieveMultipleCache) {RetrieveMultipleFunc = retrievemultiple};
                    }
                }
                entities = RetrieveMultipleCache.RetrieveMultiple(query);
            }
            else
            {
                if (EnableExecutionTracing)
                {
                    var qe = query as QueryExpression;

                    entities = qe == null ? DebugLog.Time(RetrieveMultipleInternal, query, "RetrieveMultiple {0}: {1}", query) : DebugLog.Time(RetrieveMultipleInternal, query, "RetrieveMultiple: {2}{0}{1}", Environment.NewLine, qe.GetSqlStatement());
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
                        UpdateCache = new FakeIOrganizationService(UpdateCache) {UpdateAction = update};
                    }
                }
                UpdateCache.Update(entity);
            }
            else
            {
                if (EnableExecutionTracing)
                {
                    DebugLog.Time(UpdateInternal, entity, "Update Entity {0}: {1}", entity.GetNameId());
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