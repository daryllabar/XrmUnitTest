using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;


#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    public interface IExtendedPluginContextInitializer
    {
        
        #region Initializers
        
        IPluginExecutionContext InitializePluginExecutionContext(IServiceProvider serviceProvider, ITracingService tracingService);

        IOrganizationServiceFactory InitializeServiceFactory(IServiceProvider serviceProvider, ITracingService tracingService);
        
        ITracingService InitializeTracingService(IServiceProvider serviceProvider);
        
        IOrganizationService InitializeIOrganizationService(IOrganizationServiceFactory factory, Guid? userId, ITracingService tracingService);

        #endregion Initializers
    }
}
