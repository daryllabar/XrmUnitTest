using System;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    public class PathSetting
    {
        private string NotConfiguredMessage { get; set; }

        private String _value;
        public String Value
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
