using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
	
{
    /// <summary>
    /// Plugin Handler Base.  Allows for Registered Events, preventing infinite loops, and auto logging
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class DLaBPluginHandlerBase : GenericPluginHandlerBase<IExtendedPluginContext>
    {
        /// <summary>
        /// Creates the plugin context.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        protected override IExtendedPluginContext CreatePluginContext(IServiceProvider serviceProvider)
        {
            return new DLaBExtendedPluginContextBase(serviceProvider, this);
        }
    }
}
