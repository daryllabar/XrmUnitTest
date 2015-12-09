using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// Plugin Handler Base.  Allows for Registered Events, preventing infinite loops, and auto logging
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GenericPluginHandlerBase<T> : IRegisteredEventsPluginHandler where T : ILocalPluginContext
    {
        #region Properties

        /// <summary>
        /// Gets the List of RegisteredEvents that the plug-in should fire for
        /// </summary>
        public List<RegisteredEvent> RegisteredEvents { get; }
        /// <summary>
        /// Gets or sets the secure configuration.
        /// </summary>
        /// <value>
        /// The secure configuration.
        /// </value>
        protected string SecureConfig { get; set; }
        /// <summary>
        /// Gets or sets the unsecure configuration.
        /// </summary>
        /// <value>
        /// The unsecure configuration.
        /// </value>
        protected string UnsecureConfig { get; set; }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPluginHandlerBase{T}"/> class.
        /// </summary>
        protected GenericPluginHandlerBase()
        {
            RegisteredEvents = new List<RegisteredEvent>();
        }

        #endregion // Constructors

        #region Abstract Methods / Properties

        /// <summary>
        /// The default method to be executed by the plugin.  The Registered Event could specify a different method.
        /// </summary>
        /// <param name="context">The plugin context.</param>
        protected abstract void ExecuteInternal(T context);

        /// <summary>
        /// Adds all registered events to the RegisteredEvents Property;
        /// </summary>
        public abstract void RegisterEvents();

        /// <summary>
        /// Creates the local plugin context.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        protected abstract T CreateLocalPluginContext(IServiceProvider serviceProvider);

        #endregion // Abstract Methods / Properties

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances. 
        /// The plug-in's Execute method should be written to be stateless as the constructor 
        /// is not called for every invocation of the plug-in. Also, multiple system threads 
        /// could execute the plug-in at the same time. All per invocation state information 
        /// is stored in the context. This means that you should not use class level fields/properties in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            PreExecute(serviceProvider);

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var context = CreateLocalPluginContext(serviceProvider);
            
            try
            {
                context.TraceFormat("Entered {0}.Execute()", context.PluginTypeName);

                if (context.Event == null)
                {
                    var executionContext = context.PluginExecutionContext;
                    context.TraceFormat("No Registered Event Found for Event: {0}, Entity: {1}, and Stage: {2}!", executionContext.MessageName, executionContext.PrimaryEntityName, executionContext.Stage);
                    return; 
                }
                if (PreventRecursiveCall(context))
                {
                    context.Trace("Duplicate Recursive Call Prevented!");
                    return; 
                }
                var localContext = context as LocalPluginContextBase;
                if (localContext == null)
                {
                    context.Trace("Context was not of type LocalPluginContextBase.  Unable to check for Prevention Calls!");
                }

                if (localContext != null && localContext.HasPluginHandlerExecutionBeenPrevented())
                {
                    context.Trace("Context has Specified Call to be Prevented!");
                    return; 
                }

                ExecuteRegisteredEvent(context);
            }
            catch (InvalidPluginExecutionException ex)
            {
                context.LogException(ex);
                // This error is already being thrown from the plugin, just throw
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected Exception occurred, log exception then wrap and throw new exception
                context.LogException(ex);
                throw new InvalidPluginExecutionException(ex.Message, ex);
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
        /// Methods that gets called in the finally block of the Execute
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void PostExecute(T context) { }

        /// <summary>
        /// Sets the configuration values.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration.</param>
        /// <param name="secureConfig">The secure configuration.</param>
        public void SetConfigValues(string unsecureConfig = null, string secureConfig = null)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }

        /// <summary>
        /// Traces the Execution of the registered event of the context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ExecuteRegisteredEvent(T context)
        {
            var execute = context.Event.Execute == null ? ExecuteInternal : new Action<T>(c => context.Event.Execute(c)) ;

            context.TraceFormat("{0}.{1} is Executing for Entity: {2}, Message: {3}",
                context.PluginTypeName,
                execute.Method.Name,
                context.PluginExecutionContext.PrimaryEntityName,
                context.PluginExecutionContext.MessageName);

            execute(context);
        }

        /// <summary>
        /// Allows Plugin to trigger itself.  Delete Messge Types always return False since you can't delete something twice, all other message types return true if the execution key is found in the shared parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool PreventRecursiveCall(ILocalPluginContext context)
        {
            if (context.Event.Message == MessageType.Delete)
            {
                return false;
            }

            var sharedVariables = context.PluginExecutionContext.SharedVariables;
            var key = $"{context.PluginTypeName}|{context.Event.MessageName}|{context.Event.Stage}|{context.PluginExecutionContext.PrimaryEntityId}";
            if (context.PluginExecutionContext.GetFirstSharedVariable<int>(key) > 0)
            {
                return true;
            }

            sharedVariables.Add(key, 1);
            return false;
        }
    }
}