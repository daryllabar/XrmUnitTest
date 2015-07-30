using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test
{
    public class TestSettings
    {
        #region AssumptionXml

        private static string AssumptionXmlPath { get; set; }
        public static bool IsAssumptionXmlPathConfigured { get; set; }

        /// <summary>
        /// Configures the Assumption Entity Xml Path.
        /// </summary>
        /// <param name="finder"></param>
        public static void ConfigureAssumptionXml(IPathFinder finder)
        {
            AssumptionXmlPath = finder.GetPath();
            IsAssumptionXmlPathConfigured = true;
        }

        public static string GetAssumptionXmlPath()
        {
            if (AssumptionXmlPath == null)
            {
                throw new NotConfiguredException("Assumption Xml has not been configured.  Call ConfigureAssumptionXml() first before getting the AssumptionXmlPath.");
            }

            return AssumptionXmlPath;
        }

        #endregion AssumptionXml

        #region WebResource

        private static string WebResourcePath { get; set; }
        public static bool IsWebResourcePathConfigured { get; set; }

        /// <summary>
        /// Configures the Web Resources Path.
        /// </summary>
        /// <param name="finder"></param>
        public static void ConfigureWebResource(IPathFinder finder)
        {
            WebResourcePath = finder.GetPath();
            IsWebResourcePathConfigured = true;
        }

        public static string GetWebResourcePath()
        {
            if (WebResourcePath == null)
            {
                throw new NotConfiguredException("Assumption Xml has not been configured.  Call ConfigureWebResource() first before getting the WebResourcePath.");
            }
            return WebResourcePath;
        }

        #endregion WebResource

        #region UserTestConfig

        private static string UserTestConfigPath { get; set; }
        public static bool IsUserTestConfigPathConfigured { get; set; }

        /// <summary>
        /// Configures the Assumption Entity Xml Path.
        /// </summary>
        /// <param name="finder"></param>
        public static void ConfigureUserTestConfig(IPathFinder finder)
        {
            UserTestConfigPath = finder.GetPath();
            IsUserTestConfigPathConfigured = true;
        }

        public static string GetUserTestConfigPath()
        {
            if (UserTestConfigPath == null)
            {
                throw new NotConfiguredException("Assumption Xml has not been configured.  Call ConfigureUserTestConfig() first before getting the UserTestConfigPath.");
            }
            return UserTestConfigPath;
        }

        #endregion UserTestConfig
    }
}
