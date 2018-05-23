using System;
using System.Linq;
using Source.DLaB.Xrm.Plugin;

namespace Xyz.Xrm.Test.Builders
{
    public class PluginExecutionContextBuilder : DLaB.Xrm.Test.Builders.PluginExecutionContextBuilderBase<PluginExecutionContextBuilder>
    {
        protected override PluginExecutionContextBuilder This => this;


        #region Fluent Methods

        public PluginExecutionContextBuilder WithFirstRegisteredEvent(IRegisteredEventsPlugin plugin)
        {
            var first = plugin.RegisteredEvents.FirstOrDefault();
            if (first == null)
            {
                throw new Exception("Plugin " + plugin.GetType().FullName + " does not contain any registered events!  Unable to set the registered event of the context.");
            }

            return WithRegisteredEvent(first);
        }

        public PluginExecutionContextBuilder WithRegisteredEvent(RegisteredEvent @event)
        {
            return WithRegisteredEvent((int)@event.Stage, @event.MessageName, @event.EntityLogicalName);
        }

        public PluginExecutionContextBuilder WithRegisteredEvent(IRegisteredEventsPlugin plugin)
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
        /// Sets the IsoloationMode of the Context.  This does not actually prevent Sandbox calls from being made.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public PluginExecutionContextBuilder WithIsoloationMode(IsolationMode mode)
        {
            Context.IsolationMode = (int)mode;
            return this;
        }

        #endregion Fluent Methods
    }
}
