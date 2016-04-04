using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using DLaB.Xrm.Plugin;

namespace Example.Plugin
{
    public class AssignRandomNumberActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Field")]
        public InArgument<string> Field { get; set; }

        [Input("Entity")]
        [ReferenceTarget("Contact")]
        public InArgument<bool> Contact { get; set; }

        protected override void Execute(CodeActivityContext activityContext)
        {
            var context = activityContext.GetExtension<IWorkflowContext>();

        }
    }
}
