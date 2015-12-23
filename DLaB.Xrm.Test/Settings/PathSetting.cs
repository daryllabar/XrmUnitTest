using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    /// <summary>
    /// Defines a Path Setting
    /// </summary>
    public class PathSetting
    {
        private string NotConfiguredMessage { get; }

        private string _value;
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="NotConfiguredException"></exception>
        public string Value
        {
            get
            {
                if (_value == null)
                {
                    throw new NotConfiguredException(NotConfiguredMessage);
                }
                return _value;
            }
            private set { _value = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is configured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </value>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSetting"/> class.
        /// </summary>
        /// <param name="notConfiguredMessage">The not configured message.</param>
        public PathSetting(string notConfiguredMessage)
        {
            NotConfiguredMessage = notConfiguredMessage;
        }

        /// <summary>
        /// Configures the Web Resources Path.
        /// </summary>
        /// <param name="finder"></param>
        public void Configure(IPathFinder finder)
        {
            Value = finder.GetPath();
            IsConfigured = true;
        }
    }
}
