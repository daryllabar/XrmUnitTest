using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Extension Class for Client Side Crm integrations
    /// </summary>
    public static class Extensions
    {
        #region Entity

        /// <summary>
        /// Gets the actual type of the entity as defined by the entity LogicalName, even if it is just an entity.
        /// ie: (new Entity(Contact.EntityLogicalName)).GetEntityType() == (new Contact()).GetType()
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Type GetEntityType(this Entity entity)
        {
            var assembly = CrmServiceUtility.GetEarlyBoundProxyAssembly();
            foreach (var t in assembly.GetTypes())
            {
                var attribute = (EntityLogicalNameAttribute?) t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), false).FirstOrDefault();
                if (attribute != null && attribute.LogicalName == entity.LogicalName)
                {
                    return t;
                }
            }
            throw new Exception("Type " + entity.LogicalName + " Not found!");
        }

        #endregion Entity

        #region IOrganizationService

        /// <summary>
        /// Retrieves the Organization Name from the URL being used by the IOrganizationService.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <remarks>This method will not support IFD</remarks>
        public static string GetOrganizationName(this IOrganizationService service)
        {
            var uri = service.GetServiceUri();
            if (uri == null)
            {
                throw new Exception("GetOrganizationName does not support In Process implementations of the IOrganizationService.");
            }
            return uri.Segments[1].Replace(@"/", "").ToLower();
        }

        /// <summary>
        /// Assumes that this service is of type ServiceProxy&lt;IOrganizationService&gt; or IIOrganizationServiceWrapper
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public static Uri GetServiceUri(this IOrganizationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
#if NET
            if (service is IClientSideOrganizationService clientSideService)
            {
                throw new NotSupportedException("Maybe this would work? " + clientSideService.GetServiceUri());
            }
            throw new NotSupportedException("Not checked to see if this is supported yet!");
#else
            if (service is ServiceProxy<IOrganizationService> proxy)
            {
                return proxy.ServiceConfiguration?.CurrentServiceEndpoint.Address.Uri ?? new Uri("localhost");
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (service is IClientSideOrganizationService clientSideService)
            {
                return clientSideService.GetServiceUri();
            }

            // Check for Xrm.Client.Services.OrganizationService
            var innerService = service.GetType().GetProperty("InnerService");
            if (innerService == null)
            {
                throw new ArgumentException("Unable to determine the Uri for the IOrganizationService of type " + service.GetType().FullName);
            }

            proxy = (ServiceProxy<IOrganizationService>)innerService.GetValue(service);

            return proxy.ServiceConfiguration?.CurrentServiceEndpoint.Address.Uri ?? new Uri("localhost");
#endif
        }

#endregion IOrganizationService
    }
}
