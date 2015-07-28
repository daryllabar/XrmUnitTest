using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DLaB.Common;
using DLaB.Xrm.Client;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.Test
{
    public class TestBase
    {
        public static string OrgName { get; set; }
        public static bool UseLocalCrmDatabase { get; set; }

        static TestBase()
        {
            UserUnitTestSettingsLoaded = false;
        }

        #region GetOrganizationServiceProxy

        public static IClientSideOrganizationService GetOrganizationService(string organizationName = null,
            Guid impersonationUserId = new Guid(), bool enableProxyTypes = true)
        {
            LoadUserUnitTestSettings();
            organizationName = organizationName ?? OrgName;
            if (UseLocalCrmDatabase) { return GetLocalCrmDatabaseOrganizationService(organizationName, impersonationUserId); }

            var info = GetCrmServiceEntity(organizationName, enableProxyTypes);

            if (Config.GetAppSettingOrDefault("UseDebugCredentialsForTesting", true))
            {
                // Only in Unit tests should this be allowed.
                info.UserName = ConfigurationManager.AppSettings["DebugUserAccountName"];
                info.UserPassword = ConfigurationManager.AppSettings["DebugUserAccountPassword"];
                info.UserDomainName = ConfigurationManager.AppSettings["DebugUserAccountDomain"];
            }
            return CrmServiceUtility.GetOrganizationService(info);
        }

        private static IClientSideOrganizationService GetLocalCrmDatabaseOrganizationService(string organizationName, Guid impersonationUserId)
        {
            // Create a unique Database for each Unit Test by looking up the first method in the stack trace that has a TestMethodAttribute,
            // and using it's method handle, combined with the OrganizationName, as a unqiue Key
            var method = GetUnitTestMethod() ?? System.Reflection.MethodBase.GetCurrentMethod();
            string databaseKey = String.Format("UnitTest {0}:{1}:{2}", method.Name, organizationName, method.MethodHandle);

            var info = LocalCrmDatabaseInfo.Create<CrmContext>(databaseKey, impersonationUserId);

            var service = new LocalCrmDatabaseOrganizationService(info);

            // Create BU and SystemUser for currently executing user
            var bu = new BusinessUnit
            {
                Name = "Currently Executing BusinessUnit"
            };
            bu.Id = service.Create(bu);

            var id = service.Create(new SystemUser
            {
                FirstName = Environment.UserDomainName.Split('/').First(),
                LastName = Environment.UserName,
                BusinessUnitId = bu.ToEntityReference(),
            });

            info = LocalCrmDatabaseInfo.Create<CrmContext>(databaseKey, id, impersonationUserId, bu.Id);

            return new LocalCrmDatabaseOrganizationService(info);
        }

        private static System.Reflection.MethodBase GetUnitTestMethod()
        {
            var frames = new System.Diagnostics.StackTrace().GetFrames();
            if (frames == null)
            {
                throw new Exception("Unable to get the StackTrace");
            }

            return frames.Reverse(). // Stacks are LIFO, Reverse to start at the bottom.
                          Select(frame => frame.GetMethod()).
                          FirstOrDefault(method => method.GetCustomAttributes(false).OfType<TestMethodAttribute>().Any());
        }

        public static CrmServiceInfo GetCrmServiceEntity(bool enableProxyTypes = true)
        {
            return GetCrmServiceEntity(OrgName, enableProxyTypes);
        }

        public static CrmServiceInfo GetCrmServiceEntity(string organizationName, bool enableProxyTypes = true)
        {
            LoadUserUnitTestSettings();
            return new CrmServiceInfo(organizationName) { EnableProxyTypes = enableProxyTypes };
        }

        #endregion // GetOrganizationServiceProxy

        /// <summary>
        /// Gets the project path of the given type. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetProjectPath(Type type)
        {
            var dll = new FileInfo(type.Assembly.Location);
            string solutionFolder = null;
            var sb = new System.Text.StringBuilder();
            
            if (dll.Directory == null || // ...\Solution\Project\bin\Build 
                dll.Directory.Parent == null || // ...\Solution\Project\bin
                dll.Directory.Parent.Parent == null || // ...\Solution\Project
                dll.Directory.Parent.Parent.Parent == null) // ...\Solution
            {
                sb.AppendLine("Checking for VSOnline");
                sb.AppendLine(dll.DirectoryName);
                if (dll.DirectoryName == @"C:\a\bin")
                {
                    // Build is on VSOnline.  Redirect to other c:\a\src\Branch Name
                    var s = new System.Diagnostics.StackTrace(true);
                    sb.AppendLine(s.ToString());
                    for (var i = 0; i < s.FrameCount; i++)
                    {
                        var fileName = s.GetFrame(i).GetFileName();
                        sb.AppendLine(fileName ?? String.Empty);
                        if (!String.IsNullOrEmpty(fileName))
                        {
                            // File name will be in the form of c:\a\src\Branch Name\project\filename.  Get everything up to and including the Branch Name
                            var parts = fileName.Split(Path.DirectorySeparatorChar);
                            solutionFolder = Path.Combine(parts[0] + Path.DirectorySeparatorChar + parts[1], parts[2], parts[3]);
                            sb.AppendLine(solutionFolder);
                            break;
                        }
                    }
                }

                if (String.IsNullOrWhiteSpace(solutionFolder))
                {
                    throw new Exception("Unable to find Project Path for " + type.FullName + ".  Assembly Located at " + type.Assembly.Location + sb);
                }
            }
            else
            {
                solutionFolder = dll.Directory.Parent.Parent.Parent.FullName;
            }

            // Class Name, Project Name, Version, Culture, PublicKeyTyoken
            // ReSharper disable once PossibleNullReferenceException
            var projectName = type.AssemblyQualifiedName.Split(',')[1].Trim();
            sb.AppendLine("Project Name" + projectName);
            sb.AppendLine("SolutionFolder " + solutionFolder);
            var projectPath = Path.Combine(solutionFolder, projectName);

            sb.AppendLine("Project Folder " + projectPath);
            if (!Directory.Exists(projectPath))
            {
                throw new Exception(String.Format("Unable to find Project Path for {0} at {1}. Log {2}", type.FullName, projectPath, sb));  
            }
            
            return projectPath;
        }

        /// <summary>
        /// Gets the Text Xml at the given path.
        /// </summary>
        /// <param name="xmlFileName"></param>
        /// <param name="projectRelativeFilePath"></param>
        /// <param name="type">A Type in the same project as the Test XML is located.  This is used to determine the Path</param>
        /// <returns></returns>
        public static string GetTestXml(string xmlFileName, string projectRelativeFilePath = "Entity Xml", Type type = null)
        {
            type = type ?? typeof(TestBase);
            var projectPath = GetProjectPath(type);
            var path = Path.Combine(projectPath, projectRelativeFilePath, (xmlFileName.EndsWith(".xml") ? xmlFileName : xmlFileName + ".xml"));
            var xml = File.ReadAllText(path);
            return xml;
        }

        #region UnitTestSetting.user.config

        private static bool UserUnitTestSettingsLoaded { get; set; }

        private static readonly object LocalSettingsLock = new Object();
        /// <summary>
        /// Loads the user unit test settings in a multi-thread safe manner, verifying that it is loaded once only.
        /// </summary>
        private static void LoadUserUnitTestSettings()
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
            var projectPath = GetProjectPath(typeof(TestBase));
            var userConfig = GetUserConfig(projectPath);

            AddSettingsToAppConfig(userConfig);

            OrgName = Config.GetAppSettingOrDefault("OrgName", "DominionCmxDev");
            UseLocalCrmDatabase = Config.GetAppSettingOrDefault("UseLocalCrmDatabase", false);
        }

        private static void AddSettingsToAppConfig(Configuration userConfig)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (Path.GetFileName(config.FilePath) == "vstest.executionengine.x86.exe.Config")
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
                }
                else if (appSetting.Value != setting.Value)
                {
                    // Update
                    update = true;
                    appSetting.Value = setting.Value;
                }
            }

            if (update)
            {
                config.Save(ConfigurationSaveMode.Modified, false);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private static Configuration GetUserConfig(string projectPath)
        {
            var userConfigPath = Path.Combine(projectPath, "UnitTestSettings.user.config");

            if (!File.Exists(userConfigPath))
            {
                // Copy Non User config to User config
                File.Copy(Path.Combine(projectPath, "UnitTestSettings.config"), userConfigPath);
            }

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = userConfigPath,
            };
            return ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        }

        #endregion // UnitTestSetting.user.config
    }
}
