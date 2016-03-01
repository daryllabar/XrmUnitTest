using System.Diagnostics;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    /// <summary>
    /// Handles mapping calls to the actual Test Framework Provider
    /// </summary>
    public class TestFrameworkProviderSettings
    {
        private string NotConfiguredMessage { get; }

        private ITestFrameworkProvider _value;
        /// <summary>
        /// Gets the TestFrameworkProivder.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="NotConfiguredException"></exception>
        public ITestFrameworkProvider Value
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
        /// Initializes a new instance of the <see cref="TestFrameworkProviderSettings"/> class.
        /// </summary>
        /// <param name="notConfiguredMessage">The not configured message.</param>
        public TestFrameworkProviderSettings(string notConfiguredMessage)
        {
            NotConfiguredMessage = notConfiguredMessage;
        }

        /// <summary>
        /// The Unit Test Framework Provider
        /// </summary>
        /// <param name="provider"></param>
        public void Configure(ITestFrameworkProvider provider)
        {
            Value = provider;
            IsConfigured = true;
        }

        #region Asserts

        [DebuggerHidden]
        internal void AssertAreEqual<T>(T o1, T o2, string message)
        {
            if (!o1.Equals(o2))
            {
                throw Value.GetFailedException(message);
            }
        }

        [DebuggerHidden]
        internal void AssertFail(string message)
        {
            throw Value.GetFailedException(message);
        }

        #endregion Asserts
    }
}
