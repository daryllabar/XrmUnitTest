using System;
using Microsoft.Xrm.Sdk.Workflow;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// A Fake that implements IExecutionContext
    /// </summary>
    public class FakeWorkflowContext : FakeExecutionContext, IWorkflowContext, ICloneable
    {
        /// <summary>
        /// Gets or sets the name of the stage.
        /// </summary>
        /// <value>
        /// The name of the stage.
        /// </value>
        public string StageName { get; set; }
        /// <summary>
        /// Gets or sets the workflow category.
        /// </summary>
        /// <value>
        /// The workflow category.
        /// </value>
        public int WorkflowCategory { get; set; }
        /// <summary>
        /// Gets or sets the workflow mode.
        /// </summary>
        /// <value>
        /// The workflow mode.
        /// </value>
        public int WorkflowMode { get; set; }
        /// <summary>
        /// Gets or sets the parent context.
        /// </summary>
        /// <value>
        /// The parent context.
        /// </value>
        public IWorkflowContext ParentContext { get; set; }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public new FakeWorkflowContext Clone()
        {
            var clone = (FakeWorkflowContext)MemberwiseClone();
            CloneReferenceValues(clone);
            if (ParentContext is ICloneable parent)
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
