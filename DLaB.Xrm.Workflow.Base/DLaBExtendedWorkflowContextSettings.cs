using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    /// <summary>
    /// Default Initializer
    /// </summary>
    public class DLaBExtendedWorkflowContextSettings: IExtendedWorkflowContextInitializer
    {
        public ExtendedOrganizationServiceSettings OrganizationServiceSettings { get; set; }

        public DLaBExtendedWorkflowContextSettings()
        {
            OrganizationServiceSettings = new ExtendedOrganizationServiceSettings();
        }

        #region Initializers

        public virtual IWorkflowContext InitializeWorkflowContext(CodeActivityContext executionContext, ITracingService tracingService)
        {
            return executionContext.GetExtension<IWorkflowContext>();
        }

        public virtual IOrganizationServiceFactory InitializeServiceFactory(CodeActivityContext executionContext, ITracingService tracingService)
        {
            return executionContext.GetExtension<IOrganizationServiceFactory>();
        }

        public virtual ITracingService InitializeTracingService(CodeActivityContext executionContext)
        {
            return new ExtendedTracingService(executionContext.GetExtension<ITracingService>());
        }

        public virtual IOrganizationService InitializeIOrganizationService(IOrganizationServiceFactory factory, Guid? userId, ITracingService tracingService)
        {
            return new ExtendedOrganizationService(factory.CreateOrganizationService(userId), tracingService, OrganizationServiceSettings);
        }

        #endregion Initializers
    }
}
