using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else

namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Derived Version of the PluginExecutionContextBuilderBase
    /// </summary>
    public sealed class PluginExecutionContextBuilder : PluginExecutionContextBuilderBase<PluginExecutionContextBuilder>
    {
        /// <summary>
        /// Gets the Plugin Execution Context of the derived Class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override PluginExecutionContextBuilder This => this;
    }

    /// <summary>
    /// Abstract Builder to allow for Derived Types to created
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    public abstract class PluginExecutionContextBuilderBase<TDerived> where TDerived : PluginExecutionContextBuilderBase<TDerived>
    {
        /// <summary>
        /// Gets the derived version of the class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected abstract TDerived This { get; }
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected FakePluginExecutionContext Context { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginExecutionContextBuilderBase{TDerived}" /> class.
        /// </summary>
        protected PluginExecutionContextBuilderBase()
        {
            Context = new FakePluginExecutionContext();
        }

        #region Fluent Methods

        /// <summary>
        /// Sets the IsExecutingOffline value of the Context
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TDerived IsExecutingOffline(bool value)
        {
            Context.IsExecutingOffline = value;
            return This;
        }

        /// <summary>
        /// Sets the IsOfflinePlayback value of the Context
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TDerived IsOfflinePlayback(bool value)
        {
            Context.IsOfflinePlayback = value;
            return This;
        }

        /// <summary>
        /// Sets the IsInTransaction value of the Context
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TDerived IsInTransaction(bool value)
        {
            Context.IsInTransaction = value;
            return This;
        }

        /// <summary>
        /// Sets the Business Unit Id of the Context
        /// </summary>
        /// <param name="businessUnitId"></param>
        /// <returns></returns>
        public TDerived WithBusinessUnit(Guid businessUnitId)
        {
            Context.BusinessUnitId = businessUnitId;
            return This;
        }

        /// <summary>
        /// Sets the CorrelationId of the Context
        /// </summary>
        /// <param name="id">The correlationId.</param>
        /// <returns></returns>
        public TDerived WithCorrelationId(Guid id)
        {
            Context.CorrelationId = id;
            return This;
        }

        /// <summary>
        /// Using the WhoAmIRequest, populates the UserId and InitiatingUserId of the context with the current executing user.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TDerived WithCurrentUser(IOrganizationService service)
        {
            var info = service.GetCurrentlyExecutingUserInfo();           
            return WithUser(info.UserId).WithInitiatingUser(info.UserId);
        }

        /// <summary>
        /// Depth of the plugin context
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        public TDerived WithDepth(int depth)
        {
            Context.Depth = depth;
            return This;
        }

        /// <summary>
        /// Sets the registered event for the context to the first registered event of the plugin. Throws an exception if more than one event is found.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <param name="predicate">Optional predicate based on the RegisteredEvents of the plugin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Plugin  + plugin.GetType().FullName +  does not contain any registered events!  Unable to set the registered event of the context.</exception>
        public TDerived WithFirstRegisteredEvent(IRegisteredEventsPlugin plugin, Func<RegisteredEvent, bool>? predicate = null)
        {
            var first = predicate == null 
                ? plugin.RegisteredEvents.FirstOrDefault()
                : plugin.RegisteredEvents.FirstOrDefault(predicate);
            if (first == null)
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " does not contain any registered events!  Unable to set the registered event of the context.");
            }

            return WithRegisteredEvent(first);
        }

        /// <summary>
        /// Sets the registered event for the context to the first registered event of the plugin. Throws an exception if more than one event is found.
        /// </summary>
        /// <param name="plugin">The plugin.  Must contain a property with the name RegisteredEvents</param>
        /// <param name="predicate">Optional predicate based on the RegisteredEvents of the plugin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Plugin  + plugin.GetType().FullName +  does not contain any registered events!  Unable to set the registered event of the context.</exception>
        public TDerived WithFirstRegisteredEvent(IPlugin plugin, Func<RegisteredEvent, bool>? predicate = null)
        {
            RegisteredEvent? Map(dynamic? e)
            {
                if (e == null)
                {
                    return null;
                }
                return new RegisteredEvent(
                    (PipelineStage)(int)e.Stage,
                    new MessageType(e.MessageName),
                    (string)e.EntityLogicalName);
            }

            var prop = plugin.GetType().GetProperty("RegisteredEvents");
            if (prop is null)
            {
                throw new NullReferenceException("Property RegisteredEvents did not exist on type " + plugin.GetType().FullName);
            }
            var events = (IEnumerable<dynamic>?)prop.GetValue(plugin);
            var first = predicate == null
                ? Map(events?.FirstOrDefault(e => e != null))
                : events?.Select(Map).FirstOrDefault(e => e != null && predicate(e));
            if (first == null)
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " does not contain any registered events!  Unable to set the registered event of the context.");
            }

            return WithRegisteredEvent((int)first.Stage, first.MessageName, first.EntityLogicalName);
        }

        /// <summary>
        /// Sets the initiating user for the context.  The initiating User Id is the id of user that actually triggered the plugin, rather than the user the plugin is executing as
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public TDerived WithInitiatingUser(Guid id)
        {
            Context.InitiatingUserId = id;
            return This;
        }

        /// <summary>
        /// Adds the input parameter to the context's InputParameters collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public TDerived WithInputParameter(string name, object parameter)
        {
            Context.InputParameters[name] = parameter;
            return This;
        }

        /// <summary>
        /// Key Value Pairs of input parameters to add to the context
        /// </summary>
        /// <param name="nameValuePairs">The name value pairs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The list of arguments must be an even number!;nameValuePairs</exception>
        public TDerived WithInputParameters(params object[] nameValuePairs)
        {
            if (nameValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("The list of arguments must be an even number!", nameof(nameValuePairs));
            }
            for (var i = 0; i < nameValuePairs.Length; i += 2)
            {
                WithInputParameter((string)nameValuePairs[i], nameValuePairs[i + 1]);
            }
            return This;
        }

        /// <summary>
        /// Adds the parameters in the request to the Input Parameters
        /// </summary>
        /// <param name="request">An Org Request</param>
        /// <returns></returns>
        public TDerived WithInputRequest(OrganizationRequest request)
        {
            foreach(var param in request.Parameters)
            {
                WithInputParameter(param.Key, param.Value);
            }

            return This;
        }

        /// <summary>
        /// Sets the IsolationMode of the Context.  This does not actually prevent Sandbox calls from being made.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public TDerived WithIsolationMode(IsolationMode mode)
        {
            return WithIsolationMode((int) mode);
        }

        /// <summary>
        /// Sets the IsolationMode of the Context.  This does not actually prevent Sandbox calls from being made.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public TDerived WithIsolationMode(int mode)
        {
            Context.IsolationMode = mode;
            return This;
        }

        /// <summary>
        /// Async vs Sync
        /// </summary>
        /// <param name="mode">0 = Sync, 1 = Async</param>
        /// <returns></returns>
        public TDerived WithMode(int mode)
        {
            Context.Mode = mode;
            return This;
        }

        /// <summary>
        /// Sets the Operation of the context
        /// </summary>
        /// <param name="id">Operation Id</param>
        /// <param name="createdOn">Operation Created On</param>
        /// <returns></returns>
        public TDerived WithOperation(Guid id, DateTime createdOn)
        {
            Context.OperationId = id;
            Context.OperationCreatedOn = createdOn;
            return This;
        }

        /// <summary>
        /// Sets the Org of the context
        /// </summary>
        /// <param name="id">Organization Id</param>
        /// <param name="orgName">Organization Name</param>
        /// <returns></returns>
        public TDerived WithOrg(Guid id, string orgName)
        {
            Context.OrganizationId = id;
            Context.OrganizationName = orgName;
            return This;
        }

        /// <summary>
        /// Adds the input parameter to the context's InputParameters collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public TDerived WithOutputParameter(string name, object parameter)
        {
            Context.OutputParameters[name] = parameter;
            return This;
        }

        /// <summary>
        /// Key Value Pairs of input parameters to add to the context
        /// </summary>
        /// <param name="nameValuePairs">The name value pairs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The list of arguments must be an even number!;nameValuePairs</exception>
        public TDerived WithOutputParameters(params object[] nameValuePairs)
        {
            if (nameValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("The list of arguments must be an even number!", nameof(nameValuePairs));
            }
            for (var i = 0; i < nameValuePairs.Length; i += 2)
            {
                WithOutputParameter((string)nameValuePairs[i], nameValuePairs[i + 1]);
            }
            return This;
        }

        /// <summary>
        /// Sets the owning extension for the context.
        /// </summary>
        /// <param name="owningExtension">The owning extension.</param>
        /// <returns></returns>
        public TDerived WithOwningExt(EntityReference owningExtension)
        {
            Context.OwningExtension = owningExtension;
            return This;
        }

        /// <summary>
        /// Sets the parent context for the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public TDerived WithParentContext(IPluginExecutionContext context)
        {
            Context.ParentContext = context;
            return This;
        }

        /// <summary>
        /// Sets the primary entity identifier for the context.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public TDerived WithPrimaryEntityId(Guid guid)
        {
            Context.PrimaryEntityId = guid;
            return This;
        }

        /// <summary>
        /// Sets the primary entity name for the context.
        /// </summary>
        /// <param name="logicalName">Primary Entity Logical Name.</param>
        /// <returns></returns>
        public TDerived WithPrimaryEntityName(string logicalName)
        {
            Context.PrimaryEntityName = logicalName;
            return This;
        }

        /// <summary>
        /// Sets the registered event for the context.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public TDerived WithRegisteredEvent(RegisteredEvent @event)
        {
            return WithRegisteredEvent((int) @event.Stage, @event.MessageName, @event.EntityLogicalName);
        }

        /// <summary>
        /// Sets the registered event for the context.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public TDerived WithRegisteredEvent(int stage, string messageName, string? entityLogicalName = null)
        {
            Context.Stage = stage;
            Context.MessageName = messageName;
            Context.PrimaryEntityName = entityLogicalName ?? Context.PrimaryEntityName;
            return This;
        }

        /// <summary>
        /// Sets the registered event for the context to the registered event of the plugin.  Throws an exception if more than one event is found.  Use WithFirstRegisteredEvent if the first event was intended
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Plugin does not contain any registered events!  Unable to set the registered event of the context.
        /// or
        /// Plugin contains more than one registered event!  Unable to determine what registered event to use for the context.
        /// </exception>
        public TDerived WithRegisteredEvent(IRegisteredEventsPlugin plugin)
        {
            if (!plugin.RegisteredEvents.Any())
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " does not contain any registered events!  Unable to set the registered event of the context.");
            }
            if (plugin.RegisteredEvents.Skip(1).Any())
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " contains more than one registered event!  Unable to determine what registered event to use for the context.");
            }

            return WithRegisteredEvent(plugin.RegisteredEvents.Single());
        }

        /// <summary>
        /// Sets the registered event for the context to the registered event of the plugin.  Throws an exception if more than one event is found.  Use WithFirstRegisteredEvent if the first event was intended
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Plugin does not contain any registered events!  Unable to set the registered event of the context.
        /// or
        /// Plugin contains more than one registered event!  Unable to determine what registered event to use for the context.
        /// </exception>
        public TDerived WithRegisteredEvent(IPlugin plugin)
        {
            var dynPlugin = (dynamic) plugin;
            if (!((IEnumerable<dynamic>)dynPlugin.RegisteredEvents).Any())
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " does not contain any registered events!  Unable to set the registered event of the context.");
            }
            if (((IEnumerable<dynamic>)dynPlugin.RegisteredEvents).Skip(1).Any())
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " contains more than one registered event!  Unable to determine what registered event to use for the context.");
            }

            return WithFirstRegisteredEvent(plugin);
        }

        /// <summary>
        /// Sets the secondary entity name for the context.
        /// </summary>
        /// <param name="logicalName">Secondary Entity Logical Name.</param>
        /// <returns></returns>
        public TDerived WithSecondaryEntityName(string logicalName)
        {
            Context.SecondaryEntityName = logicalName;
            return This;
        }

        /// <summary>
        /// Adds the variable to the context's SharedVariables collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public TDerived WithSharedVariable(string name, object parameter)
        {
            Context.SharedVariables[name] = parameter;
            return This;
        }

        /// <summary>
        /// Key Value Pairs of variables to add to the context's SharedVariables collection.
        /// </summary>
        /// <param name="nameValuePairs">The name value pairs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The list of arguments must be an even number!;nameValuePairs</exception>
        public TDerived WithSharedVariables(params object[] nameValuePairs)
        {
            if (nameValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("The list of arguments must be an even number!", nameof(nameValuePairs));
            }
            for (var i = 0; i < nameValuePairs.Length; i += 2)
            {
                WithSharedVariable((string)nameValuePairs[i], nameValuePairs[i + 1]);
            }
            return This;
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public TDerived WithTarget<T>(T target) where T: Entity
        {
            Context.InputParameters[ParameterName.Target] = target;
            return WithTargetInternal(target.ToEntityReference());
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public TDerived WithTarget(EntityReference target)
        {
            Context.InputParameters[ParameterName.Target] = target;
            return WithTargetInternal(target);
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <param name="id">The target.</param>
        /// <returns></returns>
        public TDerived WithTarget(Id id)
        {
            Context.InputParameters[ParameterName.Target] = id.Entity;
            return WithTargetInternal(id.EntityReference);
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The target.</param>
        /// <returns></returns>
        public TDerived WithTarget<T>(Id<T> id) where T: Entity
        {
            Context.InputParameters[ParameterName.Target] = id.Entity;
            return WithTargetInternal(id.EntityReference);
        }

        /// <summary>
        /// Sets the target internal.
        /// </summary>
        /// <param name="entityRef">The entity reference.</param>
        /// <returns></returns>
        private TDerived WithTargetInternal(EntityReference entityRef)
        {
            if (entityRef != null)
            {
                if (Context.PrimaryEntityId == Guid.Empty)
                {
                    WithPrimaryEntityId(entityRef.Id);
                }
                if (string.IsNullOrWhiteSpace(Context.PrimaryEntityName))
                {
                    WithPrimaryEntityName(entityRef.LogicalName);
                }
            }
            return This;
        }

        /// <summary>
        /// Sets the user for the context.  The user is the user the plugin is currently executing as, rather than the user that actually triggered the plugin
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public TDerived WithUser(Guid id)
        {
            Context.UserId = id;
            return This;
        }

        /// <summary>
        /// Sets the pre-image using the default PreImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPreImage<T>(T image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PreImage) where T : Entity
        {
            Context.PreEntityImages[imageKey] = image;
            return This;
        }

        /// <summary>
        /// Sets the pre-image using the default PreImage key by default.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPreImage(Id image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PreImage)
        {
            Context.PreEntityImages[imageKey] = image.Entity;
            return This;
        }

        /// <summary>
        /// Sets the pre-image using the default PreImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPreImage<T>(Id<T> image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PreImage) where T : Entity
        {
            Context.PreEntityImages[imageKey] = image.Entity;
            return This;
        }

        /// <summary>
        /// Sets the Pre-operation event for the context.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName">Utilizes the PrimaryEntityName if not specified</param>
        /// <returns></returns>
        public TDerived WithPreOperation(string messageName, string? entityLogicalName = null)
        {
            return WithRegisteredEvent((int)PipelineStage.PreOperation, messageName, entityLogicalName);
        }

        /// <summary>
        /// Sets the Pre-validation event for the context.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName">Utilizes the PrimaryEntityName if not specified</param>
        /// <returns></returns>
        public TDerived WithPreValidation(string messageName, string? entityLogicalName = null)
        {
            return WithRegisteredEvent((int) PipelineStage.PreValidation, messageName, entityLogicalName);
        }

        /// <summary>
        /// Sets the Post-operation event for the context.
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName">Utilizes the PrimaryEntityName if not specified</param>
        /// <returns></returns>
        public TDerived WithPostOperation(string messageName, string? entityLogicalName = null)
        {
            return WithRegisteredEvent((int)PipelineStage.PostOperation, messageName, entityLogicalName);
        }

        /// <summary>
        /// Sets the post image using the default PostImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPostImage<T>(T image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PostImage) where T : Entity
        {
            Context.PostEntityImages[imageKey] = image;
            return This;
        }

        /// <summary>
        /// Sets the post image using the default PostImage key by default.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPostImage(Id image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PostImage)
        {
            Context.PostEntityImages[imageKey] = image.Entity;
            return This;
        }

        /// <summary>
        /// Sets the post image using the default PostImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public TDerived WithPostImage<T>(Id<T> image, string imageKey = DLaBExtendedPluginContextBase.PluginImageNames.PostImage) where T : Entity
        {
            Context.PostEntityImages[imageKey] = image;
            return This;
        }

        #endregion Fluent Methods

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public IPluginExecutionContext Build()
        {
            return Context.Clone();
        }
    }
}
