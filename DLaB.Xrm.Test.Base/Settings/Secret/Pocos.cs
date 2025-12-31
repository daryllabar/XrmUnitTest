using System.Collections.Generic;

namespace DLaB.Xrm.Test.Settings.Secret
{
    /// <summary>
    ///  App Settings
    /// </summary>
    public class AppSetting
    {
        /// <summary>
        /// The Key
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// The Value
        /// </summary>
        public string? Value { get; set; }
    }

    /// <summary>
    /// Dataverse System Settings
    /// </summary>
    public class DataverseSystemSettings
    {
        /// <summary>
        /// Format of FullName
        ///   F = First Name
        ///   M = Middle Name
        ///   I = Middle Initial
        ///   L = Last Name
        /// </summary>
        public string? FullNameFormat { get; set; }
    }

    /// <summary>
    /// Unit Test Settings
    /// </summary>
    public class DataverseUnitTestSettings
    {
        /// <summary>
        /// Use the local In Memory Dataverse Fake?
        /// </summary>
        public bool UseDataverseFake { get; set; }
        /// <summary>
        /// Connection
        /// </summary>
        public string? Connection { get; set; }
        /// <summary>
        /// Connections
        /// </summary>
        public List<NameValue?>? Connections { get; set; }
        /// <summary>
        /// Dataverse System Settings
        /// </summary>
        public DataverseSystemSettings? DataverseSystemSettings { get; set; }
        /// <summary>
        /// App Settings
        /// </summary>
        public List<AppSetting?>? AppSettings { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// Passwords
        /// </summary>
        public List<NameValue?>? Passwords { get; set; }
    }

    /// <summary>
    /// Contains Name Value
    /// </summary>
    public class NameValue
    {
        /// <summary>
        /// The Name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The Value
        /// </summary>
        public string? Value { get; set; }
    }
}
