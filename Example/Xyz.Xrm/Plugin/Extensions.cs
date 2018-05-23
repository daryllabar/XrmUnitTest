using System;
using Source.DLaB.Xrm.Plugin;

namespace Xyz.Xrm.Plugin
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
