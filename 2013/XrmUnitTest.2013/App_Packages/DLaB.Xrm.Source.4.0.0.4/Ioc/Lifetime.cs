#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
#pragma warning disable CA1724 // CA1724: Type names should not match namespaces.  The namespace System.Runtime.Remoting.Lifetime is not going to be referenced in the same file.
    /// <summary>
    /// Defines the lifetime of an instance.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// Type is created at most once per request, and then reused.
        /// </summary>
        Scoped = 1,
        /// <summary>
        /// Type is created at most once per container, and then reused.
        /// </summary>
        Singleton = 0,
        /// <summary>
        /// A new instance of the Type is created every time.
        /// </summary>
        Transient = 2,
    }
#pragma warning restore CA1724
}
