using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    public interface IRegisteredEventsPlugin : IPlugin
    {
        IEnumerable<RegisteredEvent> RegisteredEvents { get; }
    }
}
