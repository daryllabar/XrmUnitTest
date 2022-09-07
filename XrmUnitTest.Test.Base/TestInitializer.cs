using DLaB.Xrm.Client;
using DLaB.Xrm.Entities;
using XrmUnitTest.Test.Builders;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace XrmUnitTest.Test
{
    /// <summary>
    /// Class to Initialize all TestSettings used by the Framework
    /// </summary>
    public class TestInitializer
    {
        /// <summary>
        /// Configures all XrmUnitTest Settings
        /// </summary>
        public static void InitializeTestSettings()
        {
#if NET
            if (!TestSettings.AssumptionJsonPath.IsConfigured)
            {
                TestSettings.AssumptionJsonPath.Configure(new PatherFinderProjectOfType(typeof(TestMethodClassBase), "Assumptions\\Entity Json"));
            }
            //if (!TestSettings.UserTestConfigPath.IsConfigured)
            //{
            //    TestSettings.UserTestConfigPath.Configure(new PathFinderEnvironmentFolder("Microsoft\\UserSecrets\\DataverseUnitTest.Default\\secrets.json"));
            //}
#else
            if (!TestSettings.AssumptionXmlPath.IsConfigured)
            {
                TestSettings.AssumptionXmlPath.Configure(new PatherFinderProjectOfType(typeof(TestMethodClassBase), "Assumptions\\Entity Xml"));
            }
#endif
            if (!TestSettings.UserTestConfigPath.IsConfigured)
            {
                TestSettings.UserTestConfigPath.Configure(new PatherFinderProjectOfType(typeof(TestMethodClassBase), "UnitTestSettings.user.config"));
            }
            if (!TestSettings.EntityBuilder.IsConfigured)
            {
                TestSettings.EntityBuilder.ConfigureDerivedAssembly<LeadBuilder>();
            }
            if (!TestSettings.EarlyBound.IsConfigured)
            {
                TestSettings.EarlyBound.ConfigureDerivedAssembly<CrmContext>();
                CrmServiceUtility.GetEarlyBoundProxyAssembly(TestSettings.EarlyBound.Assembly);
            }
            if (!TestSettings.SourceControlProvider.IsConfigured)
            {
                TestSettings.SourceControlProvider.ConfigureNone();
            }
            if (!TestSettings.TestFrameworkProvider.IsConfigured)
            {
                TestSettings.TestFrameworkProvider.Configure(new MsTestProvider());
            }
        }
    }
}
