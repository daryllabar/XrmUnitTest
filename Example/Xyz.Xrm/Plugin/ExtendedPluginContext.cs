using System;
using Source.DLaB.Xrm.Plugin;

namespace Xyz.Xrm.Plugin
{
    // Create a Custom Plugin Context in case there are certain things that should be overridden
    public class ExtendedPluginContext : DLaBExtendedPluginContextBase
    {
        public ExtendedPluginContext(IServiceProvider serviceProvider, IRegisteredEventsPlugin plugin) : base(serviceProvider, plugin)
        {
        }
    }
}
