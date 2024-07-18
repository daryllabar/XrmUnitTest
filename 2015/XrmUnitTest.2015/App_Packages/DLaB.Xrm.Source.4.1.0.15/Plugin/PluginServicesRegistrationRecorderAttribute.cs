using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// Attribute used to declare the Type of <see cref="IPluginServicesRegistrationRecorder"/> that will be invoked and used to register services
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class PluginServicesRegistrationRecorderAttribute : Attribute
    {
        /// <summary>
        /// The Recorder Type to record all the registrations to the Service Provider.
        /// </summary>
        public Type Recorder { get; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="recorder">The Type of the <see cref="IPluginServicesRegistrationRecorder"/> class to register.</param>
        public PluginServicesRegistrationRecorderAttribute(Type recorder)
        {
            Recorder = recorder;
        }
    }
}
