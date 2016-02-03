using Microsoft.Xrm.Sdk;
using System;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// A Disposible service that allows for getting the Service Uri.
    /// </summary>
    public interface IClientSideOrganizationService : IOrganizationService, IIOrganizationServiceWrapper, IDisposable
    {

    }
}
