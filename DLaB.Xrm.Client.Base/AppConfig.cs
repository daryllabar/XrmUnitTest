using System;
using System.Collections.Generic;
using System.Data.Common;
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
            get
            {
                if (_connectionString != null)
                {
                    return _connectionString;
                }

                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = Config.GetAppSettingOrDefault(ConnectionPrefix + "ConnectionString", string.Empty)
                };
                builder.Add("Password", Password);

                return builder.ConnectionString;
            }
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
            private static string _primaryNameAttributeName;
            private static bool? _containsPrimaryAttributeName;
            private static Dictionary<string, string> _nonStandardAttributeNamesByEntity;
            private static List<string> _namelessEntities;

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

            /// <summary>
            /// Determines if the PrimaryNameViaFieldProvider is used (if true or not provided) or PrimaryNameViaNonStandardNamesProvider (if false)
            /// </summary>
            public static bool ContainPrimaryAttributeName
            {
                get
                {
                    if (!_containsPrimaryAttributeName.HasValue)
                    {
                        _containsPrimaryAttributeName = Config.GetAppSettingOrDefault("CrmEntities.TypesContainPrimaryAttributeName", true);
                    }

                    return _containsPrimaryAttributeName.Value;
                }
                set => _containsPrimaryAttributeName = value;
            }

            /// <summary>
            /// Ignored if EarlyBoundTypesContainPrimaryAttributeName is false
            /// </summary>
            public static string PrimaryNameAttributeName
            {
                get => _primaryNameAttributeName ?? (_primaryNameAttributeName = Config.GetAppSettingOrDefault("CrmEntities.PrimaryNameAttributeName", "PrimaryNameAttribute"));
                set => _primaryNameAttributeName = value;
            }

            /// <summary>
            /// Ignored if EarlyBoundTypesContainPrimaryAttributeName is true
            /// </summary>
            public static Dictionary<string,string> NonStandardAttributeNamesByEntity
            {
                get => _nonStandardAttributeNamesByEntity ?? (_nonStandardAttributeNamesByEntity = Config.GetAppSettingOrDefault("CrmEntities.NonStandardAttributeNamesByEntity", "").ToLower().GetDictionary<string, string>());
                set => _nonStandardAttributeNamesByEntity = value;
            }

            /// <summary>
            /// List of Entities that do not have a primary name attribute, in addition to the known entities
            /// </summary>
            public static List<string> NamelessEntities
            {
                get => _namelessEntities ?? (_namelessEntities = Config.GetList("CrmEntities.NamelessEntities", new List<string>()));
                set => _namelessEntities = value;
            }
        }

        /// <summary>
        /// Settings for Crm System
        /// </summary>
        public class CrmSystemSettings
        {
            private static string _fullNameFormat;
            private static Guid? _businessUnitId;
            private static Guid? _userId;
            private static Guid? _onBehalfOfId;

            /// <summary>
            /// The id to be used as the id oof top level Business Unit.
            /// </summary>
            public static Guid BusinessUnitId
            {
                get => (_businessUnitId 
                        ?? (_businessUnitId = Config.GetAppSettingOrDefault("CrmSystemSettings.BusinessUnitId",
                                                                            new Guid("88501fd6-90b5-405f-a027-ce9903bc0bb3")))
                        ).GetValueOrDefault();
                set => _businessUnitId = value;
            }

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

            /// <summary>
            /// The id to be used as the id of the current user.
            /// </summary>
            public static Guid OnBehalfOfId
            {
                get => (_onBehalfOfId
                        ?? (_onBehalfOfId = Config.GetAppSettingOrDefault("CrmSystemSettings.OnBehalfOfId", Guid.Empty))
                    ).GetValueOrDefault();
                set => _onBehalfOfId = value;
            }

            /// <summary>
            /// The id to be used as the id of the current user.
            /// </summary>
            public static Guid UserId
            {
                get => (_userId
                        ?? (_userId = Config.GetAppSettingOrDefault("CrmSystemSettings.UserId",
                            new Guid("ba815d1d-f62b-4ea1-912f-0aab76bd7462")))
                    ).GetValueOrDefault();
                set => _userId = value;
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
