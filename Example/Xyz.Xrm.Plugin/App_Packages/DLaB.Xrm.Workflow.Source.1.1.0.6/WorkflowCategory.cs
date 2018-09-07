#if DLAB_UNROOT_NAMESPACE || DLAB_XRM_WORKFLOW
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public enum WorkflowCategory
    {
        Action = 3,
        BusinessProcessFlow = 4,
        BusinessRule = 2,
        Dialog = 1,
        Workflow = 0,
    }
}
