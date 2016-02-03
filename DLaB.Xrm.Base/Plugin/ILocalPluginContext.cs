using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// Plugin Context Interface for 
    /// </summary>
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

        /// <summary>
        /// Pulls the PrimaryEntityName, and PrimaryEntityId from the context and returns it as an Entity Reference
        /// </summary>
        /// <value>
        /// The primary entity.
        /// </value>
        EntityReference PrimaryEntity { get; }

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
}
