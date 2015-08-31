using System;
using Microsoft.Xrm.Sdk.Workflow;

namespace DLaB.Xrm.Test
{
    public class FakeWorkflowContext : FakeExecutionContext, IWorkflowContext, ICloneable
    {
        public string StageName { get; set; }
        public int WorkflowCategory { get; set; }
        public int WorkflowMode { get; set; }
        public IWorkflowContext ParentContext { get; set; }

        #region Clone

        public new FakeWorkflowContext Clone()
        {
            var clone = (FakeWorkflowContext)MemberwiseClone();
            CloneReferenceValues(clone);
            var parent = ParentContext as ICloneable;
            if (parent != null)
            {
                ParentContext = (IWorkflowContext)parent.Clone();
            }
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion Clone
    }
}
