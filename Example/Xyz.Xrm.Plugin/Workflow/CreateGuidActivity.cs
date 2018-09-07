using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace Xyz.Xrm.Plugin.Workflow
{
    public class CreateGuidActivity : CodeActivityBase
    {
        [Output("Guid")]
        public OutArgument<string> Guid { get; set; }

        protected override void Execute(ExtendedWorkflowContext activityContext)
        {
            Guid.Set(activityContext, System.Guid.NewGuid().ToString());
        }
    }
}