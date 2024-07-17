#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm.Plugin
#endif
{
    public interface IPluginServicesRegistrationRecorder
    {
        IIocContainer RegisterPluginServices(IIocContainer container, IRegisteredEventsPlugin plugin = null, string unsecureConfig = null, string secureConfig = null);
    }
}
