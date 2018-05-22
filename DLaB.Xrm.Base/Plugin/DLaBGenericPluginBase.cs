using System;
using System.Collections.Generic;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
	
{
    /// <inheritdoc />
    /// <summary>
    /// Plugin Handler Base.  Allows for Registered Events, preventing infinite loops, and auto logging
    /// </summary>
    public abstract class DLaBGenericPluginBase<T> : IRegisteredEventsPlugin where T: IExtendedPluginContext
    {
        #region Properties

        private readonly object _handlerLock = new object();
        private volatile bool _isIntialized;

        /// <inheritdoc />
        public IEnumerable<RegisteredEvent> RegisteredEvents { get; private set; }

        /// <summary>
        /// Gets or sets the secure configuration.
        /// </summary>
        /// <value>
        /// The secure configuration.
        /// </value>
        public string SecureConfig { get; }
        /// <summary>
        /// Gets or sets the unsecure configuration.
        /// </summary>
        /// <value>
        /// The unsecure configuration.
        /// </value>
        public string UnsecureConfig { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the GenericPluginHandlerBase class.
        /// </summary>
        /// <param name="unsecureConfig"></param>
        /// <param name="secureConfig"></param>
        protected DLaBGenericPluginBase(string unsecureConfig, string secureConfig)
        {
            SecureConfig = secureConfig;
            UnsecureConfig = unsecureConfig;
        }

        #endregion Constructors

        #region Abstract Methods

        /// <summary>
        /// Creates the local plugin context.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        protected abstract T CreatePluginContext(IServiceProvider serviceProvider);

        /// <summary>
        /// The default method to be executed by the plugin.  The Registered Event could specify a different method.
        /// </summary>
        /// <param name="context">The plugin context.</param>
        protected abstract void ExecuteInternal(T context);

        /// <inheritdoc />
        public abstract IEnumerable<RegisteredEvent> GetRegisterEvents();

        #endregion Abstract Methods

        #region Initialize

        private void Initialize()
        {
            if (_isIntialized) { return; }

            lock (_handlerLock)
            {
                if (_isIntialized) { return; }

                PreInitialize();
                RegisteredEvents = GetRegisterEvents();
                _isIntialized = true;
                PostInitialize();
            }
        }

        /// <summary>
        /// Called once directly before the initialization of the plugin instance, before the first PreExecute is called
        /// </summary>
        protected virtual void PreInitialize() { }

        /// <summary>
        /// Called once directly after the initialization of the plugin instance, before the first PreExecute is called
        /// </summary>
        protected virtual void PostInitialize() { }


        #endregion Initialize

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances.
        /// The plug-in's Execute method should be written to be stateless as the constructor
        /// is not called for every invocation of the plug-in. Also, multiple system threads
        /// could execute the plug-in at the same time. All per invocation state information
        /// is stored in the context. This means that you should not use class level fields/properties in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (!_isIntialized)
            {
                Initialize();
            }
            PreExecute(serviceProvider);

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var context = CreatePluginContext(serviceProvider);

            try
            {
                context.TraceFormat("Entered {0}.Execute()", context.PluginTypeName);

                if (context.Event == null)
                {
                    context.TraceFormat("No Registered Event Found for Event: {0}, Entity: {1}, and Stage: {2}!", context.MessageName, context.PrimaryEntityName, context.Stage);
                    return;
                }

                if (PreventRecursiveCall(context))
                {
                    context.Trace("Duplicate Recursive Call Prevented!");
                    return;
                }

                if (context.HasPluginHandlerExecutionBeenPrevented())
                {
                    context.Trace("Context has Specified Call to be Prevented!");
                    return;
                }

                if (SkipExecution(context))
                {
                    context.Trace("Execution Has Been Skipped!");
                    return;
                }

                ExecuteRegisteredEvent(context);
            }
            catch (Exception ex)
            {
                context.LogException(ex);
                // Unexpected Exception occurred, log exception then wrap and throw new exception
                if (context.IsolationMode == IsolationMode.Sandbox)
                {
                    Sandbox.ExceptionHandler.AssertCanThrow(ex);
                }
                throw;
            }
            finally
            {
                context.TraceFormat("Exiting {0}.Execute()", context.PluginTypeName);
                PostExecute(context);
            }
        }

        /// <summary>
        /// Method that gets called before the Execute
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        protected virtual void PreExecute(IServiceProvider serviceProvider) { }

        /// <summary>
        /// Method that gets called in the finally block of the Execute
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void PostExecute(IExtendedPluginContext context) { }

        /// <summary>
        /// Method that gets called directly before Execute(context).  Returning true will skip the Execute(context) from getting called.  
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool SkipExecution(T context) { return false; }

        /// <summary>
        /// Traces the Execution of the registered event of the context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ExecuteRegisteredEvent(T context)
        {
            var execute = context.Event.Execute == null ? ExecuteInternal : new Action<T>(c => context.Event.Execute(c));

            context.TraceFormat("{0}.{1} is Executing for Entity: {2}, Message: {3}",
                context.PluginTypeName,
                context.Event.ExecuteMethodName,
                context.PrimaryEntityName,
                context.MessageName);

            execute(context);
        }

        /// <summary>
        /// Allows Plugin to trigger itself.  Delete Messge Types always return False since you can't delete something twice, all other message types return true if the execution key is found in the shared parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool PreventRecursiveCall(IExtendedPluginContext context)
        {
            if (context.Event.Message == MessageType.Delete)
            {
                return false;
            }

            var sharedVariables = context.SharedVariables;
            var key = $"{context.PluginTypeName}|{context.Event.MessageName}|{context.Event.Stage}|{context.PrimaryEntityId}";
            if (context.GetFirstSharedVariable<int>(key) > 0)
            {
                return true;
            }

            sharedVariables.Add(key, 1);
            return false;
        }
    }
}
