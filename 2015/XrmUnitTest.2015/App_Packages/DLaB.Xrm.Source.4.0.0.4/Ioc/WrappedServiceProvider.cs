using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
    /// <summary>
    /// Used to get the wrapped Service Provider from the ScopedServiceProvider
    /// </summary>
    public class WrappedServiceProvider
    {
        /// <summary>
        /// The Wrapped Service Provider
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Constructor to set the Service Provider
        /// </summary>
        /// <param name="serviceProvider"></param>
        public WrappedServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
