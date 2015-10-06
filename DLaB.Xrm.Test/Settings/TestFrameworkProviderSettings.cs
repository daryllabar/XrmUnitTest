using System;
using System.Diagnostics;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    public class TestFrameworkProviderSettings
    {
        private string NotConfiguredMessage { get; set; }

        private ITestFrameworkProvider _value;
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

        public bool IsConfigured { get; set; }

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
