using System;
using System.Collections.Generic;
using DLaB.Xrm.Plugin;

namespace DLaB.Xrm.Plugin
{
    public abstract class GenericPluginBase<T> : IRegisteredEventsPlugin where T : ILocalPluginContext
    {
        private readonly object _handlerLock = new object();
        private GenericPluginHandlerBase<T> _handler;
        private volatile bool _isIntialized;

        protected string SecureConfig { get; }
        protected string UnsecureConfig { get; }
        public IEnumerable<RegisteredEvent> RegisteredEvents => ThreadSafeGetOrCreateHandler().RegisteredEvents;

        protected GenericPluginBase(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ThreadSafeGetOrCreateHandler().Execute(serviceProvider);
        }

        private IRegisteredEventsPluginHandler ThreadSafeGetOrCreateHandler()
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

        protected abstract GenericPluginHandlerBase<T> GetPluginHandler();
    }
}
