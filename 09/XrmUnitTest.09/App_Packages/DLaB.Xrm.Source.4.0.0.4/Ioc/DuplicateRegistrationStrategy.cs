#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
    /// <summary>
    /// Describes how to handle Duplicate Registration Scenarios
    /// </summary>
    public enum DuplicateRegistrationStrategy
    {
        /// <summary>
        /// Subsequent duplicate type registrations will override the previous registration (Last one in wins)
        /// </summary>
        Override,
        /// <summary>
        /// Subsequent duplicate type registrations will be ignored (First one in wins)
        /// </summary>
        Ignore,
        /// <summary>
        /// Subsequent duplicate type registrations will throw an exception
        /// </summary>
        Throw
    }
}
