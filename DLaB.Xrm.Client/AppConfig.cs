using DLaB.Common;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Single place for all config read settings to be performed
    /// </summary>
    public class AppConfig
    {

        private static string _crmDiscoveryServerUrl;

        /// <summary>
        /// Gets or sets the CRM discovery server URL.
        /// </summary>
        /// <value>
        /// The CRM discovery server URL.
        /// </value>
        public static string CrmDiscoveryServerUrl
        {
            get { return _crmDiscoveryServerUrl ?? (_crmDiscoveryServerUrl = Config.GetAppSettingOrDefault("CrmDiscoveryServerUrl", string.Empty)); }
            set { _crmDiscoveryServerUrl = value; }
        }

        private static string _crmServerUrl;

        /// <summary>
        /// Base Url to the Crm Server
        /// </summary>
        /// <value>
        /// The CRM server URL.
        /// </value>
        public static string CrmServerUrl
        {
            get { return _crmServerUrl ?? (_crmServerUrl = Config.GetAppSettingOrDefault("CrmServerUrl", string.Empty)); }
            set { _crmServerUrl = value; }
        }


        private static string _debugUserAccountName;

        /// <summary>
        /// Domain\UserName when connecting to IFD, just use the UserName when connecting to a non IFD and populate the DebugUserAccountDomain config value
        /// </summary>
        /// <value>
        /// The name of the debug user account.
        /// </value>
        public static string DebugUserAccountName
        {
            get { return _debugUserAccountName ?? (_debugUserAccountName = Config.GetAppSettingOrDefault("DebugUserAccountName", string.Empty)); }
            set { _debugUserAccountName = value; }
        }

        private static string _debugUserAccountPassword;

        /// <summary>
        /// Password for the Debug User Account
        /// </summary>
        /// <value>
        /// The debug user account password.
        /// </value>
        public static string DebugUserAccountPassword
        {
            get { return _debugUserAccountPassword ?? (_debugUserAccountPassword = Config.GetAppSettingOrDefault("DebugUserAccountPassword", string.Empty)); }
            set { _debugUserAccountPassword = value; }
        }

        private static string _debugUserAccountDomain;

        /// <summary>
        /// Domain of Non-IFD CRM connections
        /// </summary>
        /// <value>
        /// The debug user account domain.
        /// </value>
        public static string DebugUserAccountDomain
        {
            get { return _debugUserAccountDomain ?? (_debugUserAccountDomain = Config.GetAppSettingOrDefault("DebugUserAccountDomain", string.Empty)); }
            set { _debugUserAccountDomain = value; }
        }

        private static int? _defaultLanguageCode;

        /// <summary>
        /// Gets or sets the default language code.
        /// </summary>
        /// <value>
        /// The default language code.
        /// </value>
        public static int DefaultLanguageCode
        {
            get { return GetValue(ref _defaultLanguageCode, "DefaultLanguageCode", 1033); }
            set { _defaultLanguageCode = value; }
        }


        /// <summary>
        /// CrmEntities Settings
        /// </summary>
        public class CrmEntities
        {
            private static string _contextType;

            /// <summary>
            /// The type of the crm context definition.  This is used to determine the assembly of the early bound entities
            /// </summary>
            /// <value>
            /// The type of the context.
            /// </value>
            public static string ContextType
            {
                
                get { return _contextType ?? (_contextType = Config.GetAppSettingOrDefault("DLaB.Xrm.Entities.CrmContext, DLaB.Xrm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", string.Empty)); }
                set { _contextType = value; }
            }

        }

        /// <summary>
        /// Settings for Crm System
        /// </summary>
        public class CrmSystemSettings
        {
            private static string _fullNameFormat;

            /// <summary>
            /// Defines the full name format.  Defaults to F I L <para/>
            /// Format of FullName <para/>
            ///   F = First Name <para/>
            ///   M = Middle Name <para/>
            ///   I = Middle Initial <para/>
            ///   L = Last Name 
            /// </summary>
            /// <value>
            /// The full name format (always upper case).
            /// </value>
            public static string FullNameFormat
            {
                get { return _fullNameFormat ?? (_fullNameFormat = Config.GetAppSettingOrDefault("CrmSystemSettings.FullNameFormat", "F I L").ToUpper()); }
                set { _fullNameFormat = value; }
            }
        }   

        #region Helpers

        /// <summary>
        /// Helper method to default the field to the config value if it is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The field.</param>
        /// <param name="configName">Name of the configuration.</param>
        /// <param name="default">The default.</param>
        /// <returns></returns>
        protected static T GetValue<T>(ref T? field, string configName, T @default) where T : struct
        {
            if (!field.HasValue)
            {
                field = Config.GetAppSettingOrDefault(configName, @default);
            }
            return field.Value;
        }

        #endregion Helpers
    }
}
