using System;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public interface IExtendedWorkflowContextInitializer
    {
        /// <summary>
        /// Initializes the WorkflowContext
        /// </summary>
        /// <param name="executionContext">The Context</param>
        /// <param name="tracingService">The Tracing Service</param>
        /// <returns></returns>
        IWorkflowContext InitializeWorkflowContext(CodeActivityContext executionContext, ITracingService tracingService);

        /// <summary>
        /// Initializes the OrganizationServiceFactory
        /// </summary>
        /// <param name="executionContext">The Context</param>
        /// <param name="tracingService">The Tracing Service</param>
        /// <returns></returns>
        IOrganizationServiceFactory InitializeServiceFactory(CodeActivityContext executionContext, ITracingService tracingService);

        /// <summary>
        /// Initializes the Tracing Service
        /// </summary>
        /// <param name="executionContext">The Context</param>
        /// <returns></returns>
        ITracingService InitializeTracingService(CodeActivityContext executionContext);

        /// <summary>
        /// Initializes the Organization Service
        /// </summary>
        /// <param name="factory">The Factory</param>
        /// <param name="userId">The User Id</param>
        /// <param name="tracingService">The Tracing Service</param>
        /// <returns></returns>
        IOrganizationService InitializeIOrganizationService(IOrganizationServiceFactory factory, Guid? userId, ITracingService tracingService);
    }
}
