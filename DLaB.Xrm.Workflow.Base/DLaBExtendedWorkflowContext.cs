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
    public class DLaBExtendedWorkflowContext: IExtendedWorkflowContext
    {
        #region Properties

        public string CodeActivityTypeName { get; }

        public CodeActivityContext CodeActivityContext { get; }

        private IOrganizationService _organizationService;
        private IOrganizationService _systemOrganizationService;
        private IOrganizationService _triggeredUserOrganizationService;
        private IOrganizationServiceFactory _serviceFactory;
        private ITracingService _tracingService;
        private IWorkflowContext _workflowContext;

        /// <summary>
        /// The IOrganizationService of the workflow, Impersonated as the user that the plugin is was initiated by
        /// </summary>
        public virtual IOrganizationService InitiatingUserOrganizationService => _triggeredUserOrganizationService ?? (_triggeredUserOrganizationService = InitializeIOrganizationService(ServiceFactory, InitiatingUserId));

        /// <inheritdoc />
        /// <summary>
        /// The IOrganizationService of the workflow, Impersonated as the user that the plugin is registered to run as.
        /// </summary>
        public virtual IOrganizationService OrganizationService => _organizationService ?? (_organizationService = InitializeIOrganizationService(ServiceFactory, UserId));

        /// <summary>
        /// The IOrganizationService of the workflow, using the System User
        /// </summary>
        public virtual IOrganizationService SystemOrganizationService => _systemOrganizationService ?? (_systemOrganizationService = InitializeIOrganizationService(ServiceFactory, null));

        /// <summary>
        /// The IOrganizationServiceFactory for the workflow
        /// </summary>
        public virtual IOrganizationServiceFactory ServiceFactory => _serviceFactory ?? (_serviceFactory = InitializeServiceFactory(CodeActivityContext));

        /// <summary>
        /// The TracingService for the workflow
        /// </summary>
        public virtual ITracingService TracingService => _tracingService ?? (_tracingService = InitializeTracingService(CodeActivityContext));

        #region IWorkflowContext Implmentation

        /// <summary>
        /// The WorkflowContext for the workflow
        /// </summary>
        public IWorkflowContext WorkflowContext => _workflowContext ?? (_workflowContext = InitializeWorkflowContext(CodeActivityContext));

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

        public WorkflowCategory WorkflowCategoryEnum => (WorkflowCategory)WorkflowContext.WorkflowCategory;

        public WorkflowMode WorkflowModeEnum => (WorkflowMode)WorkflowContext.WorkflowMode;

        #endregion IWorkflowContext Implmentation

        private DLaBExtendedWorkflowContextSettings Settings { get; }

        #endregion Properties

        public DLaBExtendedWorkflowContext(CodeActivityContext executionContext, CodeActivity codeActivity, DLaBExtendedWorkflowContextSettings settings = null)
        {
            CodeActivityContext = executionContext;
            CodeActivityTypeName = codeActivity.GetType().FullName;

            Settings = settings ?? new DLaBExtendedWorkflowContextSettings();
        }

        #region Initializers

        protected virtual IWorkflowContext InitializeWorkflowContext(CodeActivityContext executionContext)
        {
            return executionContext.GetExtension<IWorkflowContext>();
        }

        protected virtual IOrganizationServiceFactory InitializeServiceFactory(CodeActivityContext executionContext)
        {
            return executionContext.GetExtension<IOrganizationServiceFactory>();
        }

        protected virtual ITracingService InitializeTracingService(CodeActivityContext executionContext)
        {
            return executionContext.GetExtension<ITracingService>();
        }

        protected virtual IOrganizationService InitializeIOrganizationService(IOrganizationServiceFactory factory, Guid? userId)
        {
            return new ExtendedOrganizationService(factory.CreateOrganizationService(userId), TracingService, Settings.OrganizationServiceSettings);
        }

        #endregion Initializers

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
            TracingService.Trace(message);
        }

        /// <summary>
        /// Traces the format.   Guaranteed to not throw an exception.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void TraceFormat(string format, params object[] args)
        {
            TracingService.Trace(format, args);
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
