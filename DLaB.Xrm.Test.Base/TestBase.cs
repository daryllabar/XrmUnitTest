using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DLaB.Xrm.Client;
using DLaB.Xrm.LocalCrm;
using ConfigManager = System.Configuration.ConfigurationManager;

#if NET
using DLaB.Xrm.Test.Settings.Secret;
using Microsoft.Extensions.Configuration;
using DLaB.Xrm;

namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Base Test class to create the Organization Service Proxy defined by the config, as well as other values
    /// </summary>
    public class TestBase
    {
        /// <summary>
        /// Gets or sets the name of the org.
        /// </summary>
        /// <value>
        /// The name of the org.
        /// </value>
        public static string OrgName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [use local CRM database].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use local CRM database]; otherwise, <c>false</c>.
        /// </value>
        public static bool UseLocalCrmDatabase { get; set; }

        static TestBase()
        {
            UserUnitTestSettingsLoaded = false;
        }

        #region GetOrganizationServiceProxy

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <param name="organizationName">Name of the organization.</param>
        /// <param name="impersonationUserId">The impersonation user identifier.</param>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService(string organizationName = null,
            Guid impersonationUserId = new Guid())
        {
            LoadUserUnitTestSettings();
            organizationName = organizationName ?? OrgName;
            if (UseLocalCrmDatabase)
            {
                return GetLocalCrmDatabaseOrganizationService(organizationName, impersonationUserId);
            }
            else
            {
                var service = CrmServiceUtility.GetOrganizationService();
                if (service == null)
                {
                    throw new Exception("Organization Service was Null!");
                }
                return service;
            }
        }

        private static IClientSideOrganizationService GetLocalCrmDatabaseOrganizationService(string organizationName, Guid impersonationUserId)
        {
            // Create a unique Database for each Unit Test by looking up the first method in the stack trace that has a TestMethodAttribute,
            // and using it's method handle, combined with the OrganizationName, as a unique Key
            var method = GetUnitTestMethod() ?? MethodBase.GetCurrentMethod();
            var databaseKey = $"UnitTest {method?.Name ?? "NULL"}:{organizationName}:{method?.MethodHandle}";
            return new LocalCrmDatabaseOrganizationService(GetConfiguredLocalDatabaseInfo(databaseKey, impersonationUserId));
        }

        /// <summary>
        /// Returns a local Crm Database Info from the TestSettings
        /// </summary>
        /// <param name="databaseKey">Key to Use</param>
        /// <param name="impersonationUserId">Impersonation User</param>
        /// <returns></returns>
        public static LocalCrmDatabaseInfo GetConfiguredLocalDatabaseInfo(string databaseKey, Guid impersonationUserId)
        {
            return LocalCrmDatabaseInfo.Create(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, databaseKey, Guid.NewGuid(), impersonationUserId, Guid.NewGuid());
        }

        private static MethodBase GetUnitTestMethod()
        {
            var frames = new StackTrace().GetFrames();
            if (frames == null)
            {
                throw new Exception("Unable to get the StackTrace");
            }

            return frames.Reverse() // Stacks are LIFO, Reverse to start at the bottom.
                         .Select(frame => frame.GetMethod())
                         .Where(m => m != null)
                         .FirstOrDefault(method => method.GetCustomAttributes(false).Any(o => o.GetType() == TestSettings.TestFrameworkProvider.Value.TestMethodAttributeType));
        }

        #endregion GetOrganizationServiceProxy

        #region UnitTestSettings

        private static bool UserUnitTestSettingsLoaded { get; set; }

        private static readonly object LocalSettingsLock = new object();
        /// <summary>
        /// Loads the user unit test settings in a multi-thread safe manner, verifying that it is loaded once only.
        /// </summary>
        public static void LoadUserUnitTestSettings()
        {
            if (UserUnitTestSettingsLoaded) { return; }

            lock (LocalSettingsLock)
            {
                if (UserUnitTestSettingsLoaded) { return; }
                LoadUserUnitTestSettingsInternal();
                UserUnitTestSettingsLoaded = true;
            }
        }

        private static void LoadUserUnitTestSettingsInternal()
        {
#if NET
            if (!TestSettings.UserTestConfigPath.IsConfigured && TestSettings.TestFrameworkProvider.IsConfigured)
            {
                AddJsonSettingsToAppConfig(null);
            }
            else
            {
                var userConfigPath = TestSettings.UserTestConfigPath.Value;
                if (userConfigPath.ToLower().EndsWith(".json"))
                {
                    AddJsonSettingsToAppConfig(userConfigPath);
                }
                else
                {
#else
            var userConfigPath = TestSettings.UserTestConfigPath.Value;
#endif
                    var userConfig = GetUserConfig(userConfigPath);
                    if (userConfig != null)
                    {
                        AddSettingsToAppConfig(userConfig);
                    }
#if NET
                }
            }
#endif

            OrgName = AppConfig.OrgName;
            UseLocalCrmDatabase = AppConfig.UseLocalCrmDatabase;
        }

        #endregion UnitTestSettings

        #region UnitTestSetting.user.config

        private static void AddSettingsToAppConfig(Configuration userConfig)
        {
            var config = ConfigManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var fileName = Path.GetFileName(config.FilePath);
            if (fileName == "vstest.executionengine.x86.exe.Config" || fileName == "te.processhost.managed.exe.Config")
            {
                throw new Exception("Unit Test Project Must Contain an App.Config file to be able to Load User Settings into!");
            }
            var update = false;

            // Load AppSettings into App.Config for settings that aren't already contained in the App.Config
            foreach (var setting in userConfig.AppSettings.Settings.Cast<KeyValueConfigurationElement>())
            {
                var appSetting = config.AppSettings.Settings[setting.Key];
                if (appSetting == null)
                {
                    // Add
                    update = true;
                    config.AppSettings.Settings.Add(setting.Key, setting.Value);
                    ConfigManager.AppSettings.Set(setting.Key, setting.Value); // Set in Memory
                }
                else if (appSetting.Value != setting.Value)
                {
                    // Update
                    update = true;
                    appSetting.Value = setting.Value;
                    ConfigManager.AppSettings.Set(setting.Key, setting.Value); // Update in Memory
                }
            }

            if (update)
            {
                config.Save(ConfigurationSaveMode.Modified, false);
                ConfigManager.RefreshSection("appSettings");
            }
        }

        private static Configuration GetUserConfig(string userConfigPath)
        {
            if (!File.Exists(userConfigPath) && userConfigPath?.EndsWith("user.config") == true)
            {
                // Attempt to lookup the non User Config settings.  This is used when the user config is copied over from a checked in version
                var index = userConfigPath.LastIndexOf("user.config", StringComparison.Ordinal);
                var configPath = userConfigPath.Remove(index, "user.".Length);

                // Copy Non User config to User config
                File.Copy(configPath, userConfigPath);
            }

            if (!File.Exists(userConfigPath))
            {
                return null;
            }

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = userConfigPath,
            };
            return ConfigManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        }

        #endregion UnitTestSetting.user.config

        #region Secrets Json
#if NET

        public const string DataverseUnitTestSettingsName = "dataverseUnitTestSettings";

        private static void AddJsonSettingsToAppConfig(string jsonConfigPath)
        {
            var builder = new ConfigurationBuilder();
            if (jsonConfigPath == null)
            {
                builder.AddUserSecrets(TestSettings.TestFrameworkProvider.Value.GetType().Assembly);
            }
            else
            {
                builder.AddJsonFile(jsonConfigPath);
            }
            var configuration = builder.Build();
            var settings = new DataverseUnitTestSettings
            {
                UseDataverseFake = true,
                DataverseSystemSettings = new DataverseSystemSettings
                {
                    FullNameFormat = "F I L"
                }
            };
            configuration.Bind(DataverseUnitTestSettingsName, settings);

            var config = ConfigManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var fileName = Path.GetFileName(config.FilePath);
            // ReSharper disable StringLiteralTypo
            if (fileName == "vstest.executionengine.x86.exe.Config" || fileName == "te.processhost.managed.exe.Config")
                // ReSharper restore StringLiteralTypo
            {
                throw new Exception("Unit Test Project Must Contain an App.Config file to be able to Load User Settings into!");
            }

            LoadDefaultSettings(config, settings);
            LoadAppSettings(settings, config);

            config.Save(ConfigurationSaveMode.Modified, false);
            ConfigManager.RefreshSection("appSettings");
        }

        private static void LoadDefaultSettings(Configuration config, DataverseUnitTestSettings settings)
        {
            AddOrUpdate(config, "UseLocalCrmDatabase", settings.UseDataverseFake.ToString());
            AddOrUpdate(config, "ConnectionPrefix", settings.Connection + ".");
            if (settings.Connections != null) {
                foreach (var connection in settings.Connections.Where(c => c != null))
                {
                    AddOrUpdate(config, connection.Name + ".ConnectionString", connection.Value);
                }
            }

            AddOrUpdate(config, "CrmSystemSettings.FullNameFormat", settings.DataverseSystemSettings?.FullNameFormat);
            AddOrUpdate(config, "Password", settings.Password);
            
            if (settings.Passwords != null) {
                foreach (var password in settings.Passwords.Where(p => p != null))
                {
                    AddOrUpdate(config, password.Name + ".Password", password.Value);
                }
            }
        }

        private static void LoadAppSettings(DataverseUnitTestSettings settings, Configuration config)
        {
            if (settings.AppSettings != null)
            {
                foreach (var setting in settings.AppSettings.Where(s => s != null))
                {
                    var key = setting.Key;
                    var value = setting.Value;
                    AddOrUpdate(config, key, value);
                }
            }
        }

        private static void AddOrUpdate(Configuration config, string key, string value)
        {
            var appSettings = ConfigManager.AppSettings;
            var appSetting = config.AppSettings.Settings[key];
            if (appSetting == null)
            {
                if (value == null)
                {
                    return;
                }
                // Add
                config.AppSettings.Settings.Add(key, value);
                appSettings.Set(key, value); // Set in Memory
            }
            else if (appSetting.Value != value)
            {
                if (value == null)
                {
                    return;
                }
                // Update
                appSetting.Value = value;
                appSettings.Set(key, value); // Update in Memory
            }
        }
#endif

        #endregion Secrets Json

        /// <summary>
        /// Gets the Entity type based on the entity logical name.
        /// </summary>
        /// <param name="entityLogicalName">Name of the entity logical.</param>
        /// <returns></returns>
        public static Type GetType(string entityLogicalName)
        {
            return EntityHelper.GetType(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, entityLogicalName);
        }
    }
}
