using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
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

        #region IPluginExecutionContext

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
            
            return ((Entity)parameters[ParameterName.Target]).ToEntity<T>();
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

        #endregion IPluginExecutionContext
    }
}
