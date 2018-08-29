using System.Activities;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public abstract class DLaBCodeActivityBase: DLaBGenericCodeActivityBase<DLaBExtendedWorkflowContext>
    {
        #region Overrides of DLaBGenericCodeActivityBase<DLaBExtendedWorkflowContext>

        protected override DLaBExtendedWorkflowContext CreateWorkflowContext(CodeActivityContext context)
        {
            return new DLaBExtendedWorkflowContext(context, this);
        }

        #endregion
    }
}
