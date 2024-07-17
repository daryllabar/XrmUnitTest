using System;
using System.Linq;
#if !DLAB_XRM_DEBUG
using System.Diagnostics;

#endif
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Common;
#else
using Source.DLaB.Common;
#endif

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// The Implementation of the IExtendedPluginContext
    /// </summary>
    // ReSharper disable once InconsistentNaming
#if !DLAB_XRM_DEBUG
    [DebuggerNonUserCode]
#endif
    public class DLaBExtendedPluginContextBase : IExtendedPluginContext
    {
        #region Properties

        private readonly OrganizationServicesWrapper _organizationServices;

        #region IPluginContext Properties

        /// <summary>
        /// Gets or sets the plugin execution context.
        /// </summary>
        /// <value>
        /// The plugin execution context.
        /// </value>
        protected IPluginExecutionContext PluginExecutionContext { get; }
        /// <inheritdoc />
        public virtual int Mode => PluginExecutionContext.Mode;
        /// <inheritdoc />
        int IExecutionContext.IsolationMode => PluginExecutionContext.IsolationMode;
        /// <inheritdoc />
        public virtual int Depth => PluginExecutionContext.Depth;
        /// <inheritdoc />
        public string MessageName => PluginExecutionContext.MessageName;
        /// <inheritdoc />
        public string PrimaryEntityName => PluginExecutionContext.PrimaryEntityName;
        /// <inheritdoc />
        public virtual Guid? RequestId => PluginExecutionContext.RequestId;
        /// <inheritdoc />
        public virtual string SecondaryEntityName => PluginExecutionContext.SecondaryEntityName;
        /// <inheritdoc />
        public virtual ParameterCollection InputParameters => PluginExecutionContext.InputParameters;
        /// <inheritdoc />
        public virtual ParameterCollection OutputParameters => PluginExecutionContext.OutputParameters;
        /// <inheritdoc />
        public virtual ParameterCollection SharedVariables => PluginExecutionContext.SharedVariables;
        /// <inheritdoc />
        public virtual Guid UserId => PluginExecutionContext.UserId;
        /// <inheritdoc />
        public virtual Guid InitiatingUserId => PluginExecutionContext.InitiatingUserId;
        /// <inheritdoc />
        public virtual Guid BusinessUnitId => PluginExecutionContext.BusinessUnitId;
        /// <inheritdoc />
        public virtual Guid OrganizationId => PluginExecutionContext.OrganizationId;
        /// <inheritdoc />
        public virtual string OrganizationName => PluginExecutionContext.OrganizationName;
        /// <inheritdoc />
        public virtual Guid PrimaryEntityId => PluginExecutionContext.PrimaryEntityId;
        /// <inheritdoc />
        public virtual EntityImageCollection PreEntityImages => PluginExecutionContext.PreEntityImages;
        /// <inheritdoc />
        public virtual EntityImageCollection PostEntityImages => PluginExecutionContext.PostEntityImages;
        /// <inheritdoc />
        public virtual EntityReference OwningExtension => PluginExecutionContext.OwningExtension;
        /// <inheritdoc />
        public virtual Guid CorrelationId => PluginExecutionContext.CorrelationId;
        /// <inheritdoc />
        public virtual bool IsExecutingOffline => PluginExecutionContext.IsExecutingOffline;
        /// <inheritdoc />
        public virtual bool IsOfflinePlayback => PluginExecutionContext.IsOfflinePlayback;
        /// <inheritdoc />
        public virtual bool IsInTransaction => PluginExecutionContext.IsInTransaction;
        /// <inheritdoc />
        public virtual Guid OperationId => PluginExecutionContext.OperationId;
        /// <inheritdoc />
        public virtual DateTime OperationCreatedOn => PluginExecutionContext.OperationCreatedOn;
        /// <inheritdoc />
        public virtual int Stage => PluginExecutionContext.Stage;
        /// <inheritdoc />
        public virtual IPluginExecutionContext ParentContext => PluginExecutionContext.ParentContext;

        #endregion // IPluginContext Properties

        #region IExtendedPluginContext Properties

        /// <inheritdoc />
        public IsolationMode IsolationMode => (IsolationMode) PluginExecutionContext.IsolationMode;
        /// <inheritdoc />
        public bool IsAsync => Mode == RegisteredEvent.ContextMode.Async;
        /// <inheritdoc />
        public bool IsSync => Mode == RegisteredEvent.ContextMode.Sync;
        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }
        /// <inheritdoc />
        public RegisteredEvent Event { get; private set; }
        /// <inheritdoc />
        public IOrganizationService InitiatingUserOrganizationService => _organizationServices.InitiatingUser.Value;
        /// <inheritdoc />
        public IOrganizationService OrganizationService => _organizationServices.Organization.Value;
        /// <inheritdoc />
        public string PluginTypeName { get; private set; }
        /// <inheritdoc />
        public virtual EntityReference PrimaryEntity => new EntityReference(PrimaryEntityName, PrimaryEntityId);
        /// <inheritdoc />
        public IOrganizationServiceFactory ServiceFactory => ServiceProvider.Get<IOrganizationServiceFactory>();
        /// <inheritdoc />
        public IOrganizationService CachedOrganizationService => _organizationServices.Cached.Value;
        /// <inheritdoc />
        public IOrganizationService SystemOrganizationService => _organizationServices.System.Value;
        /// <inheritdoc />
        public ITracingService TracingService => ServiceProvider.Get<ITracingService>();

        #endregion IExtendedPluginContext Properties

        #endregion Properties

        #region ImageNames struct

        /// <summary>
        /// Struct for the Standard Plugin Image Names
        /// </summary>
        public struct PluginImageNames
        {
            /// <summary>
            /// The default pre image
            /// </summary>
            public const string PreImage = "PreImage";
            /// <summary>
            /// The default post image
            /// </summary>
            public const string PostImage = "PostImage";
        }

        #endregion ImageNames struct

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DLaBExtendedPluginContextBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="plugin">The plugin.</param>
        public DLaBExtendedPluginContextBase(IServiceProvider serviceProvider, IRegisteredEventsPlugin plugin) : this(serviceProvider)
        {
            InitializePluginProperties(plugin);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DLaBExtendedPluginContextBase"/> class.  Useful for testing or when not using an IRegisteredEventsPlugin.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="pluginTypeName"></param>
        /// <param name="event"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DLaBExtendedPluginContextBase(IServiceProvider serviceProvider, string pluginTypeName, RegisteredEvent @event = null) : this(serviceProvider)
        {
            InitializePluginProperties(null, pluginTypeName, @event);
        }

        private DLaBExtendedPluginContextBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            PluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();
            _organizationServices = serviceProvider.Get<OrganizationServicesWrapper>();
        }

        #endregion Constructors

        #region PropertyInitializers

        /// <summary>
        /// Initializes the plugin properties.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <param name="pluginTypeName">The name of the Plugin Type</param>
        /// <param name="event">The registered Event for this context</param>
        private void InitializePluginProperties(IRegisteredEventsPlugin plugin, string pluginTypeName = null, RegisteredEvent @event = null)
        {
            if (plugin == null)
            {
                Event = @event;
                PluginTypeName = pluginTypeName;
            }
            else
            {
                Event = PluginExecutionContext.GetEvent(plugin.RegisteredEvents);
                PluginTypeName = plugin.GetType().FullName;
            }

            if (Event == null)
            {
                var message = $"No RegisteredEvent found for the current context of Stage: {this.GetPipelineStage()}, Message: {MessageName}, Entity: {PrimaryEntityName}.  Either Unregister the plugin for this event, or include this as a RegisteredEvent in the Plugin's RegisteredEvents.";
                try
                {
                    TracingService.Trace(message);
                    TracingService.Trace(this.GetContextInfo());
                }
                finally
                {
                    throw new InvalidPluginExecutionException(message);
                }
            }

            if (Event.Message == RegisteredEvent.Any)
            {
                Event = new RegisteredEvent(Event.Stage, PluginExecutionContext.GetMessageType(), Event.Execute);
            }
        }

        #endregion PropertyInitializers

        #region Exception Logging

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public virtual void LogException(Exception ex)
        {
            while (ex is AggregateException aggEx && aggEx.InnerExceptions.Count == 1)
            {
                ex = aggEx.InnerExceptions.First();
            }
            TracingService.Trace("Exception: {0}", ex.ToStringWithCallStack());
            TracingService.Trace(this.GetContextInfo());
        }

        #endregion Exception Logging

        #region Trace

        /// <summary>
        /// Traces the specified message.  By default, is guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional Args</param>
        public void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        /// <summary>
        /// Traces the entire context.
        /// </summary>
        public virtual void TraceContext()
        {
            Trace(this.ToStringDebug());
        }

        /// <summary>
        /// Traces the time from call to dispose.  Designed to be used in a using statement
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public IDisposable TraceTime(string format, params object[] args)
        {
            return new TraceTimer(TracingService, string.Format(format, args));
        }

        #endregion Trace

        #region ServiceProvider

        /// <summary>Gets the service object of the specified type.</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.
        /// -or-
        /// <see langword="null" /> if there is no service object of type <paramref name="serviceType" />.</returns>
        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        #endregion ServiceProvider
    }
}
