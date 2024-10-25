using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

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
