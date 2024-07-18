using System;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif

{
    /// <summary>
    /// Plugin Context Interface for Handling Additional functionality
    /// </summary>
    public interface IExtendedPluginContext : IPluginExecutionContext, IExtendedExecutionContext, IServiceProvider
    {
        #region Properties

        /// <summary>
        /// The current event the plugin is executing for.
        /// </summary>
        RegisteredEvent Event { get; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the isolation mode of the plugin assembly.
        /// </summary>
        /// <value>
        /// The isolation mode of the plugin assembly.
        /// </value>
        new IsolationMode IsolationMode { get; }

        /// <summary>
        /// The Type.FullName of the plugin.
        /// </summary>
        string PluginTypeName { get; }

        /// <summary>
        /// Pulls the PrimaryEntityName, and PrimaryEntityId from the context and returns it as an Entity Reference
        /// </summary>
        /// <value>
        /// The primary entity.
        /// </value>
        EntityReference PrimaryEntity { get; }

        #endregion Properties
    }

    /// <summary>
    /// Extensions for IExtendedPluginContext
    /// </summary>
    public static class ExtendedPluginContextExtensions
    {
        /// <summary>Gets the service object of the specified type.</summary>
        /// <typeparam name="TService">A type that specifies the type of service object to get</typeparam>
        /// <param name="context">The context</param>
        /// <returns>A service object specified type.
        /// -or-
        /// <see langword="null" /> if there is no service object of specified type.</returns>
        public static TService Get<TService>(this IExtendedPluginContext context) where TService : class
        {
            return context.ServiceProvider.GetService<TService>();
        }
    }
}
