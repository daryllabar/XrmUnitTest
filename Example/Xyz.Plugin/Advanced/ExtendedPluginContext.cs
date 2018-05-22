using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Xyz.Plugin.Advanced
{
    // Create a Custom Plugin Context in case there are certain things that should be overridden
    public class ExtendedPluginContext : DLaBExtendedPluginContextBase
    {
        public ExtendedPluginContext(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin) : base(serviceProvider, plugin)
        {
        }
    }
}
