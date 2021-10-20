#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Common;
#else
using Source.DLaB.Common;
#endif

#if NET
using Client = DLaB.Xrm.Client;

namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Defines all the potential App.Config Values
    /// </summary>
    public class AppConfig : Client.AppConfig
    {
        private static string _orgName;

        /// <summary>
        /// The name of the Org of CRM you are connecting to.
        /// </summary>
        /// <value>
        /// The name of the org.
        /// </value>
        public static string OrgName
        {
            get
            {
                if (_orgName != null)
                {
                    return _orgName;
                }

                _orgName = Config.GetAppSettingOrDefault("OrgName", "Specify \"OrgName\" in App.Config");
                if (_orgName.EndsWith("."))
                {
                    _orgName.Remove(_orgName.Length - 1);
                }

                return _orgName;

            }
            set => _orgName = value;
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
            get => GetValue(ref _useLocalCrmDatabase, "UseLocalCrmDatabase", true);
            set => _useLocalCrmDatabase = value;
        }
    }
}
