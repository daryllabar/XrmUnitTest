using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
#if NET
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Implements IClientSideOrganizationService
    /// </summary>
    public class ClientSideOrganizationService : IClientSideOrganizationService
    {
        #region Properties

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public IOrganizationService Service { get; protected set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSideOrganizationService"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(IOrganizationService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSideOrganizationService"/> class.
        /// </summary>
        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService() :
            this(CrmServiceUtility.GetOrganizationService())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSideOrganizationService"/> class.
        /// </summary>
        /// <param name="connectionString">The CRM organization Connection String.</param>
        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(string connectionString) :
            this(CrmServiceUtility.GetOrganizationService(connectionString))
        { }

        #endregion Constructors

        #region IOrganizationService Members

        /// <summary>
        /// Creates a link between records.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
        [System.Diagnostics.DebuggerHidden]
        public virtual void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Service.Associate(entityName, entityId, relationship, relatedEntities);
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerHidden]
        public virtual Guid Create(Entity entity)
        {
            return Service.Create(entity);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        [System.Diagnostics.DebuggerHidden]
        public virtual void Delete(string entityName, Guid id)
        {
            Service.Delete(entityName, id);
        }

        /// <summary>
        /// Removes a link between records.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
        [System.Diagnostics.DebuggerHidden]
        public virtual void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Service.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerHidden]
        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            return Service.Execute(request);
        }

        /// <summary>
        /// Retrieves the specified entity.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="columnSet">The column set.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerHidden]
        public virtual Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return Service.Retrieve(entityName, id, columnSet);
        }

        /// <summary>
        /// Retrieves the entities defined by the Query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerHidden]
        public virtual EntityCollection RetrieveMultiple(QueryBase query)
        {
            return Service.RetrieveMultiple(query);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        [System.Diagnostics.DebuggerHidden]
        public virtual void Update(Entity entity)
        {
            Service.Update(entity);
        }

        #endregion

#if NET
        #region IOrganizationServiceAsync2 Members

        /// <summary>
        /// Gets the wrapped service as an IOrganizationServiceAsync, or null if it does not implement it.
        /// </summary>
        private IOrganizationServiceAsync? AsyncService => Service as IOrganizationServiceAsync;

        /// <summary>
        /// Gets the wrapped service as an IOrganizationServiceAsync2, or null if it does not implement it.
        /// </summary>
        private IOrganizationServiceAsync2? AsyncService2 => Service as IOrganizationServiceAsync2;

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return AsyncService?.AssociateAsync(entityName, entityId, relationship, relatedEntities)
                   ?? Task.Run(() => Associate(entityName, entityId, relationship, relatedEntities));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
        {
            return AsyncService2?.AssociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken)
                   ?? Task.Run(() => Associate(entityName, entityId, relationship, relatedEntities), cancellationToken);
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
        {
            if (AsyncService2 != null)
            {
                return AsyncService2.CreateAndReturnAsync(entity, cancellationToken);
            }

            entity.Id = Create(entity);
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<Guid> CreateAsync(Entity entity)
        {
            return AsyncService?.CreateAsync(entity)
                   ?? Task.FromResult(Create(entity));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
        {
            return AsyncService2?.CreateAsync(entity, cancellationToken)
                   ?? Task.FromResult(Create(entity));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task DeleteAsync(string entityName, Guid id)
        {
            return AsyncService?.DeleteAsync(entityName, id)
                   ?? Task.Run(() => Delete(entityName, id));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
        {
            return AsyncService2?.DeleteAsync(entityName, id, cancellationToken)
                   ?? Task.Run(() => Delete(entityName, id), cancellationToken);
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return AsyncService?.DisassociateAsync(entityName, entityId, relationship, relatedEntities)
                   ?? Task.Run(() => Disassociate(entityName, entityId, relationship, relatedEntities));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
        {
            return AsyncService2?.DisassociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken)
                   ?? Task.Run(() => Disassociate(entityName, entityId, relationship, relatedEntities), cancellationToken);
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
        {
            return AsyncService?.ExecuteAsync(request)
                   ?? Task.FromResult(Execute(request));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
        {
            return AsyncService2?.ExecuteAsync(request, cancellationToken)
                   ?? Task.FromResult(Execute(request));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet)
        {
            return AsyncService?.RetrieveAsync(entityName, id, columnSet)
                   ?? Task.FromResult(Retrieve(entityName, id, columnSet));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
        {
            return AsyncService2?.RetrieveAsync(entityName, id, columnSet, cancellationToken)
                   ?? Task.FromResult(Retrieve(entityName, id, columnSet));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<EntityCollection> RetrieveMultipleAsync(QueryBase query)
        {
            return AsyncService?.RetrieveMultipleAsync(query)
                   ?? Task.FromResult(RetrieveMultiple(query));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
        {
            return AsyncService2?.RetrieveMultipleAsync(query, cancellationToken)
                   ?? Task.FromResult(RetrieveMultiple(query));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task UpdateAsync(Entity entity)
        {
            return AsyncService?.UpdateAsync(entity)
                   ?? Task.Run(() => Update(entity));
        }

        /// <inheritdoc/>
        [System.Diagnostics.DebuggerHidden]
        public virtual Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
        {
            return AsyncService2?.UpdateAsync(entity, cancellationToken)
                   ?? Task.Run(() => Update(entity), cancellationToken);
        }

        #endregion IOrganizationServiceAsync2 Members
#endif

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Default initialization for a bool is 'false'</remarks>
        private bool IsDisposed { get; set; }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        //~ClientSideOrganizationService() 
        //{
        //    // Finalizer calls Dispose(false)
        //    Dispose(false);
        //}

        /// <summary>
        /// Overloaded Implementation of Dispose.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// <list type="bulleted">Dispose(bool isDisposing) executes in two distinct scenarios.
        /// <item>If <paramref name="isDisposing"/> equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.</item>
        /// <item>If <paramref name="isDisposing"/> equals <c>false</c>, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.</item></list>
        /// </remarks>
        protected virtual void Dispose(bool isDisposing)
        {
            // TODO If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            try
            {
                if (IsDisposed || !isDisposing || Service == null)
                {
                    return;
                }
                // Explicitly set root references to null to expressly tell the GarbageCollector
                // that the resources have been disposed of and its ok to release the memory 
                // allocated for them.

                // Release all managed resources here
                (Service as IDisposable)?.Dispose();
            }
            finally
            {
                IsDisposed = true;

                // explicitly call the base class Dispose implementation
                //base.Dispose(isDisposing);
            }
        }

        #endregion

        #region ICliendSideOrganizationService Members

        /// <summary>
        /// Gets the service URI.
        /// </summary>
        /// <returns></returns>
        public Uri GetServiceUri()
        {
            return Service == null ? new Uri("localhost") : Service.GetServiceUri();
        }

        #endregion ICliendSideOrganizationService Members
    }
}
