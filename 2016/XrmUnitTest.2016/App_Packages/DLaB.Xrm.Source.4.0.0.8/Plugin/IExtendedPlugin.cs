#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm.Plugin
#endif

{
    /// <summary>
    /// Plugin Interface for the extended plugin
    /// </summary>
    public interface IExtendedPlugin : IRegisteredEventsPlugin, IContainerWrapper
    {
    }
}