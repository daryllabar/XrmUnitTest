using Microsoft.Xrm.Sdk;
using System;
using DLaB.Xrm;
#if NET
using Microsoft.PowerPlatform.Dataverse.Client;
#endif

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// A Disposible service that allows for getting the Service Uri.
    /// </summary>
#if NET
    public interface IClientSideOrganizationService : IOrganizationServiceAsync2, IDisposable
#else
    public interface IClientSideOrganizationService : IOrganizationService, IDisposable
#endif
    {
        /// <summary>
        /// Returns an Uri for the Organization Service
        /// </summary>
        /// <returns></returns>
        Uri GetServiceUri();
    }
}
