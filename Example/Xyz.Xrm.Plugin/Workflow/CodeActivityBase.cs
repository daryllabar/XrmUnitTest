using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Source.DLaB.Xrm.Workflow;

namespace Xyz.Xrm.Plugin.Workflow
{
    public abstract class CodeActivityBase : DLaBCodeActivityBase
    {
        #region Overrides of DLaBCodeActivityBase

        protected override IExtendedWorkflowContext CreateWorkflowContext(CodeActivityContext context)
        {
            return new ExtendedWorkflowContext(context, this);
        }

        protected sealed override void Execute(IExtendedWorkflowContext context)
        {
            Execute((ExtendedWorkflowContext)context);
        }

        #endregion

        protected abstract void Execute(ExtendedWorkflowContext context);
    }
}
