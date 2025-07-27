using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else

namespace DLaB.Xrm.Test.Builders
#endif
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
                    : base(TestBase.GetOrganizationService(Guid.NewGuid().ToString()),
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

        #region Constructors

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

#if !PRE_MULTISELECT
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilderBase{TDerived}" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <param name="trace">The tracing service.</param>
        /// <param name="managed">The managed identity service.</param>
        protected ServiceProviderBuilderBase(IOrganizationService service, IPluginExecutionContext context, ITracingService trace, IManagedIdentityService managed)
        {
            ServiceProvider = new FakeServiceProvider();
            ServiceProvider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(service));
            ServiceProvider.AddService(context);
            ServiceProvider.AddService(trace);
            ServiceProvider.AddService(managed);
        }
#endif

        #endregion Constructors

        #region Fluent Methods

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
        /// The build method will clone all services.  This call will prevent the given service from being cloned on build.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TDerived WithoutClone<TService>()
        {
            return WithoutClone(typeof(TService));
        }

        /// <summary>
        /// The build method will clone all services.  This call will prevent the given service from being cloned on build.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public TDerived WithoutClone(Type type)
        {
            ServiceProvider.TypesToSkipCloning.Add(type);
            return This;
        }

        /// <summary>
        /// The fallback service provider if the built IServiceProvider doesn't define a requested type.
        /// </summary>
        /// <param name="defaultProvider">The fallback service provider if the built IServiceProvider doesn't define a requested type.</param>
        /// <returns></returns>
        public TDerived WithDefaultProvider(IServiceProvider defaultProvider)
        {
            ServiceProvider.DefaultProvider = defaultProvider;
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
        /// Causes the Build Service provider to have an IOrganizationServiceFactory that returns the given service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="setWhoAmIUserId">if set to <c>true</c> wraps service with a WhoAmI of the current user.</param>
        /// <returns></returns>
        public TDerived WithService(IOrganizationService service, Guid userId, bool setWhoAmIUserId = true)
        {
            if (setWhoAmIUserId)
            {
                var builder = new OrganizationServiceBuilder(service);

                builder.WithFakeExecute((s, r) =>
                {
                    if (r is WhoAmIRequest)
                    {
                        var result = (WhoAmIResponse)s.Execute(r);
                        result.Results["UserId"] = userId;
                        return result;
                    }

                    return s.Execute(r);
                });

                service = builder.Build();
                
            }
            ((FakeOrganizationServiceFactory)ServiceProvider.GetService<IOrganizationServiceFactory>()).SetService(userId, service);
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

        #endregion Fluent Methods

        /// <summary>
        /// Called before the Service Provider Build() is called.
        /// </summary>
        protected virtual void PreBuild() { }

        /// <summary>
        /// Builds a Cloned version of the Service Provider.
        /// </summary>
        /// <returns></returns>
        public IServiceProvider Build()
        {
            PreBuild();
            return BuildInternal(ServiceProvider.Clone());
        }

        /// <summary>
        /// Called when Build() is called.  Value returned will be the value returned from Build()
        /// </summary>
        /// <param name="provider">The provider created</param>
        /// <returns>The value to return from the Build() method.</returns>
        protected virtual IServiceProvider BuildInternal(IServiceProvider provider)
        {
            return provider;
        }
    }
}
