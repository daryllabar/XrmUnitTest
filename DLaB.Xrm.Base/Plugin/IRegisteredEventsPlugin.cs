using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// An IPlugin that defines Registered Events
    /// </summary>
#if DLAB_PUBLIC
    public interface IRegisteredEventsPlugin : IPlugin
#else
    internal interface IRegisteredEventsPlugin : IPlugin
#endif
    {
        /// <summary>
        /// Gets the registered events.
        /// </summary>
        /// <value>
        /// The registered events.
        /// </value>
        IEnumerable<RegisteredEvent> RegisteredEvents { get; }
    }
}
