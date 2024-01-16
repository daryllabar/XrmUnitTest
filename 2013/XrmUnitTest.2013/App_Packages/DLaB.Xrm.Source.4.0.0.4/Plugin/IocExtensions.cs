using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// Extensions for Ioc
    /// </summary>
    public static class IocExtensions
    {
        /// <summary>
        /// Registers the default types for use within the context of a Dataverse plugin.
        /// </summary>
        /// <param name="container">The IoC container.</param>
        /// <param name="unsecureConfig">The unsecure configuration string.</param>
        /// <param name="secureConfig">The secure configuration string.</param>
        /// <param name="plugin">The registered events plugin used to build the Extended Plugin Context.</param>
        /// <returns>The IoC container.</returns>
        public static IIocContainer RegisterDataversePluginDefaults(this IIocContainer container, string unsecureConfig = null, string secureConfig = null, IRegisteredEventsPlugin plugin = null)
        {
            return container.RegisterDataversePluginDefaults(new ConfigWrapper(unsecureConfig, secureConfig)
            {
                UnsecureConfig = unsecureConfig,
                SecureConfig = secureConfig
            }, plugin);
        }

        /// <summary>
        /// Registers the default types for use within the context of a Dataverse plugin.
        /// Assumes the container will have a service provider with an IPluginExecutionContext, IOrganizationServiceFactory, and ITracingService registered.
        /// </summary>
        /// <param name="container">The IoC container.</param>
        /// <param name="configWrapper">The configuration wrapper.</param>
        /// <param name="plugin">The registered events plugin used to build the Extended Plugin Context.</param>
        /// <returns>The IoC container.</returns>
        public static IIocContainer RegisterDataversePluginDefaults(this IIocContainer container, ConfigWrapper configWrapper, IRegisteredEventsPlugin plugin = null)
        {
            // Conditionally register the IRegistered Events Plugin
            if (plugin != null)
            {
                // IRegisteredEventsPlugin
                container.AddSingleton(plugin);
            }

            // Order of registrations does not matter. 
            container.RegisterDLaBDefaults()
                // ConfigWrapper
                .AddSingleton(configWrapper)

                // IExtendedPluginContext
                .AddScoped<IExtendedPluginContext, DLaBExtendedPluginContextBase>()

                // IOrganizationService
                .AddScoped<IOrganizationService>(s =>
                {
                    var context = s.Get<IPluginExecutionContext>();
                    return s.CreateExtendedOrganizationService(context.UserId);
                })

                // ITracingService
                .AddScoped<ITracingService>(s =>
                {
                    var defaultTracingService = s.Get<WrappedServiceProvider>()?.ServiceProvider?.Get<ITracingService>();
                    if(defaultTracingService == null)
                    {
                        throw new Exception("When RegisterDataversePluginDefaults is used, the wrapped Service Provider must provide an ITracingService!");
                    }
                    return new ExtendedTracingService(defaultTracingService);
                })

                // OrganizationServicesWrapper
                .AddScoped(s =>
                { 
                    var admin = new Lazy<IOrganizationService>(() => s.CreateExtendedOrganizationService(null));
                    return new OrganizationServicesWrapper(
                        // Standard
                        s.Get<Lazy<IOrganizationService>>(), 
                        // Admin
                        admin,
                        // InitiatingUser
                        new Lazy<IOrganizationService>(() => s.CreateExtendedOrganizationService(s.Get<IPluginExecutionContext>().InitiatingUserId)),
                        // Cached
                        new Lazy<IOrganizationService>(() => new ReadOnlyCachedService(admin.Value)));
                })
                ;
            return container;
        }
    }
}
