#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Workflow
#else
namespace Source.DLaB.Xrm.Workflow
#endif
{
    public class DLaBExtendedWorkflowContextSettings
    {
        public ExtendedOrganizationServiceSettings OrganizationServiceSettings { get; set; }

        public DLaBExtendedWorkflowContextSettings()
        {
            OrganizationServiceSettings = new ExtendedOrganizationServiceSettings();
        }
    }
}
