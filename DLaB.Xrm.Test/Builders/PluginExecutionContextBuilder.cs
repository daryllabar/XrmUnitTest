using System;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public class PluginExecutionContextBuilder
    {
        private FakePluginExecutionContext Context { get; set; }

        public PluginExecutionContextBuilder()
        {
            Context = new FakePluginExecutionContext();
        }

        #region Fluent Methods

        /// <summary>
        /// Sets the initiating user for the context.  The initiaiting User Id is the id of user that actually triggered the plugin, rather than the user the plugin is executing as
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithInitiatingUser(Guid id)
        {
            Context.InitiatingUserId = id;
            return this;
        }

        public PluginExecutionContextBuilder WithInputParameter(string name, object parameter)
        {
            Context.InputParameters[name] = parameter;
            return this;
        }

        public PluginExecutionContextBuilder WithInputParameters(params object[] nameValuePairs)
        {
            if (nameValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("The list of arguments must be an even number!", "nameValuePairs");
            }
            for (var i = 0; i < nameValuePairs.Length; i += 2)
            {
                WithInputParameter((string)nameValuePairs[i], nameValuePairs[i + 1]);
            }
            return this;
        }

        public PluginExecutionContextBuilder WithPrimaryEntityId(Guid guid)
        {
            Context.PrimaryEntityId = guid;
            return this;
        }

        /// <summary>
        /// Sets the registered event for the context.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithRegisteredEvent(RegisteredEvent @event)
        {
            Context.Stage = (int)@event.Stage;
            Context.MessageName = @event.MessageName;
            Context.PrimaryEntityName = @event.EntityLogicalName;
            return this;
        }

        /// <summary>
        /// Sets the registered event for the context.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="messageName"></param>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithRegisteredEvent(int stage, string messageName, string entityLogicalName)
        {
            Context.Stage = stage;
            Context.MessageName = messageName;
            Context.PrimaryEntityName = entityLogicalName;
            return this;
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithTarget<T>(T target)
        {
            Context.InputParameters[ParameterName.Target] = target;
            return this;
        }

        /// <summary>
        /// Sets the user for the context.  The user is the user the plugin is currently executing as, rather than the user that actually triggered the plugin
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithUser(Guid id)
        {
            Context.UserId = id;
            return this;
        }

        /// <summary>
        /// Sets the pre image using the default PreImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithPreImage<T>(T image, string imageKey = LocalPluginContextBase.PluginImageNames.PreImage) where T : Entity
        {
            Context.PreEntityImages[imageKey] = image;
            return this;
        }

        /// <summary>
        /// Sets the post image using the default PostImage key by default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image">The image.</param>
        /// <param name="imageKey">The image key.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithPostImage<T>(T image, string imageKey = LocalPluginContextBase.PluginImageNames.PostImage) where T : Entity
        {
            Context.PostEntityImages[imageKey] = image;
            return this;
        }

        #endregion // Fluent Methods

        public IPluginExecutionContext Build()
        {
            return Context;
        }
    }
}
