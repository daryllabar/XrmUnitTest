#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// Represents a contract for recording the registration of plugin services.
    /// </summary>
    public interface IPluginServicesRegistrationRecorder
    {
        /// <summary>
        /// Registers the plugin services in the specified IoC container.
        /// </summary>
        /// <param name="container">The IoC container.</param>
        /// <param name="plugin">The registered events plugin.</param>
        /// <param name="unsecureConfig">The unsecure configuration string.</param>
        /// <param name="secureConfig">The secure configuration string.</param>
        /// <returns>The updated IoC container.</returns>
        IIocContainer RegisterPluginServices(IIocContainer container, IRegisteredEventsPlugin plugin = null, string unsecureConfig = null, string secureConfig = null);
    }
}
