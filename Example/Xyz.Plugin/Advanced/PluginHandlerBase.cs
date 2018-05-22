using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Xyz.Plugin.Advanced
{
    // Create a Custom Plugin Handler Base to specify the type of PluginContext To Execute Against
    public abstract class PluginHandlerBase : GenericPluginHandlerBase<ExtendedPluginContext>
    {
        protected override ExtendedPluginContext CreatePluginContext(IServiceProvider serviceProvider)
        {
            return new ExtendedPluginContext(serviceProvider, this);
        }
    }
}
