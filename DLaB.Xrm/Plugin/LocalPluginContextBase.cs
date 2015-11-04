using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DLaB.Common;
using DLaB.Xrm.Exceptions;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    public interface ILocalPluginContext
    {
        #region Properties

        /// <summary>
        /// The current event the plugin is executing for.
        /// </summary>
        RegisteredEvent Event { get; }

        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that plugin is registered to run as, useing the PluginExecutionContext.UserId.
        /// </summary>
        IOrganizationService OrganizationService { get; }

        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that triggered the services using the PluginExecutionContext.InitiatingUserId.
        /// </summary>
        IOrganizationService InitiatingUserOrganizationService { get; }


        /// <summary>
        /// The IOrganizationService of the plugin, using the System User by not specifying a UserId.
        /// </summary>
        IOrganizationService SystemOrganizationService { get; }

        /// <summary>
        /// The IPluginExecutionContext of the plugin.
        /// </summary>
        IPluginExecutionContext PluginExecutionContext { get; }

        /// <summary>
        /// The Type.FullName of the plugin.
        /// </summary>
        String PluginTypeName { get; }

        /// <summary>
        /// The ITracingService of the plugin.
        /// </summary>
        ITracingService TracingService { get; }

        EntityReference GetPrimaryEntity { get; }

        #endregion Properties

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void LogException(Exception ex);

        /// <summary>
        /// Traces the specified message.  Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        void Trace(string message);

        /// <summary>
        /// Traces the format.   Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        void TraceFormat(string format, params object[] args);
    }

    public abstract class LocalPluginContextBase : ILocalPluginContext
    {
        #region Properties

        /// <summary>
        /// The current PluginExecutionContext and the parent context hierarchy of the plugin.
        /// </summary>
        public IEnumerable<IPluginExecutionContext> Contexts
        {
            get
            {
                yield return PluginExecutionContext;
                foreach (var parent in PluginExecutionContext.GetParentContexts())
                {
                    yield return parent;
                }
                ;
            }
        }

        /// <summary>
        /// The current event the plugin is executing for.
        /// </summary>
        public RegisteredEvent Event { get; private set; }
        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that the plugin is registered to run as.
        /// </summary>
        public IOrganizationService OrganizationService => _organizationService ?? (_organizationService = ServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId));

        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that the plugin is was initiated by
        /// </summary>
        public IOrganizationService InitiatingUserOrganizationService => _triggeredUserOrganizationService ?? (_triggeredUserOrganizationService = ServiceFactory.CreateOrganizationService(PluginExecutionContext.InitiatingUserId));

        /// <summary>
        /// The IOrganizationService of the plugin, using the System User
        /// </summary>
        public IOrganizationService SystemOrganizationService => _systemOrganizationService ?? (_systemOrganizationService = ServiceFactory.CreateOrganizationService(null));

        private IOrganizationService _organizationService;
        private IOrganizationService _triggeredUserOrganizationService;
        private IOrganizationService _systemOrganizationService;
        private IOrganizationServiceFactory ServiceFactory { get; set; }
        /// <summary>
        /// The IPluginExecutionContext of the plugin.
        /// </summary>
        public IPluginExecutionContext PluginExecutionContext { get; private set; }
        /// <summary>
        /// The Type.FullName of the plugin.
        /// </summary>
        public String PluginTypeName { get; private set; } 
        /// <summary>
        /// The ITracingService of the plugin.
        /// </summary>
        public ITracingService TracingService { get; private set; }

        #endregion // Properties

        #region ImageNames struct
		
        public struct PluginImageNames
        {
            public const string PreImage = "PreImage";
            public const string PostImage = "PostImage";
        }
		
	    #endregion ImageNames struct

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalPluginContextBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="plugin">The plugin.</param>
        /// <exception cref="System.ArgumentNullException">
        /// serviceProvider
        /// or
        /// plugin
        /// </exception>
        protected LocalPluginContextBase(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            if (plugin == null)
            {
                throw new ArgumentNullException("plugin");
            }

            InitializeServiceProviderProperties(serviceProvider);
            InitializePluginProperties(PluginExecutionContext, plugin);
        }

        #endregion // Constructors

        #region AssertEntityImageAttributesRegistered

        /// <summary>
        /// Checks the Pre/Post Entity Images to determine if the image collections contains an image with the given given key, that contains the attributes
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="attributeNames"></param>
        public void AssertEntityImageAttributesRegistered(string imageName, params string[] attributeNames)
        {
            Entity image;
            var imageCollection = InvalidPluginStepRegistrationException.ImageCollection.Pre;
            if (PluginExecutionContext.PostEntityImages.TryGetValue(imageName, out image))
            {
                imageCollection = InvalidPluginStepRegistrationException.ImageCollection.Post;
            }
            else
            {
                PluginExecutionContext.PreEntityImages.TryGetValue(imageName, out image);
            }
            
            if(image == null)
            {
                throw InvalidPluginStepRegistrationException.ImageMissing(imageName);
            }

            var missingAttributes = attributeNames.Where(attribute => !image.Contains(attribute)).ToList();

            if (!missingAttributes.Any()) { return; }

            throw InvalidPluginStepRegistrationException.ImageMissingRequiredAttributes(imageCollection, imageName, missingAttributes);
        }

        #endregion AssertEntityImageAttributesRegistered
        
        #region CalledFrom

        /// <summary>
        /// Returns true if the current plugin maps to the Registered Event, or the current plugin has been triggered by the given registered event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public bool CalledFrom(RegisteredEvent @event)
        {
            return Contexts.Any(c => c.MessageName == @event.MessageName && c.PrimaryEntityName == @event.EntityLogicalName && c.Stage == (int) @event.Stage);
        }

        /// <summary>
        /// Returns true if the current plugin maps to the parameters given, or the current plugin has been triggered by the given parameters
        /// </summary>
        /// <returns></returns>
        public bool CalledFrom(string entityLogicalName = null, MessageType message = null, int? stage = null)
        {
            if (message == null && entityLogicalName == null && stage == null)
            {
                throw new Exception("At least one parameter for LocalPluginContextBase.CalledFrom must be populated");
            }
            return Contexts.Any(c =>
                (message == null || c.MessageName == message.Name) && 
                (entityLogicalName == null || c.PrimaryEntityName == entityLogicalName) && 
                (stage == null || c.Stage == stage.Value));
        }

        #endregion CalledFrom

        #region PropertyInitializers

        /// <summary>
        /// Initializes the IServiceProvider properties.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        private void InitializeServiceProviderProperties(IServiceProvider serviceProvider)
        {
            PluginExecutionContext = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
            TracingService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
            ServiceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
        }

        /// <summary>
        /// Initializes the plugin properties.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="plugin">The plugin.</param>
        private void InitializePluginProperties(IPluginExecutionContext context, IRegisteredEventsPluginHandler plugin)
        {
            // Iterate over all of the expected registered events to ensure that the plugin
            // has been invoked by an expected event
            // For any given plug-in event at an instance in time, we would expect at most 1 result to match.
            Event = plugin.RegisteredEvents.FirstOrDefault(a =>
                (int)a.Stage == context.Stage &&
                a.MessageName == context.MessageName &&
                (string.IsNullOrWhiteSpace(a.EntityLogicalName) || a.EntityLogicalName == context.PrimaryEntityName)
                );

            PluginTypeName = plugin.GetType().FullName;
        }

        #endregion // PropertyInitializers

        #region Trace

        /// <summary>
        /// Traces the specified message.  Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Trace(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null)
                {
                    return;
                }

                if (PluginExecutionContext == null)
                {
                    TracingService.Trace(message);
                }
                else
                {
                    TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        PluginExecutionContext.CorrelationId,
                        PluginExecutionContext.InitiatingUserId);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    TracingService.Trace("Exception occured attempting to trace {0}: {1}", message, ex);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // Attempted to trace a message, and had an exception, and then had another exception attempting to trace the exception that occured when tracing.
                    // Better to give up rather than stopping the entire program when attempting to write a Trace message
                }
            }
        }

        /// <summary>
        /// Traces the format.   Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void TraceFormat(string format, params object[] args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(format) || TracingService == null)
                {
                    return;
                }
                var message = String.Format(CultureInfo.InvariantCulture, format, args);
                TracingService.Trace(PluginExecutionContext == null ?
                    message :
                    String.Format("{0}, Corralation Id: {1}, Initiating User: {2}", message, PluginExecutionContext.CorrelationId, PluginExecutionContext.InitiatingUserId));
            }
            catch (Exception ex)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    TracingService.Trace("Exception occured attempting to trace {0}: {1}", format, ex);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // Attempted to trace a message, and had an exception, and then had another exception attempting to trace the exception that occured when tracing.
                    // Better to give up rather than stopping the entire program when attempting to write a Trace message
                }
            }
        }

        #endregion // Trace

        #region ContainsAllNonNull

        /// <summary>
        /// Checks to see if the PluginExecutionContext.InputParameters Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <returns></returns>
        public bool InputContainsAllNonNull(params string[] parameterNames)
        {
            return PluginExecutionContext.InputParameters.ContainsAllNonNull(parameterNames);
        }

        /// <summary>
        /// Checks to see if the PluginExecutionContext.OutputParameters Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <returns></returns>
        public bool OutputContainsAllNonNull(params string[] parameterNames)
        {
            return PluginExecutionContext.OutputParameters.ContainsAllNonNull(parameterNames);
        }

        /// <summary>
        /// Checks to see if the PluginExecutionContext.SharedVariables Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <returns></returns>
        public bool SharedContainsAllNonNull(params string[] parameterNames)
        {
            return PluginExecutionContext.SharedVariables.ContainsAllNonNull(parameterNames);
        }

        #endregion // ContainsAllNonNull

        #region GetParameterValue

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.InputParameters collection, cast to type 'T', or default(T) if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the parameter to be returned</typeparam>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public T GetInputParameterValue<T>(string parameterName)
        {
            return PluginExecutionContext.InputParameters.GetParameterValue<T>(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.InputParameters collection, or null if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public object GetInputParameterValue(string parameterName)
        {
            return PluginExecutionContext.InputParameters.GetParameterValue(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.OutputParameters collection, cast to type 'T', or default(T) if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the parameter to be returned</typeparam>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public T GetOutputParameterValue<T>(string parameterName)
        {
            return PluginExecutionContext.OutputParameters.GetParameterValue<T>(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.OutputParameters collection, or null if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public object GetOutputParameterValue(string parameterName)
        {
            return PluginExecutionContext.OutputParameters.GetParameterValue(parameterName);
        }

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables collection, cast to type 'T', or default(T) if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the variable to be returned</typeparam>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public T GetSharedVariable<T>(string variableName)
        {
            return PluginExecutionContext.SharedVariables.GetParameterValue<T>(variableName);
        }

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables collection, or null if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public object GetSharedVariable(string variableName)
        {
            return PluginExecutionContext.SharedVariables.GetParameterValue(variableName);
        }

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables or anywhere in the Plugin Context Hierarchy collection, cast to type 'T', or default(T) if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the variable to be returned</typeparam>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public T GetFirstSharedVariable<T>(string variableName)
        {
            var context = PluginExecutionContext;
            while (context != null)
            {
                if (context.SharedVariables.ContainsKey(variableName))
                {
                    return context.SharedVariables.GetParameterValue<T>(variableName);
                }
                context = context.ParentContext;
            }
            return default(T);
        }

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables or anywhere in the Plugin Context Hierarchy collection, or null if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public object GetFirstSharedVariable(string variableName)
        {
            return PluginExecutionContext.GetFirstSharedVariable(variableName);
        }

        #endregion // GetParameterValue

        #region Exception Logging

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public virtual void LogException(Exception ex)
        {
            TraceFormat("Exception: {0}", ex.ToStringWithCallStack());
            Trace(GetContextInfo());
        }

        #endregion Exception Logging

        #region Retrieve Entity From Context

        /// <summary>
        /// Creates a new Entity of type T, adding the attributes from both the Target and the Post Image if they exist.  
        /// Does not return null.
        /// Does not return a reference to Target
        /// </summary>
        /// <param name="imageName">Name of the image.</param>
        /// <returns></returns>
        public T CoallesceTargetWithPostEntity<T>(string imageName = PluginImageNames.PostImage) where T : Entity
        {
            var entity = (T) Activator.CreateInstance(typeof (T));
            var target = GetTarget<T>();
            if (target != null)
            {
                entity.Id = target.Id;
                foreach (var attribute in target.Attributes)
                {
                    entity[attribute.Key] = attribute.Value;
                }
            }

            var postImage = GetPostEntity<T>(imageName);
            if (postImage == null)
            {
                return entity;
            }

            if (entity.Id == Guid.Empty)
            {
                entity.Id = postImage.Id;
            }

            foreach (var attribute in postImage.Attributes.Where(a => !entity.Contains(a.Key)))
            {
                entity[attribute.Key] = attribute.Value;
            }

            return entity;
        }

        /// <summary>
        /// If the PreEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public T GetPreEntity<T>(string imageName = PluginImageNames.PreImage) where T : Entity
        {
            return GetEntity<T>(PluginExecutionContext.PreEntityImages, imageName);
        }

        /// <summary>
        /// If the PostEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public T GetPostEntity<T>(string imageName = PluginImageNames.PostImage) where T : Entity
        {
            return GetEntity<T>(PluginExecutionContext.PostEntityImages, imageName);
        }

        /// <summary>
        /// If the images collection contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="images"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private T GetEntity<T>(DataCollection<string, Entity> images, string imageName) where T : Entity
        {
            T entity = null;
            if (!images.ContainsKey(imageName))
            {
                return null;
            }

            try
            {
                entity = (images[imageName]).ToEntity<T>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return entity;
        }

        /// <summary>
        /// Cast the Target to the given Entity Type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTarget<T>() where T : Entity
        {
            T entity = null;
            var parameters = PluginExecutionContext.InputParameters;
            if (!parameters.ContainsKey(ParameterName.Target) || !(parameters[ParameterName.Target] is Entity))
            {
                return null;
            }

            // Obtain the target business entity from the input parmameters.
            try
            {
                entity = ((Entity)parameters[ParameterName.Target]).ToEntity<T>();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return entity;
        }

        /// <summary>
        /// Finds and returns the Target as an Entity Reference (Delete Plugins)
        /// </summary>
        /// <returns></returns>
        public EntityReference GetTargetEntityReference()
        {
            EntityReference entity = null;
            var parameters = PluginExecutionContext.InputParameters;
            if (parameters.ContainsKey(ParameterName.Target) &&
                 parameters[ParameterName.Target] is EntityReference)
            {
                entity = (EntityReference)parameters[ParameterName.Target];
            }
            return entity;
        }

        public EntityReference GetPrimaryEntity
        {
            get
            {
                return new EntityReference(PluginExecutionContext.PrimaryEntityName, PluginExecutionContext.PrimaryEntityId);
            }
        }

        #endregion Retrieve Entity From Context

        #region Prevent Plugin Execution

        #region PreventPluginHandlerExecution

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="messageType">Type of the message.</param>
        public void PreventPluginHandlerExecution(String handlerTypeFullName, MessageType messageType)
        {
            PreventPluginHandlerExecution(handlerTypeFullName, messageType.Name);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="event">Type of the event.</param>
        public void PreventPluginHandlerExecution(String handlerTypeFullName, RegisteredEvent @event)
        {
            if (@event == null) throw new ArgumentNullException("event");

            PreventPluginHandlerExecution(handlerTypeFullName, @event.MessageName, @event.EntityLogicalName, @event.Stage);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="messageName"></param>
        /// <param name="logicalName"></param>
        /// <param name="stage"></param>
        public void PreventPluginHandlerExecution(String handlerTypeFullName, string messageName = null, string logicalName = null, PipelineStage? stage = null)
        {
            var preventionName = GetPreventPluginHandlerSharedVariableName(handlerTypeFullName);
            object value;
            if (!PluginExecutionContext.SharedVariables.TryGetValue(preventionName, out value))
            {
                value = new Entity();
                PluginExecutionContext.SharedVariables.Add(preventionName, value);
            }

            // Wish I could use a Hash<T> here, but CRM won't serialize it.  I'll Hack the Entity Object for now
            var hash = ((Entity)value).Attributes;
            var rule = GetPreventionRule(messageName, logicalName, stage);
            if (!hash.Contains(rule))
            {
                hash.Add(rule, null);
            }
        }

        #endregion PreventPluginHandlerExecution

        #region PreventPluginHandlerExecution<T>

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <param name="messageType">Type of the message.</param>
        public void PreventPluginHandlerExecution<T>(MessageType messageType)
            where T : IRegisteredEventsPluginHandler
        {
            PreventPluginHandlerExecution<T>(messageType.Name);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <param name="event">Type of the event.</param>
        public void PreventPluginHandlerExecution<T>(RegisteredEvent @event)
            where T : IRegisteredEventsPluginHandler
        {
            PreventPluginHandlerExecution(typeof (T).FullName, @event);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        public void PreventPluginHandlerExecution<T>(string messageName = null, string logicalName = null, PipelineStage? stage = null)
            where T : IRegisteredEventsPluginHandler
        {
            PreventPluginHandlerExecution(typeof(T).FullName, messageName, logicalName, stage);
        }

        #endregion PreventPluginHandlerExecution<T>

        private static string GetPreventionRule(string messageName = null, string logicalName = null, PipelineStage? stage = null)
        {
            var rule = messageName == null ? String.Empty : "MessageName:" + messageName + "|";
            rule += logicalName == null ? String.Empty : "LogicalName:" + logicalName + "|";
            rule += stage == null ? String.Empty : "Stage:" + stage + "|";
            return rule;
        }

        private static string GetPreventPluginHandlerSharedVariableName(string pluginTypeName)
        {
            return pluginTypeName + "PreventExecution";
        }

        #region HasPluginHandlerExecutionBeenPrevented

        public bool HasPluginHandlerExecutionBeenPrevented()
        {
            return HasPluginHandlerExecutionBeenPreventedInternal(Event, GetPreventPluginHandlerSharedVariableName(PluginTypeName));
        }

        /// <summary>
        /// Determines whether a shared variable exists that specifies that the plugin or the plugin and specifc message type should be prevented from executing
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <returns></returns>
        public bool HasPluginHandlerExecutionBeenPrevented<T>(RegisteredEvent @event)
            where T : IRegisteredEventsPluginHandler
        {
            var preventionName = GetPreventPluginHandlerSharedVariableName(typeof(T).FullName);
            return HasPluginHandlerExecutionBeenPreventedInternal(@event, preventionName);
        }

        private bool HasPluginHandlerExecutionBeenPreventedInternal(RegisteredEvent @event, string preventionName)
        {
            var value = GetFirstSharedVariable(preventionName);
            if (value == null)
            {
                return false;
            }

            var hash = ((Entity) value).Attributes;
            return hash.Contains(String.Empty) ||
                   hash.Contains(GetPreventionRule(@event.MessageName)) ||
                   hash.Contains(GetPreventionRule(@event.MessageName, @event.EntityLogicalName)) ||
                   hash.Contains(GetPreventionRule(@event.MessageName, stage: @event.Stage)) ||
                   hash.Contains(GetPreventionRule(@event.MessageName, @event.EntityLogicalName, @event.Stage)) ||
                   hash.Contains(GetPreventionRule(logicalName: @event.EntityLogicalName)) ||
                   hash.Contains(GetPreventionRule(logicalName: @event.EntityLogicalName, stage: @event.Stage)) ||
                   hash.Contains(GetPreventionRule(stage: @event.Stage));
        }

        #endregion HasPluginHandlerExecutionBeenPrevented

        #endregion Prevent Plugin Execution

        #region Diagnostics

        public String GetContextInfo()
        {
            return
                "**** Context Info ****" + Environment.NewLine +
                "Plugin: " + PluginTypeName + Environment.NewLine +
                "* Registered Event *" + Environment.NewLine + Event.ToString("   ") + Environment.NewLine +
                PluginExecutionContext.ToStringDebug();
        }

        #endregion Diagnostics
    }
}