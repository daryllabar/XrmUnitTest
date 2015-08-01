using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Plugin
{
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
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, MessageType message, Action<ILocalPluginContext> execute)
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
        public static void AddEvent(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, MessageType message, Action<ILocalPluginContext> execute){
            events.Add(new RegisteredEvent(stage, message, execute, entityLogicalName));
        }

        #endregion // AddEvent

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
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, Action<ILocalPluginContext> execute, params MessageType[] messages)
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
        public static void AddEvents(this List<RegisteredEvent> events, PipelineStage stage, string entityLogicalName, Action<ILocalPluginContext> execute, params MessageType[] messages)
        {
            foreach (var message in messages)
            {
                events.Add(new RegisteredEvent(stage, message, execute, entityLogicalName));
            }
        }

        #endregion // AddEvent

        #endregion // List<RegisteredEvent>
    }
}
