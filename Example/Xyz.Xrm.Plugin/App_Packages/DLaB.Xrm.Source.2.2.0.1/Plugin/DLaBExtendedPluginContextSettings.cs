using System;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    public class DLaBExtendedPluginContextSettings: IExtendedPluginContextInitializer
    {
        public ExtendedOrganizationServiceSettings OrganizationServiceSettings { get; set; }

        public DLaBExtendedPluginContextSettings()
        {
            OrganizationServiceSettings = new ExtendedOrganizationServiceSettings();
        }

        #region Initializers

        public virtual IOrganizationServiceFactory InitializeServiceFactory(IServiceProvider serviceProvider, ITracingService tracingService)
        {
            return serviceProvider.GetService<IOrganizationServiceFactory>();
        }

        public virtual ITracingService InitializeTracingService(IServiceProvider serviceProvider)
        {
            return new ExtendedTracingService(serviceProvider.GetService<ITracingService>());
        }

        public virtual IOrganizationService InitializeIOrganizationService(IOrganizationServiceFactory factory, Guid? userId, ITracingService tracingService)
        {
            return new ExtendedOrganizationService(factory.CreateOrganizationService(userId), tracingService, OrganizationServiceSettings);
        }

        public virtual IPluginExecutionContext InitializePluginExecutionContext(IServiceProvider serviceProvider, ITracingService tracingService)
        {
            return (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));
        }

        #endregion Initializers

    }
}
