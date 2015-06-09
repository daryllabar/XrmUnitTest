using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DLaB.Xrm.Client
{
    public class ClientSideOrganizationService : IClientSideOrganizationService
    {
        #region Properties

        public IOrganizationService Service { get; set; }

        #endregion // Properties

        #region Constructors

        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(IOrganizationService service)
        {
            Service = service;
        }

        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(CrmServiceInfo info) :
            this(CrmServiceUtility.GetOrganizationService(info))
        { }

        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(string crmOrganization, Guid impersonationUserId = new Guid()) :
            this(CrmServiceUtility.GetOrganizationService(crmOrganization, impersonationUserId))
        { }

        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(string crmOrganizationUrl, string crmDiscoveryUrl, string crmOrganization,
                                             Guid impersonationUserId = new Guid(), bool enableProxyTypes = true) :
            this(CrmServiceUtility.GetOrganizationService(crmOrganizationUrl, crmDiscoveryUrl, crmOrganization,
                                                               impersonationUserId, enableProxyTypes))
        { }

        /// <summary>
        /// Create the Organization proxy given the network user credentials
        /// </summary>
        [System.Diagnostics.DebuggerHidden]
        public ClientSideOrganizationService(string crmOrganizationUrl, string crmDiscoveryUrl, string crmOrganization,
                                             string domain, string userName, string password, bool enableProxyTypes = true) :
            this(CrmServiceUtility.GetOrganizationService(crmOrganizationUrl, crmDiscoveryUrl, crmOrganization,
                                                               domain, userName, password, enableProxyTypes))
        { }

        #endregion // Constructors

        #region IOrganizationService Members

        [System.Diagnostics.DebuggerHidden]
        public virtual void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Service.Associate(entityName, entityId, relationship, relatedEntities);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual Guid Create(Entity entity)
        {
            return Service.Create(entity);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual void Delete(string entityName, Guid id)
        {
            Service.Delete(entityName, id);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Service.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            return Service.Execute(request);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return Service.Retrieve(entityName, id, columnSet);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual EntityCollection RetrieveMultiple(QueryBase query)
        {
            return Service.RetrieveMultiple(query);
        }

        [System.Diagnostics.DebuggerHidden]
        public virtual void Update(Entity entity)
        {
            Service.Update(entity);
        }

        #endregion

        #region IDisposable Members

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
                if (!this.IsDisposed)
                {
                    // Explicitly set root references to null to expressly tell the GarbageCollector
                    // that the resources have been disposed of and its ok to release the memory 
                    // allocated for them.
                    if (isDisposing)
                    {
                        // Release all managed resources here
                        if (Service != null)
                        {
                            var disposable = Service as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
                this.IsDisposed = true;

                // explicitly call the base class Dispose implementation
                //base.Dispose(isDisposing);
            }
        }

        #endregion

        #region IIOrganizationServiceWrapper

        public Uri GetServiceUri()
        {
            return Service == null ? new Uri("localhost") : Service.GetServiceUri();
        }

        #endregion // IIOrganizationServiceWrapper
    }
}
