using Microsoft.Xrm.Sdk.Extensions;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
using DLaB.Xrm.Ioc;

namespace DLaB.Xrm
#else
using Source.DLaB.Xrm.Ioc;

namespace Source.DLaB.Xrm
#endif
{
    /// <summary>
    /// Extensions for Ioc
    /// </summary>
    public static class IocExtensions
    {
        /// <summary>
        /// Registers the default types for use within the DLaB.Xrm library.
        /// </summary>
        /// <param name="container">The IoC container.</param>
        /// <returns>The IoC container.</returns>
        public static IIocContainer RegisterDLaBDefaults(this IIocContainer container)
        {
            // Order of registrations does not matter. 
            container
                // ICacheConfig
                .AddSingleton(s => DLaBXrmConfig.CacheConfig)

                // IEntityHelperConfig 
                .AddSingleton(s => DLaBXrmConfig.EntityHelperConfig)

                // MemoryCache
                .AddSingleton(s => s.Get<ICacheConfig>().GetCache())
                ;
            return container;
        }
    }
}
