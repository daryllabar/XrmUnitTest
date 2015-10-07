using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin
{
    public static class Extensions
    {
        #region RegisteredEventBuilder

        public static RegisteredEventBuilder WithExecuteAction(this RegisteredEventBuilder builder, Action<LocalPluginContext> execute)
        {
            builder.WithExecuteAction(execute);
            return builder;
        }

        #endregion RegisteredEventBuilder
    }
}
