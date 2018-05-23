using System;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm
#else
namespace Source.DLaB.Xrm
#endif
	
{
    /// <summary>
    /// A wrapper of an IOrganizationService
    /// </summary>
    public interface IIOrganizationServiceWrapper
    {
        /// <summary>
        /// The base IOrganizationService.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        IOrganizationService Service { get; set; }

        /// <summary>
        /// Returns the URI of the service.
        /// </summary>
        /// <returns></returns>
        Uri GetServiceUri();
    }
}
