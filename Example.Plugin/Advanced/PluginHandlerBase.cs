using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin.Advanced
{
    // Create a Custom Plugin Handler Base to specify the type of PluginContext To Execute Against
    public abstract class PluginHandlerBase : GenericPluginHandlerBase<LocalPluginContext>
    {
        protected override LocalPluginContext CreateLocalPluginContext(IServiceProvider serviceProvider)
        {
            return new LocalPluginContext(serviceProvider, this);
        }
    }
}
