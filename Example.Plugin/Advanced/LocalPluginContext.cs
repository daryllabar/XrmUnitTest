using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin.Advanced
{
    // Create a Custom Plugin Context in case there are certain things that should be overridden
    public class LocalPluginContext : LocalPluginContextBase
    {
        public LocalPluginContext(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin) : base(serviceProvider, plugin)
        {
        }
    }
}
