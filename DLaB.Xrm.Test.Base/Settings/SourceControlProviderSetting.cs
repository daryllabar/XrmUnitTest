using DLaB.Common.VersionControl;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    /// <summary>
    /// The Source Control Provider Setting
    /// </summary>
    public class SourceControlProviderSetting
    {
        private string NotConfiguredMessage { get; }

        private ISourceControlProvider _value;
        /// <summary>
        /// Gets the SourceControlProvider.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="NotConfiguredException"></exception>
        public ISourceControlProvider Value
        {
            get
            {
                if (_value == null)
                {
                    throw new NotConfiguredException(NotConfiguredMessage);
                }
                return _value;
            }
            private set => _value = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is configured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </value>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlProviderSetting"/> class.
        /// </summary>
        /// <param name="notConfiguredMessage">The not configured message.</param>
        public SourceControlProviderSetting(string notConfiguredMessage)
        {
            NotConfiguredMessage = notConfiguredMessage;
        }

        /// <summary>
        /// The Source Control Provider
        /// </summary>
        /// <param name="provider"></param>
        public void Configure(ISourceControlProvider provider)
        {
            Value = provider;
            IsConfigured = true;
        }

        /// <summary>
        /// The Source Control Provider
        /// </summary>
        public void ConfigureNone()
        {
            Configure(new NoSourceControlProvider());
        }

        /// <summary>
        /// The Source Control Provider
        /// </summary>
        public void ConfigureTfs()
        {
            Configure(new VsTfsSourceControlProvider());
        }
    }
}
