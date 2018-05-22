using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
	
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
        /// Adds all registered events to the RegisteredEvents Property;
        /// </summary>
        void RegisterEvents();


        /// <summary>
        /// Sets the configuration values.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration.</param>
        /// <param name="secureConfig">The secure configuration.</param>
        void SetConfigValues(string unsecureConfig = null, string secureConfig = null);
    }
}
