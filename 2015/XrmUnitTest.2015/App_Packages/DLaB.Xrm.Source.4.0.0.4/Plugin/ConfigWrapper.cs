#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// Wrapper of Plugin Secure and Unsecure Configuration values
    /// </summary>
    public class ConfigWrapper
    {
        /// <summary>
        /// The Unsecure Config Value
        /// </summary>
        public string UnsecureConfig { get; set; }
        /// <summary>
        /// The Secure Config Value
        /// </summary>
        public string SecureConfig { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigWrapper"/> class.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration value.</param>
        /// <param name="secureConfig">The secure configuration value.</param>
        public ConfigWrapper(string unsecureConfig = null, string secureConfig = null)
        {
            UnsecureConfig = unsecureConfig;
            SecureConfig = secureConfig;
        }
    }
}
