using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin.Advanced
{
    public abstract class PluginBase : GenericPluginBase<LocalPluginContext>
    {
        protected PluginBase(string unsecureConfig, string secureConfig) : base(unsecureConfig,secureConfig)
        {
        }
    }
}
