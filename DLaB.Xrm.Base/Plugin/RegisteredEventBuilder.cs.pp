using System;
using System.Collections.Generic;
using System.Linq;

#if DLAB_UNROOT_NAMESPACE
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
	
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
        protected List<string> EntityLogicalNames { get; set; }
        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        protected Action<IExtendedPluginContext> Execute { get; set; }

        /// <summary>
        /// Gets or sets the name of the Execute method for logging purposes.
        /// </summary>
        /// <value>
        /// The name of the execute method.
        /// </value>
        protected string ExecuteMethodName { get; set; }
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
                throw new ArgumentException("messageTypes must contain at least one value", nameof(messageTypes));
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
        public RegisteredEventBuilder ForEntities(params string[] logicalnames)
        {
            EntityLogicalNames.AddRange(logicalnames);
            return this;
        }

        /// <summary>
        /// Defines the custom Action to be performed rather than the standard ExecuteInternal.
        /// </summary>
        /// <param name="execute">Action that is invoked when the Plugin Executes.</param>
        /// <returns></returns>
        public RegisteredEventBuilder WithExecuteAction<T>(Action<T> execute) where T : IExtendedPluginContext
        {
            ExecuteMethodName = execute.Method.Name;
            Execute = context => execute((T)context);
            return this;
        }

        /// <summary>
        /// Defines the custom Action to be performed rather than the standard ExecuteInternal.
        /// </summary>
        /// <param name="execute">Action that is invoked when the Plugin Executes.</param>
        /// <returns></returns>
        public RegisteredEventBuilder WithExecuteAction(Action<IExtendedPluginContext> execute)
        {
            ExecuteMethodName = execute.Method.Name;
            Execute = execute;
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
                    events.AddRange(EntityLogicalNames.Select(logicalName => new RegisteredEvent(Stage, messageType, Execute, logicalName)
                                                                                 {
                                                                                     ExecuteMethodName = ExecuteMethodName
                                                                                 }));
                }
                else
                {
                    events.Add(new RegisteredEvent(Stage, messageType, Execute)
                                   {
                                       ExecuteMethodName = ExecuteMethodName
                                   });
                }
            }

            return events;
        } 
    }
}
