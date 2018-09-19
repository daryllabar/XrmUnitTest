using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Common;
#else
using Source.DLaB.Common;
#endif
#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Plugin;
#else
using Source.DLaB.Xrm.Plugin;
#endif

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public class DLaBExtendedWorkflowContext : IExtendedWorkflowContext
    {
        #region Properties

        public string CodeActivityTypeName { get; }

        public CodeActivityContext CodeActivityContext { get; }

        #region IExtendedExecutionContext Implementation

        private IOrganizationService _organizationService;
        private IOrganizationService _systemOrganizationService;
        private IOrganizationService _triggeredUserOrganizationService;
        private IOrganizationServiceFactory _serviceFactory;
        private ITracingService _tracingService;
        private IWorkflowContext _workflowContext;

        /// <summary>
        /// The IOrganizationService of the workflow, Impersonated as the user that the plugin is was initiated by
        /// </summary>
        public IOrganizationService InitiatingUserOrganizationService => _triggeredUserOrganizationService ?? (_triggeredUserOrganizationService = Settings.InitializeIOrganizationService(ServiceFactory, InitiatingUserId, TracingService));

        /// <inheritdoc />
        /// <summary>
        /// The IOrganizationService of the workflow, Impersonated as the user that the plugin is registered to run as.
        /// </summary>
        public IOrganizationService OrganizationService => _organizationService ?? (_organizationService = Settings.InitializeIOrganizationService(ServiceFactory, UserId, TracingService));

        /// <summary>
        /// The IOrganizationService of the workflow, using the System User
        /// </summary>
        public IOrganizationService SystemOrganizationService => _systemOrganizationService ?? (_systemOrganizationService = Settings.InitializeIOrganizationService(ServiceFactory, null, TracingService));

        /// <summary>
        /// The IOrganizationServiceFactory for the workflow
        /// </summary>
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory ?? (_serviceFactory = Settings.InitializeServiceFactory(CodeActivityContext, TracingService));

        /// <summary>
        /// The TracingService for the workflow
        /// </summary>
        public ITracingService TracingService => _tracingService ?? (_tracingService = Settings.InitializeTracingService(CodeActivityContext));

        #endregion IExtendedExecutionContext Implementation

        #region IWorkflowContext Implmentation

        /// <summary>
        /// The WorkflowContext for the workflow
        /// </summary>
        public IWorkflowContext WorkflowContext => _workflowContext ?? (_workflowContext = Settings.InitializeWorkflowContext(CodeActivityContext, TracingService));

        public int Mode => WorkflowContext.Mode;

        public int IsolationMode => WorkflowContext.IsolationMode;

        public int Depth => WorkflowContext.Depth;

        public string MessageName => WorkflowContext.MessageName;

        public string PrimaryEntityName => WorkflowContext.PrimaryEntityName;

        public Guid? RequestId => WorkflowContext.RequestId;

        public string SecondaryEntityName => WorkflowContext.SecondaryEntityName;

        public ParameterCollection InputParameters => WorkflowContext.InputParameters;

        public ParameterCollection OutputParameters => WorkflowContext.OutputParameters;

        public ParameterCollection SharedVariables => WorkflowContext.SharedVariables;

        public Guid UserId => WorkflowContext.UserId;

        public Guid InitiatingUserId => WorkflowContext.InitiatingUserId;

        public Guid BusinessUnitId => WorkflowContext.BusinessUnitId;

        public Guid OrganizationId => WorkflowContext.OrganizationId;

        public string OrganizationName => WorkflowContext.OrganizationName;

        public Guid PrimaryEntityId => WorkflowContext.PrimaryEntityId;

        public EntityImageCollection PreEntityImages => WorkflowContext.PreEntityImages;

        public EntityImageCollection PostEntityImages => WorkflowContext.PostEntityImages;

        public EntityReference OwningExtension => WorkflowContext.OwningExtension;

        public Guid CorrelationId => WorkflowContext.CorrelationId;

        public bool IsExecutingOffline => WorkflowContext.IsExecutingOffline;

        public bool IsOfflinePlayback => WorkflowContext.IsOfflinePlayback;

        public bool IsInTransaction => WorkflowContext.IsInTransaction;

        public Guid OperationId => WorkflowContext.OperationId;

        public DateTime OperationCreatedOn => WorkflowContext.OperationCreatedOn;

        public string StageName => WorkflowContext.StageName;

        public int WorkflowCategory => WorkflowContext.WorkflowCategory;

        public int WorkflowMode => WorkflowContext.WorkflowMode;

        public IWorkflowContext ParentContext => WorkflowContext.ParentContext;

        public WorkflowCategory WorkflowCategoryEnum => (WorkflowCategory) WorkflowContext.WorkflowCategory;

        public WorkflowMode WorkflowModeEnum => (WorkflowMode) WorkflowContext.WorkflowMode;

        #endregion IWorkflowContext Implmentation

        #region CodeActivityContext Accessors

        /// <summary>Gets the unique identifier of the currently executing activity instance.</summary>
        /// <returns>The unique identifier of the currently executing activity instance.</returns>
        public string ActivityInstanceId => CodeActivityContext.ActivityInstanceId;

        /// <summary>Gets the unique indentifier of the currently executing workflow instance.</summary>
        /// <returns>The unique identifier of the currently executing workflow instance.</returns>
        public Guid WorkflowInstanceId => CodeActivityContext.WorkflowInstanceId;

        /// <summary>Gets the data context of the currently executing activity.</summary>
        /// <returns>The workflow data context of the currently executing activity.</returns>
        public WorkflowDataContext DataContext => CodeActivityContext.DataContext;

        #endregion CodeActivityContext Accessors

        private IExtendedWorkflowContextInitializer Settings { get; }

        #endregion Properties
        
        #region ImageNames struct

        /// <summary>
        /// Struct for the Standard Plugin Image Names
        /// </summary>
        public struct WorkflowImageNames
        {
            /// <summary>
            /// The default pre image
            /// </summary>
            public const string PreImage = "PreBusinessEntity";
            /// <summary>
            /// The default post image
            /// </summary>
            public const string PostImage = "PostBusinessEntity";
        }

        #endregion ImageNames struct

        #region Constructors

        public DLaBExtendedWorkflowContext(CodeActivityContext executionContext, CodeActivity codeActivity, IExtendedWorkflowContextInitializer settings = null)
        {
            CodeActivityContext = executionContext;
            CodeActivityTypeName = codeActivity.GetType().FullName;

            Settings = settings ?? new DLaBExtendedWorkflowContextSettings();
        }

        #endregion Constructors

        #region IExtendedExecutionContext Implementation

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public virtual void LogException(Exception ex)
        {
            TracingService.Trace("Exception: {0}", ex.ToStringWithCallStack());
            TracingService.Trace(this.GetContextInfo());
        }

        public void TraceContext()
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

        #endregion IExtendedExecutionContext Implementation

        #region ITracingService Implementation

        /// <summary>
        /// Traces the specified message.  By default, is guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Optional Args</param>
        public void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        #endregion ITracingService Implementation

        /// <summary>
        /// Allows for SomeInOrOutParameter.Get&lt;string&gt;(context)
        /// </summary>
        /// <param name="context"></param>
        public static implicit operator CodeActivityContext(DLaBExtendedWorkflowContext context)
        {
            return context.CodeActivityContext;
        }
    }
}
