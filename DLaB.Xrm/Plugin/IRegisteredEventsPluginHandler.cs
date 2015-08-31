using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    public interface IRegisteredEventsPluginHandler : IPlugin
    {
        List<RegisteredEvent> RegisteredEvents { get; }
    }
}
