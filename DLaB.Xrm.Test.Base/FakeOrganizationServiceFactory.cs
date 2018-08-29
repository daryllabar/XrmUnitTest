using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// A Fake that implements IOrganizationServiceFactory
    /// </summary>
    public class FakeOrganizationServiceFactory : IOrganizationServiceFactory
    {
        /// <summary>
        /// Gets the services key'd by User Id.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public Dictionary<Guid, IOrganizationService> Services { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOrganizationServiceFactory"/> class.
        /// </summary>
        public FakeOrganizationServiceFactory()
        {
            Services = new Dictionary<Guid, IOrganizationService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOrganizationServiceFactory"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public FakeOrganizationServiceFactory(IOrganizationService service): this()
        {
            Services.Add(Guid.Empty, service);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeOrganizationServiceFactory"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public FakeOrganizationServiceFactory(Dictionary<Guid, IOrganizationService> services)
        {
            Services = services;            
        }

        /// <summary>
        /// Creates the organization service.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            if (Services.TryGetValue(userId.GetValueOrDefault(), out IOrganizationService service)
                ||
                Services.TryGetValue(Guid.Empty, out service)
                )
            {
                return service;
            }

            return null;
        }

        /// <summary>
        /// Sets the service for the given user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="service">The service.</param>
        public void SetService(Guid? userId, IOrganizationService service)
        {
            Services[userId.GetValueOrDefault()] = service;
        }
    }
}
