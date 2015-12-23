using System.Activities;

namespace XrmUnitTest.Test.Builders
{
    public class WorkflowInvokerBuilder : DLaB.Xrm.Test.Builders.WorkflowInvokerBuilderBase<WorkflowInvokerBuilder>
    {
        protected override WorkflowInvokerBuilder This
        {
            get { return this; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowInvokerBuilder"/> class.
        /// </summary>
        /// <param name="workflow">The workflow to invoke.</param>
        public WorkflowInvokerBuilder(Activity workflow) : base(workflow)
        {

        }
    }
}
