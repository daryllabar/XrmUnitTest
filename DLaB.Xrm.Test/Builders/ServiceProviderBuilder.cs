using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public class ServiceProviderBuilder
    {
        private FakeServiceProvider ServiceProvider { get; set; }

        public ServiceProviderBuilder()
            : this(TestBase.GetOrganizationService(),
            new FakePluginExecutionContext())
        {

        }

        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context)
        {
            ServiceProvider = new FakeServiceProvider();
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            ServiceProvider.AddService(context);
            ServiceProvider.AddService<ITracingService>(new FakeTraceService());
        }

        #region Fleunt Methods

        /// <summary>
        /// Causes the Build Service provider to have the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public ServiceProviderBuilder WithContext(IPluginExecutionContext context)
        {
            ServiceProvider.AddService(context);
            return this;
        }

        /// <summary>
        /// Causes the Build Service provider to have an IOrganizationServiceFactory that returns the given service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public ServiceProviderBuilder WithService(IOrganizationService service)
        {
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            return this;
        }

        /// <summary>
        /// Causes the Build Service provider to have the given service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public ServiceProviderBuilder WithService<T>(T service)
        {
            ServiceProvider.AddService(service);
            return this;
        }

        #endregion // Fleunt Methods

        public IServiceProvider Build() {
            return ServiceProvider;
        }
    }
}
