using DLaB.Xrm.Test.Builders;
using DLaB.Xrm.Test.Settings;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Test
{
    public class TestSettings
    {
        public static PathSetting AssumptionXmlPath { get; } = new PathSetting("Assumption Xml Path has not been configured.  Call AssumptionXmlPath.Configure() first before getting the AssumptionXmlPath.");

        public static NamespaceSetting<OrganizationServiceContext> EarlyBound { get; } = new NamespaceSetting<OrganizationServiceContext>("Early Bound Assembly has not been configured.  Call EarlyBoundAssembly.Configure<>() first before getting the Assembly.");

        public static NamespaceSetting<EntityBuilder<Entity>> EntityBuilder { get; } = new NamespaceSetting<EntityBuilder<Entity>>("Entity Builder Assembly has not been configured.  Call EntityBuilder.Configure<T>() first before getting the Assembly.");

        public static PathSetting UserTestConfigPath { get; } = new PathSetting("User Test Config Path has not been configured.  Call ConfigureUserTestConfig() first before getting the UserTestConfigPath.");

        public static PathSetting WebResourcePath { get; } = new PathSetting("Web Resource Path has not been configured.  Call ConfigureWebResource() first before getting the WebResourcePath.");

        public static TestFrameworkProviderSettings TestFrameworkProvider { get; } = new TestFrameworkProviderSettings("Test Framework Provider Settings have not been configured.  Call UnitTestFrameworkProvider.Configure() first before getting the WebResourcePath.");
    }
}
