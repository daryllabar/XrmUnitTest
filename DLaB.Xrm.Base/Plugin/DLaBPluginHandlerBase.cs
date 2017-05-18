using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// Plugin Handler Base.  Allows for Registered Events, preventing infinite loops, and auto logging
    /// </summary>
    // ReSharper disable once InconsistentNaming
#if DLAB_PUBLIC
    public abstract class DLaBPluginHandlerBase : GenericPluginHandlerBase<IExtendedPluginContext>
#else
    internal abstract class DLaBPluginHandlerBase : GenericPluginHandlerBase<IExtendedPluginContext>
#endif
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
