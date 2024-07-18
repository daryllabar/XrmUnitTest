using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{

    /// <summary>
    /// Defines that a Container Property is defined
    /// </summary>
    public interface IContainerWrapper
    {
        /// <summary>
        /// The Container.
        /// </summary>
        IIocContainer Container { get; }
    }
}
