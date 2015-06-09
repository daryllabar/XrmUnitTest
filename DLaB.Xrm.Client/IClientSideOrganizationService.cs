using Microsoft.Xrm.Sdk;
using System;

namespace DLaB.Xrm.Client
{
    public interface IClientSideOrganizationService : IOrganizationService, IIOrganizationServiceWrapper, IDisposable
    {
        IOrganizationService Service { get; set; }
    }
}
