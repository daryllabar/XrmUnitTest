using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;

namespace Example.Plugin
{
    public abstract class PluginBase : IRegisteredEventsPlugin
    {
        private readonly object _handlerLock = new object();
        private PluginHandlerBase _handler;
        private volatile bool _isIntialized;

        private String SecureConfig { get; set; }
        private String UnsecureConfig { get; set; }
        public IEnumerable<RegisteredEvent> RegisteredEvents { get { return ThreadSafeGetOrCreateHandler().RegisteredEvents; } }

        protected PluginBase(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ThreadSafeGetOrCreateHandler().Execute(serviceProvider);
        }

        private PluginHandlerBase ThreadSafeGetOrCreateHandler()
        {
            if (_handler != null) { return _handler; }

            if (_isIntialized) { return _handler; }

            lock (_handlerLock)
            {
                if (_isIntialized) { return _handler; }

                var local = GetPluginHandler();
                local.SetConfigValues(UnsecureConfig, SecureConfig);
                local.RegisterEvents();
                _handler = local;
                _isIntialized = true;
            }
            return _handler;
        }

        protected abstract PluginHandlerBase GetPluginHandler();
    }
}
