using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
    /// <summary>
    /// Allows for registering or overriding service registrations
    /// </summary>
    public interface IIocServiceProviderBuilder
    {
        /// <summary>
        /// Create the Service Provider.
        /// </summary>
        /// <param name="provider">The Dataverse Provider.</param>
        /// <param name="container">The Container.</param>
        /// <returns></returns>
        IServiceProvider BuildServiceProvider(IServiceProvider provider, IIocContainer container);
    }
}
