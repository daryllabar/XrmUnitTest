#if !NET
using System.Activities;

namespace XrmUnitTest.Test.Builders
{
    /// <summary>
    /// Workflow Builder
    /// </summary>
    public class WorkflowInvokerBuilder : DLaB.Xrm.Test.Builders.WorkflowInvokerBuilderBase<WorkflowInvokerBuilder>
    {
        /// <summary>
        /// Gets the current instance of the builder.
        /// </summary>
        protected override WorkflowInvokerBuilder This => this;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder"/> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        public WorkflowInvokerBuilder(Activity workflow) : base(workflow)
        {

        }
    }
}
#endif