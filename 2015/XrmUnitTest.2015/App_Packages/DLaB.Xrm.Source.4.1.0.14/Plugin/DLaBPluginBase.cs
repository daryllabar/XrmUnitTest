#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm.Plugin
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm.Plugin
#endif

{
    /// <summary>
    /// An abstract base Plugin that implements the DLaBGenericPluginBase.
    /// </summary>
    public abstract class DLaBPluginBase: DLaBGenericPluginBase<IExtendedPluginContext>
    {
        /// <inheritdoc />
        protected DLaBPluginBase(string unsecureConfig, string secureConfig, IIocContainer container = null): base(unsecureConfig, secureConfig, container)
        {
        }
    }
}
