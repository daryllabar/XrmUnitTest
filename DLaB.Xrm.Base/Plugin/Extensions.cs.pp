using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
#if DLAB_UNROOT_NAMESPACE
using DLaB.Xrm.Exceptions;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Exceptions;

namespace Source.DLaB.Xrm.Plugin
#endif	
{
    /// <summary>
    /// Extension Class for Plugins
    /// </summary>
    public static class Extensions
    {
        #region List<RegisteredEvent>

        #region AddEvent

        /// <summary>
        /// Defaults the execute method to be InternalExecute and run against all entities.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, MessageType message)
        {
            events.AddEvent(stage, null, message, null);
        }

        /// <summary>
        /// Runs against all entities.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        /// <param name="execute"></param>
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, MessageType message, Action<IExtendedPluginContext> execute)
        {
            events.AddEvent(stage, null, message, execute);
        }

        /// <summary>
        /// Defaults the execute method to be InternalExecute and runs against the specified entity.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="message"></param>
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, MessageType message)
        {
            events.AddEvent(stage, entityLogicalName, message, null);
        }

        /// <summary>
        /// Runs against the specified entity
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="message"></param>
        /// <param name="execute"></param>
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, MessageType message, Action<IExtendedPluginContext> execute){
            events.Add(new RegisteredEvent(stage, message, execute, entityLogicalName));
        }

        #endregion AddEvent

        #region AddEvents

        /// <summary>
        /// Defaults the execute method to be InternalExecute and run against all entities.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="messages"></param>
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, params MessageType[] messages)
        {
            events.AddEvents(stage, null, null, messages);
        }

        /// <summary>
        /// Defaults the execute method to be InternalExecute and runs against the specified entity.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="messages"></param>
        /// <param name="entityLogicalName"></param>
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, params MessageType[] messages)
        {
            events.AddEvents(stage, entityLogicalName, null, messages);
        }

        /// <summary>
        /// Runs against all entities.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="messages"></param>
        /// <param name="execute"></param>
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, Action<IExtendedPluginContext> execute, params MessageType[] messages)
        {
            events.AddEvents(stage, null, execute, messages);
        }


        /// <summary>
        /// Runs against the specified entity
        /// </summary>
        /// <param name="events"></param>
        /// <param name="stage"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="execute"></param>
        /// <param name="messages"></param>
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, Action<IExtendedPluginContext> execute, params MessageType[] messages)
        {
            foreach (var message in messages)
            {
                events.Add(new RegisteredEvent(stage, message, execute, entityLogicalName));
            }
        }

        #endregion AddEvent

        #endregion List<RegisteredEvent>

        #region IExtendedPluginContext

        #region GetContextInfo

        /// <summary>
        /// Gets the context information.
        /// </summary>
        /// <returns></returns>
        public static string GetContextInfo(this IExtendedPluginContext context)
        {
            return
                "**** Context Info ****" + Environment.NewLine +
                "Plugin: " + context.PluginTypeName + Environment.NewLine +
                "* Registered Event *" + Environment.NewLine + context.Event.ToString("   ") + Environment.NewLine +
                context.ToStringDebug();
        }

        #endregion GetContextInfo

        /// <summary>
        /// Determines whether a shared variable exists that specifies that the plugin or the plugin and specifc message type should be prevented from executing.
        /// This is used in conjunction with PreventPluginHandlerExecution
        /// </summary>
        /// <returns></returns>
        public static bool HasPluginHandlerExecutionBeenPrevented(this IExtendedPluginContext context)
        {
            return context.HasPluginHandlerExecutionBeenPreventedInternal(context.Event, GetPreventPluginHandlerSharedVariableName(context.PluginTypeName));
        }

        /// <summary>
        /// Cast the Target to the given Entity Type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetTarget<T>(this IExtendedPluginContext context) where T : Entity
        {
            // Obtain the target business entity from the input parmameters.
            try
            {
                return ((IPluginExecutionContext)context).GetTarget<T>();
            }
            catch (Exception ex)
            {
                context.LogException(ex);
            }
            return null;
        }

        #endregion IExtendedPluginContext

        #region IPluginExecutionContext

        #region AssertEntityImageAttributesExist

        /// <summary>
        /// Checks the Pre/Post Entity Images to determine if the image collections contains an image with the given key, that contains the attributes.
        /// Throws an exception if the image name is contained in both the Pre and Post Image.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="imageName">Name of the image.</param>
        /// <param name="attributeNames">The attribute names.</param>
        public static void AssertEntityImageAttributesExist(this IPluginExecutionContext context, string imageName, params string[] attributeNames)
        {
            AssertEntityImageRegistered(context, imageName);
            Entity preImage;
            Entity postImage;
            var imageCollection = context.PreEntityImages.TryGetValue(imageName, out preImage) ? 
                InvalidPluginStepRegistrationException.ImageCollection.Pre : 
                InvalidPluginStepRegistrationException.ImageCollection.Post;
            context.PostEntityImages.TryGetValue(imageName, out postImage);

            var image = preImage ?? postImage;
            var missingAttributes = attributeNames.Where(attribute => !image.Contains(attribute)).ToList();

            if (missingAttributes.Any())
            {
                throw InvalidPluginStepRegistrationException.ImageMissingRequiredAttributes(imageCollection, imageName, missingAttributes);
            }
        }

        #endregion AssertEntityImageAttributesExist

        #region AssertEntityImageRegistered

        /// <summary>
        /// Checks the Pre/Post Entity Images to determine if the the collection contains an image with the given key.
        /// Throws an exception if the image name is contained in both the Pre and Post Image.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="imageName">Name of the image.</param>
        public static void AssertEntityImageRegistered(this IPluginExecutionContext context, string imageName)
        {
            var pre = context.PreEntityImages.ContainsKey(imageName);
            var post =  context.PostEntityImages.ContainsKey(imageName);

            if (pre && post)
            {
                throw new Exception($"Both Preimage and Post Image Contain the Image \"{imageName}\".  Unable to determine what entity collection to search for the given attributes.");
            }

            if (!pre && !post)
            {
                throw InvalidPluginStepRegistrationException.ImageMissing(imageName);
            }
        }

        #endregion AssertEntityImageRegistered

        #region CalledFrom

        /// <summary>
        /// Returns true if the current plugin maps to the Registered Event, or the current plugin has been triggered by the given registered event
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public static bool CalledFrom(this IPluginExecutionContext context, RegisteredEvent @event)
        {
            return context.GetContexts().Any(c => c.MessageName == @event.MessageName && c.PrimaryEntityName == @event.EntityLogicalName && c.Stage == (int)@event.Stage);
        }

        /// <summary>
        /// Returns true if the current plugin maps to the parameters given, or the current plugin has been triggered by the given parameters
        /// </summary>
        /// <returns></returns>
        public static bool CalledFrom(this IPluginExecutionContext context, string entityLogicalName = null, MessageType message = null, int? stage = null)
        {
            if (message == null && entityLogicalName == null && stage == null)
            {
                throw new Exception("At least one parameter for IPluginExecutionContext.CalledFrom must be populated");
            }
            return context.GetContexts().Any(c =>
                (message == null || c.MessageName == message.Name) &&
                (entityLogicalName == null || c.PrimaryEntityName == entityLogicalName) &&
                (stage == null || c.Stage == stage.Value));
        }

        #endregion CalledFrom

        #region CoallesceTarget

        /// <summary>
        /// Creates a new Entity of type T, adding the attributes from both the Target and the Post Image if they exist.
        /// If imageName is null, the first non-null image found is used.
        /// Does not return null.
        /// Does not return a reference to Target
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="imageName">Name of the image.</param>
        /// <returns></returns>
        public static T CoallesceTargetWithPreEntity<T>(this IPluginExecutionContext context, string imageName = null) where T : Entity
        {
            return DereferenceTarget<T>(context).CoallesceEntity(context.GetPreEntity<T>(imageName));
        }

        /// <summary>
        /// Creates a new Entity of type T, adding the attributes from both the Target and the Post Image if they exist.
        /// If imageName is null, the first non-null image found is used.
        /// Does not return null.
        /// Does not return a reference to Target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="imageName">Name of the image.</param>
        /// <returns></returns>
        public static T CoallesceTargetWithPostEntity<T>(this IPluginExecutionContext context, string imageName = null) where T : Entity
        {
            return DereferenceTarget<T>(context).CoallesceEntity(context.GetPostEntity<T>(imageName));
        }

        #endregion CoallesceTarget

        #region ContainsAllNonNull

        /// <summary>
        /// Checks to see if the PluginExecutionContext.InputParameters Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <returns></returns>
        public static bool InputContainsAllNonNull(this IPluginExecutionContext context, params string[] parameterNames)
        {
            return context.InputParameters.ContainsAllNonNull(parameterNames);
        }

        /// <summary>
        /// Checks to see if the PluginExecutionContext.OutputParameters Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <returns></returns>
        public static bool OutputContainsAllNonNull(this IPluginExecutionContext context, params string[] parameterNames)
        {
            return context.OutputParameters.ContainsAllNonNull(parameterNames);
        }

        /// <summary>
        /// Checks to see if the PluginExecutionContext.SharedVariables Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <returns></returns>
        public static bool SharedContainsAllNonNull(this IPluginExecutionContext context, params string[] parameterNames)
        {
            return context.SharedVariables.ContainsAllNonNull(parameterNames);
        }

        #endregion ContainsAllNonNull

        #region GetContexts

        /// <summary>
        /// The current PluginExecutionContext and the parent context hierarchy of the plugin.
        /// </summary>
        public static IEnumerable<IPluginExecutionContext> GetContexts(this IPluginExecutionContext context)
        {
            yield return context;
            foreach (var parent in context.GetParentContexts())
            {
                yield return parent;
            }
        }

        /// <summary>
        /// Iterates through all parent contexts.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static IEnumerable<IPluginExecutionContext> GetParentContexts(this IPluginExecutionContext context)
        {
            var parent = context.ParentContext;
            while (parent != null)
            {
                yield return parent;
                parent = parent.ParentContext;
            }
        }

        #endregion GetContexts

        /// <summary>
        /// Gets the event by iterating over all of the expected registered events to ensure that the plugin has been invoked by an expected event.
        /// For any given plug-in event at an instance in time, we would expect at most 1 result to match.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="events">The events.</param>
        /// <returns></returns>
        public static RegisteredEvent GetEvent(this IPluginExecutionContext context, IEnumerable<RegisteredEvent> events)
        {
            return events.FirstOrDefault(a =>
                (int)a.Stage == context.Stage 
                && a.MessageName == context.MessageName
                && (string.IsNullOrWhiteSpace(a.EntityLogicalName) || a.EntityLogicalName == context.PrimaryEntityName)
                ) 
                ?? events.FirstOrDefault(a =>
                (int)a.Stage == context.Stage 
                && a.Message == RegisteredEvent.Any 
                && (string.IsNullOrWhiteSpace(a.EntityLogicalName) || a.EntityLogicalName == context.PrimaryEntityName)
                );
        }

        #region GetFirstSharedVariable

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables or anywhere in the Plugin Context Hierarchy collection, cast to type 'T', or default(T) if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the variable to be returned</typeparam>
        /// <param name="context"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static T GetFirstSharedVariable<T>(this IPluginExecutionContext context, string variableName)
        {
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
        /// <param name="context"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static object GetFirstSharedVariable(this IPluginExecutionContext context, string variableName)
        {
            while (context != null)
            {
                if (context.SharedVariables.ContainsKey(variableName))
                {
                    return context.SharedVariables.GetParameterValue(variableName);
                }
                context = context.ParentContext;
            }
            return null;
        }

        #endregion GetFirstSharedVariable

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static MessageType GetMessageType(this IPluginExecutionContext context) { return new MessageType(context.MessageName); }

        #region GetParameterValue

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.InputParameters collection, cast to type 'T', or default(T) if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the parameter to be returned</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public static T GetInputParameterValue<T>(this IPluginExecutionContext context, string parameterName)
        {
            return context.InputParameters.GetParameterValue<T>(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the PluginExecutionContext.InputParameters collection, or null if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public static object GetInputParameterValue(this IPluginExecutionContext context, string parameterName)
        {
            return context.InputParameters.GetParameterValue(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the OutputParameters collection, cast to type 'T', or default(T) if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the parameter to be returned</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public static T GetOutputParameterValue<T>(this IPluginExecutionContext context, string parameterName)
        {
            return context.OutputParameters.GetParameterValue<T>(parameterName);
        }

        /// <summary>
        /// Gets the parameter value from the OutputParameters collection, or null if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public static object GetOutputParameterValue(this IPluginExecutionContext context, string parameterName)
        {
            return context.OutputParameters.GetParameterValue(parameterName);
        }

        /// <summary>
        /// Populates a local version of the request using the parameters from the context.  This exposes (most of) the parameters of that particular request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static T GetRequestParameters<T>(this IPluginExecutionContext context) where T : OrganizationRequest
        {
            var request = Activator.CreateInstance<T>();
            request.Parameters = context.InputParameters;
            return request;
        }

        /// <summary>
        /// Populates a local version of the response using the parameters from the context.  This exposes (most of) the parameters of that particular response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static T GetResponseParameters<T>(this IPluginExecutionContext context) where T : OrganizationResponse
        {
            var response = Activator.CreateInstance<T>();
            response.Results = context.OutputParameters;
            return response;
        }

        /// <summary>
        /// Gets the variable value from the SharedVariables collection, cast to type 'T', or default(T) if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the variable to be returned</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public static T GetSharedVariable<T>(this IPluginExecutionContext context, string variableName)
        {
            return context.SharedVariables.GetParameterValue<T>(variableName);
        }

        /// <summary>
        /// Gets the variable value from the PluginExecutionContext.SharedVariables collection, or null if the collection doesn't contain a variable with the given name.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public static object GetSharedVariable(this IPluginExecutionContext context, string variableName)
        {
            return context.SharedVariables.GetParameterValue(variableName);
        }

        #endregion GetParameterValue

        /// <summary>
        /// Gets the pipeline stage.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static PipelineStage GetPipelineStage(this IPluginExecutionContext context) { return (PipelineStage)context.Stage; }

        #region Get(Pre/Post)Entities

        /// <summary>
        /// If the imageName is populated and the PreEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// If the imageName is not populated, than the first image in PreEntityImages with a value, is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public static T GetPreEntity<T>(this IPluginExecutionContext context, string imageName = null) where T : Entity
        {
            return GetEntity<T>(context.PreEntityImages, imageName, DLaBExtendedPluginContextBase.PluginImageNames.PreImage);
        }

        /// <summary>
        /// If the imageName is populated and the PostEntityImages contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// If the imageName is not populated, than the first image in PostEntityImages with a value, is cast to the Entity type T, else null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public static T GetPostEntity<T>(this IPluginExecutionContext context, string imageName) where T : Entity
        {
            return GetEntity<T>(context.PostEntityImages, imageName, DLaBExtendedPluginContextBase.PluginImageNames.PostImage);
        }

        /// <summary>
        /// If the imageName is populated, then if images collection contains the given imageName Key, the Value is cast to the Entity type T, else null is returned
        /// If the imageName is not populated but the default name is, then the defaultname is searched for in the images collection and if it has a value, it is cast to the Entity type T.
        /// Else, the first non-null value in the images collection is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="images"></param>
        /// <param name="imageName"></param>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        private static T GetEntity<T>(DataCollection<string, Entity> images, string imageName, string defaultName) where T: Entity
        {
            if (images.Count == 0)
            {
                return null;
            }

            if (images.Count == 1 && imageName == null)
            {
                return images.Values.FirstOrDefault().AsEntity<T>();
            }

            Entity entity;

            if (imageName != null)
            {
                return images.TryGetValue(imageName, out entity) ? entity.AsEntity<T>() : null;
            }

            return defaultName != null && images.TryGetValue(defaultName, out entity)
                ? entity.AsEntity<T>()
                : images.Values.FirstOrDefault(v => v != null).AsEntity<T>();
        }

        #endregion Get(Pre/Post)Entities

        #region GetTarget

        /// <summary>
        /// Dereferences the target so an update to it will not cause an update to the actual target and result in a crm update post plugin execution
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static T DereferenceTarget<T>(IPluginExecutionContext context) where T : Entity
        {
            var entity = Activator.CreateInstance<T>();
            var target = context.GetTarget<T>();
            if (target != null)
            {
                entity.Id = target.Id;
                entity.LogicalName = target.LogicalName;
                foreach (var attribute in target.Attributes)
                {
                    entity[attribute.Key] = attribute.Value;
                }
            }
            return entity;
        }

        /// <summary>
        /// Cast the Target to the given Entity Type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static T GetTarget<T>(this IPluginExecutionContext context) where T : Entity
        {
            var parameters = context.InputParameters;
            if (!parameters.ContainsKey(ParameterName.Target) || !(parameters[ParameterName.Target] is Entity))
            {
                return null;
            }

            // Obtain the target business entity from the input parmameters.

            return ((Entity)parameters[ParameterName.Target]).AsEntity<T>();
        }

        /// <summary>
        /// Finds and returns the Target as an Entity Reference (Delete Plugins)
        /// </summary>
        /// <returns></returns>
        public static EntityReference GetTargetEntityReference(this IPluginExecutionContext context)
        {
            EntityReference entity = null;
            var parameters = context.InputParameters;
            if (parameters.ContainsKey(ParameterName.Target) &&
                 parameters[ParameterName.Target] is EntityReference)
            {
                entity = (EntityReference)parameters[ParameterName.Target];
            }
            return entity;
        }

        #endregion GetTarget

        #region Prevent Plugin Execution

        #region PreventPluginHandlerExecution

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="messageType">Type of the message.</param>
        public static void PreventPluginHandlerExecution(this IPluginExecutionContext context, string handlerTypeFullName, MessageType messageType)
        {
            context.PreventPluginHandlerExecution(handlerTypeFullName, messageType.Name);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="event">Type of the event.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void PreventPluginHandlerExecution(this IPluginExecutionContext context, string handlerTypeFullName, RegisteredEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            context.PreventPluginHandlerExecution(handlerTypeFullName, @event.MessageName, @event.EntityLogicalName, @event.Stage);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="handlerTypeFullName">The Full Type Name of the Plugin to Prevent</param>
        /// <param name="messageName">Name of the message.</param>
        /// <param name="logicalName">Name of the logical.</param>
        /// <param name="stage">The stage.</param>
        public static void PreventPluginHandlerExecution(this IPluginExecutionContext context, string handlerTypeFullName, string messageName = null, string logicalName = null, PipelineStage? stage = null)
        {
            var preventionName = GetPreventPluginHandlerSharedVariableName(handlerTypeFullName);
            object value;
            if (!context.SharedVariables.TryGetValue(preventionName, out value))
            {
                value = new Entity();
                context.SharedVariables.Add(preventionName, value);
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
        /// <param name="context">The context.</param>
        /// <param name="messageType">Type of the message.</param>
        public static void PreventPluginHandlerExecution<T>(this IPluginExecutionContext context, MessageType messageType)
            where T : IRegisteredEventsPluginHandler
        {
            context.PreventPluginHandlerExecution<T>(messageType.Name);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="event">Type of the event.</param>
        public static void PreventPluginHandlerExecution<T>(this IPluginExecutionContext context, RegisteredEvent @event)
            where T : IRegisteredEventsPluginHandler
        {
            context.PreventPluginHandlerExecution(typeof(T).FullName, @event);
        }

        /// <summary>
        /// Adds a shared Variable to the context that is checked by the GenericPluginHandlerBase to determine if it should be skipped  * NOTE * The Plugin has to finish executing for the Shared Variable to be passed to a new plugin
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        public static void PreventPluginHandlerExecution<T>(this IPluginExecutionContext context, string messageName = null, string logicalName = null, PipelineStage? stage = null)
            where T : IRegisteredEventsPluginHandler
        {
            context.PreventPluginHandlerExecution(typeof(T).FullName, messageName, logicalName, stage);
        }

        #endregion PreventPluginHandlerExecution<T>

        private static string GetPreventionRule(string messageName = null, string logicalName = null, PipelineStage? stage = null)
        {
            var rule = messageName == null ? string.Empty : "MessageName:" + messageName + "|";
            rule += logicalName == null ? string.Empty : "LogicalName:" + logicalName + "|";
            rule += stage == null ? string.Empty : "Stage:" + stage + "|";
            return rule;
        }

        private static string GetPreventPluginHandlerSharedVariableName(string pluginTypeName)
        {
            return pluginTypeName + "PreventExecution";
        }

        #region HasPluginHandlerExecutionBeenPrevented

        /// <summary>
        /// Determines whether a shared variable exists that specifies that the plugin or the plugin and specifc message type should be prevented from executing.
        /// This is used in conjunction with PreventPluginHandlerExecution
        /// </summary>
        /// <typeparam name="T">The type of the plugin.</typeparam>
        /// <returns></returns>
        public static bool HasPluginHandlerExecutionBeenPrevented<T>(this IPluginExecutionContext context, RegisteredEvent @event)
            where T : IRegisteredEventsPluginHandler
        {
            var preventionName = GetPreventPluginHandlerSharedVariableName(typeof(T).FullName);
            return context.HasPluginHandlerExecutionBeenPreventedInternal(@event, preventionName);
        }

        private static bool HasPluginHandlerExecutionBeenPreventedInternal(this IPluginExecutionContext context, RegisteredEvent @event, string preventionName)
        {
            var value = context.GetFirstSharedVariable(preventionName);
            if (value == null)
            {
                return false;
            }

            var hash = ((Entity)value).Attributes;
            return hash.Contains(string.Empty) ||
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

        #endregion IPluginExecutionContext
    }
}
