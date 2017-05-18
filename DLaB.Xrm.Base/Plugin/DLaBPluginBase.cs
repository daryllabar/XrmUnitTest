using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// An abstract base Plugin that Implments the IRegisteredEventsPlugin interface
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class DLaBPluginBase : IRegisteredEventsPlugin 
    {
        private readonly object _handlerLock = new object();
        private IRegisteredEventsPluginHandler _handler;
        private volatile bool _isIntialized;

        /// <summary>
        /// Gets the secure configuration.
        /// </summary>
        /// <value>
        /// The secure configuration.
        /// </value>
        protected string SecureConfig { get; }
        /// <summary>
        /// Gets the unsecure configuration.
        /// </summary>
        /// <value>
        /// The unsecure configuration.
        /// </value>
        protected string UnsecureConfig { get; }
        /// <summary>
        /// Gets the registered events.
        /// </summary>
        /// <value>
        /// The registered events.
        /// </value>
        public IEnumerable<RegisteredEvent> RegisteredEvents => ThreadSafeGetOrCreateHandler().RegisteredEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLaBPluginBase"/> class.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration.</param>
        /// <param name="secureConfig">The secure configuration.</param>
        protected DLaBPluginBase(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        /// <summary>
        /// Executes the plugin
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
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

        /// <summary>
        /// Gets the plugin handler.
        /// </summary>
        /// <returns></returns>
        protected abstract IRegisteredEventsPluginHandler GetPluginHandler();
    }
}
