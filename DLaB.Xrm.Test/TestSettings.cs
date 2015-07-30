using System.Reflection;
using DLaB.Xrm.Test.Exceptions;
using System;


namespace DLaB.Xrm.Test
{
    public class TestSettings
    {
        #region AssumptionXml

        private static string _assumptionXmlPath;
        public static string AssumptionXmlPath
        {
            get
            {
                if (_assumptionXmlPath == null)
                {
                    throw new NotConfiguredException("Assumption Xml has not been configured.  Call ConfigureAssumptionXml() first before getting the AssumptionXmlPath.");
                }

                return _assumptionXmlPath;
            }
            private set { _assumptionXmlPath = value; }
        }

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

        #endregion AssumptionXml

        #region CrmContextConfig

        private static Assembly _earlyBoundAssembly;
        public static Assembly EarlyBoundAssembly
        {
            get
            {
                if (_earlyBoundAssembly == null)
                {
                    throw new NotConfiguredException("Early Bound Assembly has not been configured.  Call ConfigureEarlyBoundAssembly() first before getting the EarlyBoundAssembly.");
                }
                return _earlyBoundAssembly;
            }
            private set { _earlyBoundAssembly = value; }
        }

        private static String _earlyBoundNamespace;
        public static String EarlyBoundNamespace
        {
            get
            {
                if (_earlyBoundNamespace == null)
                {
                    throw new NotConfiguredException("Early Bound Namespace has not been configured.  Call ConfigureEarlyBoundAssembly() first before getting the EarlyBoundNamespace.");
                }
                return _earlyBoundNamespace;
            }
            private set { _earlyBoundNamespace = value; }
        }

        public static bool IsEarlyBoundAssemblyConfigured { get; set; }

        /// <summary>
        /// Configures the Early Bound Assembly.
        /// </summary>
        public static void ConfigureEarlyBoundAssembly<T>() where T : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
        {
            var contextType = typeof(T);
            if (contextType.Name == "OrganizationServiceContext")
            {
                throw new Exception("Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }

            EarlyBoundAssembly = contextType.Assembly;
            EarlyBoundNamespace = contextType.Namespace;
            IsEarlyBoundAssemblyConfigured = true;
        }

        #endregion CrmContextConfig

        #region UserTestConfig

        private static string _userTestConfigPath;
        public static string UserTestConfigPath
        {
            get
            {
                if (_userTestConfigPath == null)
                {
                    throw new NotConfiguredException("User Test Config Path has not been configured.  Call ConfigureUserTestConfig() first before getting the UserTestConfigPath.");
                }
                return _userTestConfigPath;    
            }
            private set { _userTestConfigPath = value; }
        }

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

        #endregion UserTestConfig

        #region WebResource

        private static string _webResourcePath;
        public static string WebResourcePath
        {
            get
            {
                if (_webResourcePath == null)
                {
                    throw new NotConfiguredException("Web Resource Path has not been configured.  Call ConfigureWebResource() first before getting the WebResourcePath.");
                }
                return _webResourcePath;
            }
            private set
            {
                _webResourcePath = value;
            }
        }

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

        #endregion WebResource
    }
}
