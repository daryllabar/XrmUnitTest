using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace Example.Plugin.Workflow
{
    public class CreateGuidActivity : CodeActivity
    {
        [Output("Guid")]
        public OutArgument<string> Guid { get; set; }

        protected override void Execute(CodeActivityContext activityContext)
        {
            Guid.Set(activityContext, System.Guid.NewGuid().ToString());
        }
    }
}