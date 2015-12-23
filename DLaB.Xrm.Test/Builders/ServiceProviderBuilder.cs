using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// Builder for creating Service Provider
    /// </summary>
    public sealed class ServiceProviderBuilder : ServiceProviderBuilderBase<ServiceProviderBuilder>
    {
        /// <summary>
        /// Gets the Service Provider Builder of the derived Class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override ServiceProviderBuilder This => this;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder" /> class.
        /// </summary>
        public ServiceProviderBuilder()
                    : base(TestBase.GetOrganizationService(),
                    new FakePluginExecutionContext(), (ITestLogger)null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context) : base(service, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITestLogger logger) : base(service, context, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <param name="trace">The tracing service.</param>
        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITracingService trace) : base(service, context, trace)
        {
        }
    }

    /// <summary>
    /// Abstract Builder to allow for Derived Types to be created
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    public abstract class ServiceProviderBuilderBase<TDerived> where TDerived : ServiceProviderBuilderBase<TDerived>
    {
        /// <summary>
        /// Gets the derived version of the class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected abstract TDerived This { get; }
        private FakeServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilderBase{TDerived}" /> class.
        /// </summary>
        protected ServiceProviderBuilderBase()
                    : this(TestBase.GetOrganizationService(),
                    new FakePluginExecutionContext(), (ITestLogger)null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        protected ServiceProviderBuilderBase(IOrganizationService service, IPluginExecutionContext context) : this(service,context, (ITestLogger)null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        protected ServiceProviderBuilderBase(IOrganizationService service, IPluginExecutionContext context, ITestLogger logger)
        {
            ServiceProvider = new FakeServiceProvider();
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            ServiceProvider.AddService(context);
            ServiceProvider.AddService<ITracingService>(new FakeTraceService(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <param name="trace">The tracing service.</param>
        protected ServiceProviderBuilderBase(IOrganizationService service, IPluginExecutionContext context, ITracingService trace)
        {
            ServiceProvider = new FakeServiceProvider();
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            ServiceProvider.AddService(context);
            ServiceProvider.AddService(trace);
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

        /// <summary>
        /// Builds a Cloned version of the Service Provider.
        /// </summary>
        /// <returns></returns>
        public IServiceProvider Build()
        {
            return ServiceProvider.Clone();
        }
    }
}
