using System;
using DLaB.Common;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// The Implementation of the ILocalPluginContext
    /// </summary>
    public class LocalPluginContextBase : ILocalPluginContext
    {
        #region Properties

        #region IPluginContext Properties

        protected IPluginExecutionContext PluginExecutionContext { get; set; }
        public virtual int Mode => PluginExecutionContext.Mode;
        int IExecutionContext.IsolationMode => PluginExecutionContext.IsolationMode;
        public virtual int Depth => PluginExecutionContext.Depth;
        public virtual string MessageName => PluginExecutionContext.MessageName;
        public virtual string PrimaryEntityName => PluginExecutionContext.PrimaryEntityName;
        public virtual Guid? RequestId => PluginExecutionContext.RequestId;
        public virtual string SecondaryEntityName => PluginExecutionContext.SecondaryEntityName;
        public virtual ParameterCollection InputParameters => PluginExecutionContext.InputParameters;
        public virtual ParameterCollection OutputParameters => PluginExecutionContext.OutputParameters;
        public virtual ParameterCollection SharedVariables => PluginExecutionContext.SharedVariables;
        public virtual Guid UserId => PluginExecutionContext.UserId;
        public virtual Guid InitiatingUserId => PluginExecutionContext.InitiatingUserId;
        public virtual Guid BusinessUnitId => PluginExecutionContext.BusinessUnitId;
        public virtual Guid OrganizationId => PluginExecutionContext.OrganizationId;
        public virtual string OrganizationName => PluginExecutionContext.OrganizationName;
        public virtual Guid PrimaryEntityId => PluginExecutionContext.PrimaryEntityId;
        public virtual EntityImageCollection PreEntityImages => PluginExecutionContext.PreEntityImages;
        public virtual EntityImageCollection PostEntityImages => PluginExecutionContext.PostEntityImages;
        public virtual EntityReference OwningExtension => PluginExecutionContext.OwningExtension;
        public virtual Guid CorrelationId => PluginExecutionContext.CorrelationId;
        public virtual bool IsExecutingOffline => PluginExecutionContext.IsExecutingOffline;
        public virtual bool IsOfflinePlayback => PluginExecutionContext.IsOfflinePlayback;
        public virtual bool IsInTransaction => PluginExecutionContext.IsInTransaction;
        public virtual Guid OperationId => PluginExecutionContext.OperationId;
        public virtual DateTime OperationCreatedOn => PluginExecutionContext.OperationCreatedOn;
        public virtual int Stage => PluginExecutionContext.Stage;
        public virtual IPluginExecutionContext ParentContext => PluginExecutionContext.ParentContext;

        #endregion // IPluginContext Properties

        #region ILocalPluginContext Properties

        /// <summary>
        /// Gets the isolation mode of the plugin assembly.
        /// </summary>
        /// <value>
        /// The isolation mode of the plugin assembly.
        /// </value>
        public IsolationMode IsolationMode { get; private set; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The current event the plugin is executing for.
        /// </summary>
        public virtual RegisteredEvent Event { get; private set; }

        private IOrganizationService _organizationService;
        private IOrganizationService _systemOrganizationService;
        private IOrganizationService _triggeredUserOrganizationService;
        private IOrganizationServiceFactory ServiceFactory { get; set; }

        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that the plugin is was initiated by
        /// </summary>
        public virtual IOrganizationService InitiatingUserOrganizationService => _triggeredUserOrganizationService ?? (_triggeredUserOrganizationService = ServiceFactory.CreateOrganizationService(InitiatingUserId));

        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that the plugin is registered to run as.
        /// </summary>
        public virtual IOrganizationService OrganizationService => _organizationService ?? (_organizationService = ServiceFactory.CreateOrganizationService(UserId));

        /// <summary>
        /// The IPluginExecutionContext of the plugin.
        /// </summary>
        //internal IPluginExecutionContext PluginExecutionContext { get; private set; }
        /// <summary>
        /// The Type.FullName of the plugin.
        /// </summary>
        public string PluginTypeName { get; private set; }

        /// <summary>
        /// Pulls the PrimaryEntityName, and PrimaryEntityId from the context and returns it as an Entity Reference
        /// </summary>
        /// <value>
        /// The primary entity.
        /// </value>
        public virtual EntityReference PrimaryEntity => new EntityReference(PrimaryEntityName, PrimaryEntityId);

        /// <summary>
        /// The IOrganizationService of the plugin, using the System User
        /// </summary>
        public virtual IOrganizationService SystemOrganizationService => _systemOrganizationService ?? (_systemOrganizationService = ServiceFactory.CreateOrganizationService(null));

        /// <summary>
        /// The ITracingService of the plugin.
        /// </summary>
        public ITracingService TracingService { get; private set; }

        #endregion ILocalPluginContext Properties

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
        /// Initializes a new instance of the <see cref="LocalPluginContextBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="plugin">The plugin.</param>
        /// <exception cref="System.ArgumentNullException">
        /// serviceProvider
        /// or
        /// plugin
        /// </exception>
        public LocalPluginContextBase(IServiceProvider serviceProvider, IRegisteredEventsPluginHandler plugin)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            InitializeServiceProviderProperties(serviceProvider);
            InitializePluginProperties(PluginExecutionContext, plugin);
        }

        #endregion Constructors

        #region PropertyInitializers

        /// <summary>
        /// Initializes the IServiceProvider properties.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        private void InitializeServiceProviderProperties(IServiceProvider serviceProvider)
        {
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            ServiceProvider = serviceProvider;
            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }

        /// <summary>
        /// Initializes the plugin properties.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="plugin">The plugin.</param>
        private void InitializePluginProperties(IPluginExecutionContext context, IRegisteredEventsPluginHandler plugin)
        {
            Event = context.GetEvent(plugin.RegisteredEvents);
            IsolationMode = (IsolationMode)context.IsolationMode;
            PluginTypeName = plugin.GetType().FullName;
        }

        #endregion PropertyInitializers

        #region Exception Logging

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public virtual void LogException(Exception ex)
        {
            TraceFormat("Exception: {0}", ex.ToStringWithCallStack());
            Trace(this.GetContextInfo());
        }

        #endregion Exception Logging

        #region Trace

        /// <summary>
        /// Traces the specified message.  Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void Trace(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null)
                {
                    return;
                }

                TracingService.Trace(message);
            }
            catch (Exception ex)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    TracingService.Trace("Exception occured attempting to trace {0}: {1}", message, ex);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // Attempted to trace a message, and had an exception, and then had another exception attempting to trace the exception that occured when tracing.
                    // Better to give up rather than stopping the entire program when attempting to write a Trace message
                }
            }
        }

        /// <summary>
        /// Traces the format.   Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void TraceFormat(string format, params object[] args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(format) || TracingService == null)
                {
                    return;
                }
                TracingService.Trace(format, args);
            }
            catch (Exception ex)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException
                    TracingService.Trace("Exception occured attempting to trace {0}: {1}", format, ex);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // Attempted to trace a message, and had an exception, and then had another exception attempting to trace the exception that occured when tracing.
                    // Better to give up rather than stopping the entire program when attempting to write a Trace message
                }
            }
        }

        #endregion Trace
    }
}