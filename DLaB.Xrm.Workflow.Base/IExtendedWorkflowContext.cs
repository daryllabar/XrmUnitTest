using System.Activities;
#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm;
#else
using Source.DLaB.Xrm;
#endif
using Microsoft.Xrm.Sdk.Workflow;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public interface IExtendedWorkflowContext : IWorkflowContext, IExtendedExecutionContext
    {
        string CodeActivityTypeName { get; }

        CodeActivityContext CodeActivityContext { get; }

        WorkflowCategory WorkflowCategoryEnum { get; }
        WorkflowMode WorkflowModeEnum { get; }
    }
}
