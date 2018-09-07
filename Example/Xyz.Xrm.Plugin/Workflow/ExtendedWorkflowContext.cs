using System.Activities;
using Source.DLaB.Xrm.Workflow;

namespace Xyz.Xrm.Plugin.Workflow
{
    public class ExtendedWorkflowContext: DLaBExtendedWorkflowContext
    {
        public ExtendedWorkflowContext(CodeActivityContext executionContext, CodeActivity codeActivity) 
            : base(executionContext, codeActivity)
        {
        }
    }
}
