using Microsoft.Xrm.Sdk.Workflow;

namespace DLaB.Xrm.Test
{
    public class FakeWorkflowContext : FakeExecutionContext, IWorkflowContext
    {
        public string StageName { get; set; }
        public int WorkflowCategory { get; set; }
        public int WorkflowMode { get; set; }
        public IWorkflowContext ParentContext { get; set; }
    }
}
