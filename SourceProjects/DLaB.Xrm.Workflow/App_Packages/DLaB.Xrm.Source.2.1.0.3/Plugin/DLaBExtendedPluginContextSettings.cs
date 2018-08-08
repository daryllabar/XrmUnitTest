#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    public class DLaBExtendedPluginContextSettings
    {
        public ExtendedOrganizationServiceSettings OrganizationServiceSettings { get; set; }

        public DLaBExtendedPluginContextSettings()
        {
            OrganizationServiceSettings = new ExtendedOrganizationServiceSettings();
        }
    }
}
