using DLaB.Common;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Single place for all config read settings to be performed
    /// </summary>
    public class AppConfig
    {
        private static string _connectionString;

        /// <summary>
        /// The Connection string used to access CRM
        /// </summary>
        public static string ConnectionString
        {
            get => _connectionString ?? (_connectionString = Config.GetAppSettingOrDefault(ConnectionPrefix + "ConnectionString", string.Empty));
            set => _connectionString = value;
        }

        private static string _connectionPrefix;

        /// <summary>
        /// The Connection Prefix to use to determine the Connection String to use to connect.
        /// </summary>
        public static string ConnectionPrefix => _connectionPrefix ?? (_connectionPrefix = Config.GetAppSettingOrDefault("ConnectionPrefix", string.Empty));

        private static int? _defaultLanguageCode;

        /// <summary>
        /// Gets or sets the default language code.
        /// </summary>
        /// <value>
        /// The default language code.
        /// </value>
        public static int DefaultLanguageCode
        {
            get => GetValue(ref _defaultLanguageCode, "DefaultLanguageCode", 1033);
            set => _defaultLanguageCode = value;
        }

        private static string _password;

        /// <summary>
        /// Password for the Debug User Account
        /// </summary>
        /// <value>
        /// The debug user account password.
        /// </value>
        public static string Password
        {
            get => _password ?? (_password = Config.GetAppSettingOrDefault(ConnectionPrefix + "Password",
                       Config.GetAppSettingOrDefault("Password", string.Empty)));
            set => _password = value;
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
                
                get => _contextType ?? (_contextType = Config.GetAppSettingOrDefault("DLaB.Xrm.Entities.CrmContext, DLaB.Xrm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", string.Empty));
                set => _contextType = value;
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
                get => _fullNameFormat ?? (_fullNameFormat = Config.GetAppSettingOrDefault("CrmSystemSettings.FullNameFormat", "F I L").ToUpper());
                set => _fullNameFormat = value;
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
