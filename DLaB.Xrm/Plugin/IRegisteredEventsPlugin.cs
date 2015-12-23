using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// An IPlugin that defines Registered Events
    /// </summary>
    public interface IRegisteredEventsPlugin : IPlugin
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
