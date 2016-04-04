using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using DLaB.Xrm.Plugin;

namespace Example.Plugin
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
