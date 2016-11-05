using DLaB.Xrm.Client;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Example.MsTestBase.Builders;
using Microsoft.Xrm.Sdk;

namespace Example.MsTestBase
{
    /// <summary>
    /// Class to Initalize all TestSettings used by the Framework
    /// </summary>
    public class TestInitializer
    {
        /// <summary>
        /// Initializes the test settings.
        /// </summary>
        public static void InitializeTestSettings()
        {
            if (!TestSettings.AssumptionXmlPath.IsConfigured)
            {
                TestSettings.AssumptionXmlPath.Configure(new PatherFinderProjectOfType(typeof(TestMethodClassBase), "Assumptions\\Entity Xml"));
            }
            if (!TestSettings.UserTestConfigPath.IsConfigured)
            {
                TestSettings.UserTestConfigPath.Configure(new PatherFinderProjectOfType(typeof(TestMethodClassBase), "UnitTestSettings.user.config"));
            }
            if (!TestSettings.EntityBuilder.IsConfigured)
            {
                TestSettings.EntityBuilder.ConfigureDerivedAssembly<EntityBuilder<Entity>>();
            }
            if (!TestSettings.EarlyBound.IsConfigured)
            {
                TestSettings.EarlyBound.ConfigureDerivedAssembly<CrmContext>();
                CrmServiceUtility.GetEarlyBoundProxyAssembly(TestSettings.EarlyBound.Assembly);
            }
            if (!TestSettings.TestFrameworkProvider.IsConfigured)
            {
                TestSettings.TestFrameworkProvider.Configure(new MsTestProvider());
            }
        }
    }
}
