using System;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// Fluent Builder for Registered Events
    /// </summary>
    public class RegisteredEventBuilder
    {
        /// <summary>
        /// Gets or sets the entity logical names.
        /// </summary>
        /// <value>
        /// The entity logical names.
        /// </value>
        protected List<String> EntityLogicalNames { get; set; }
        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        protected Action<ILocalPluginContext> Execute { get; set; }
        /// <summary>
        /// Gets or sets the message types.
        /// </summary>
        /// <value>
        /// The message types.
        /// </value>
        protected List<MessageType> MessageTypes { get; set; }
        /// <summary>
        /// Gets or sets the stage.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        protected PipelineStage Stage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredEventBuilder"/> class.  With the only required attributes, what pipelines stage to execute, and for what message type(s)
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="messageTypes">The message types.</param>
        public RegisteredEventBuilder(PipelineStage stage, params MessageType[] messageTypes)
        {
            if (!messageTypes.Any())
            {
                throw new ArgumentException("messageTypes must contain at least one value", "messageTypes");
            }

            EntityLogicalNames = new List<string>();
            Execute = null;
            MessageTypes = new List<MessageType>(messageTypes);
            Stage = stage;
        }

        #region Fluent Methods

        /// <summary>
        /// Defines the Entities that will be created for the registered events
        /// </summary>
        /// <param name="logicalnames">The logicalnames.</param>
        /// <returns></returns>
        public RegisteredEventBuilder ForEntities(params String[] logicalnames)
        {
            EntityLogicalNames.AddRange(logicalnames);
            return this;
        }

        /// <summary>
        /// Defines the custom Action to be performed rather than the standard ExecuteInternal.
        /// </summary>
        /// <param name="execute">Action that is invoked when the Plugin Executes.</param>
        /// <returns></returns>
        public RegisteredEventBuilder WithExecuteAction<T>(Action<T> execute) where T : ILocalPluginContext
        {
            Execute = context => execute((T)context);
            return this;
        }

        #endregion Fluent Methods

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public List<RegisteredEvent> Build()
        {
            var events = new List<RegisteredEvent>();
            foreach (var messageType in MessageTypes)
            {
                if (EntityLogicalNames.Any())
                {
                    events.AddRange(EntityLogicalNames.Select(logicalName => new RegisteredEvent(Stage, messageType, Execute, logicalName)));
                }
                else
                {
                    events.Add(new RegisteredEvent(Stage, messageType, Execute));
                }
            }

            return events;
        } 
    }
}
