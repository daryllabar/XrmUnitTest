using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin.Advanced
{
    public static class Extensions
    {
        #region RegisteredEventBuilder

        // Create Extension Method to Accept the Custom PluginContext

        public static RegisteredEventBuilder WithExecuteAction(this RegisteredEventBuilder builder, Action<ExtendedPluginContext> execute)
        {
            builder.WithExecuteAction(execute);
            return builder;
        }

        #endregion RegisteredEventBuilder
    }
}
