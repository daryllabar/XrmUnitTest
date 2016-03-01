using DLaB.Common;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Defines all the potential App.Config Values
    /// </summary>
    public class AppConfig : Client.AppConfig
    {
        private static bool? _useDebugCredentialsForTesting;

        /// <summary>
        /// Determines whether the DebugUserAccountName, DebugUserAccountPassword, and DebugUserAccountDomain should be used to connect to CRM, rather than the default windows credentials
        /// </summary>
        /// <value>
        /// <c>true</c> if [use debug credentials for testing]; otherwise, <c>false</c>.
        /// </value>
        public static bool UseDebugCredentialsForTesting
        {
            get
            {
                return GetValue(ref _useDebugCredentialsForTesting, "UseDebugCredentialsForTesting", true);
            }
            set { _useDebugCredentialsForTesting = value; }
        }

        private static string _orgName;

        /// <summary>
        /// The name of the Org of CRM you are connecting to.
        /// </summary>
        /// <value>
        /// The name of the org.
        /// </value>
        public static string OrgName
        {
            get { return _orgName ?? (_orgName = Config.GetAppSettingOrDefault("OrgName", "Specify \"OrgName\" in App.Config")); }
            set { _orgName = value; }
        }

        private static bool? _useLocalCrmDatabase;

        /// <summary>
        /// Controls if TestBase.GetOrganizationService returns a connection to an actual CRM, or if it returns a connection to an "In Memory" CRM.    
        /// </summary>
        /// <value>
        /// The use local CRM database.
        /// </value>
        public static bool UseLocalCrmDatabase
        {
            get { return GetValue(ref _useLocalCrmDatabase, "UseLocalCrmDatabase", true); }
            set { _useLocalCrmDatabase = value; }
        }
    }
}
