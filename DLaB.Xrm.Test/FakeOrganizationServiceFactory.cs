using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// A Fake that implements IOrganizationServiceFactory
    /// </summary>
    public class FakeOrganizationServiceFactory : IOrganizationServiceFactory
    {
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public IOrganizationService Service { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOrganizationServiceFactory"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public FakeOrganizationServiceFactory(IOrganizationService service)
        {
            Service = service;
        }

        /// <summary>
        /// Creates the organization service.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return Service;
        }
    }
}
