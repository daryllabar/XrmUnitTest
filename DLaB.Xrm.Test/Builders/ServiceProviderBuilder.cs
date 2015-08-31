using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public sealed class ServiceProviderBuilder : ServiceProviderBuilderBase<ServiceProviderBuilder>
    {
        protected override ServiceProviderBuilder This
        {
            get { return this; }
        }
    }

    public abstract class ServiceProviderBuilderBase<TDerived> where TDerived : ServiceProviderBuilderBase<TDerived>
    {
        protected abstract TDerived This { get; }
        private FakeServiceProvider ServiceProvider { get; set; }

        protected ServiceProviderBuilderBase()
            : this(TestBase.GetOrganizationService(),
            new FakePluginExecutionContext())
        {

        }

        protected ServiceProviderBuilderBase(IOrganizationService service, IPluginExecutionContext context)
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
        public TDerived WithContext(IPluginExecutionContext context)
        {
            ServiceProvider.AddService(context);
            return This;
        }

        /// <summary>
        /// Causes the Build Service provider to have an IOrganizationServiceFactory that returns the given service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TDerived WithService(IOrganizationService service)
        {
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            return This;
        }

        /// <summary>
        /// Causes the Build Service provider to have the given service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TDerived WithService<T>(T service)
        {
            ServiceProvider.AddService(service);
            return This;
        }

        #endregion // Fleunt Methods

        public IServiceProvider Build() {
            return ServiceProvider.Clone();
        }
    }
}
