using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRegisteredEventsPluginHandler : IPlugin
    {
        /// <summary>
        /// Gets the registered events.
        /// </summary>
        /// <value>
        /// The registered events.
        /// </value>
        List<RegisteredEvent> RegisteredEvents { get; }

        /// <summary>
        /// Sets the configuration values.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration.</param>
        /// <param name="secureConfig">The secure configuration.</param>
        void SetConfigValues(string unsecureConfig = null, string secureConfig = null);
    }
}
