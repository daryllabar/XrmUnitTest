using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    public class FakeOrganizationServiceFactory : IOrganizationServiceFactory
    {
        public IOrganizationService Service { get; set; }

        public FakeOrganizationServiceFactory(IOrganizationService service)
        {
            Service = service;
        }

        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return Service;
        }
    }
}
