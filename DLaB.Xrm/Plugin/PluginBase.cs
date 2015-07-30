using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    public abstract class PluginBase : IRegisteredEventsPlugin
    {
        #region Properties

        /// <summary>
        /// Gets the List of RegisteredEvents that the plug-in should fire for
        /// </summary>
        public List<RegisteredEvent> RegisteredEvents { get; private set; }
        protected String SecureConfig { get; set; }
        protected String UnsecureConfig { get; set; }

        #endregion // Properties

        #region Constructors

        protected PluginBase() : this(null,null)
        {

        }

        protected PluginBase(string unsecureConfig, string secureConfig)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
            RegisteredEvents = new List<RegisteredEvent>();
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            PopulateRegisteredEvents();
        }

        #endregion // Constructors

        #region Abstract Methods / Properties

        /// <summary>
        /// The default method to be executed by the plugin.  The Registered Event could specify a different method.
        /// </summary>
        /// <param name="context">The plugin context.</param>
        protected abstract void ExecuteInternal(LocalPluginContext context);
        /// <summary>
        /// Forces the Base class to specify RegistrationEvents.  This will be exectued before the derived class's constructor runs! 
        /// </summary>
        protected abstract void PopulateRegisteredEvents();

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
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            var context = new LocalPluginContext(serviceProvider, this);
            
            try
            {
                context.TraceFormat("Entered {0}.Execute()", context.PluginTypeName);

                if (context.Event == null)
                {
                    context.Trace("No Registered Event Found!");
                    return; 
                }
                if (PreventRecursiveCall(context))
                {
                    context.Trace("Duplicate Recursive Call Prevented!");
                    return; 
                }

                ExecuteRegisteredEvent(context);
            }
            catch (InvalidPluginExecutionException ex)
            {
                context.LogException(ex);
                context.Trace(context.GetContextInfo());
                // This error is already being thrown from the plugin, just throw
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected Exception occurred, log exception then wrap and throw new exception
                context.LogException(ex);
                context.Trace(context.GetContextInfo());
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
            finally
            {
                context.TraceFormat("Exiting {0}.Execute()", context.PluginTypeName);
            }
        }

        /// <summary>
        /// Traces the Execution of the registered event of the context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ExecuteRegisteredEvent(LocalPluginContext context)
        {
            var execute = context.Event.Execute ?? ExecuteInternal;

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
        protected virtual bool PreventRecursiveCall(LocalPluginContext context)
        {
            if (context.Event.Message == MessageType.Delete)
            {
                return false;
            }

            var sharedVariables = context.PluginExecutionContext.SharedVariables;
            var key = String.Format("{0}|{1}|{2}", context.PluginTypeName, context.Event.MessageName, context.PluginExecutionContext.PrimaryEntityId);
            if (sharedVariables.ContainsKey(key))
            {
                return true;
            }

            sharedVariables.Add(key, 0);
            return false;
        }
    }
}