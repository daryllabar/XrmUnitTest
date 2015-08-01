using DLaB.Xrm.Test.Builders;
using DLaB.Xrm.Test.Settings;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Test
{
    public class TestSettings
    {
        private static readonly PathSetting AssumptionXml = new PathSetting("Assumption Xml Path has not been configured.  Call AssumptionXmlPath.Configure() first before getting the AssumptionXmlPath.");
        private static readonly NamespaceSetting<OrganizationServiceContext> EarlyBoundAssembly = new NamespaceSetting<OrganizationServiceContext>("Early Bound Assembly has not been configured.  Call EarlyBoundAssembly.Configure<>() first before getting the Assembly.");
        private static readonly NamespaceSetting<EntityBuilder<Entity>> Builder = new NamespaceSetting<EntityBuilder<Entity>>("Entity Builder Assembly has not been configured.  Call EntityBuilder.Configure<T>() first before getting the Assembly.");
        private static readonly PathSetting UserTestConfig = new PathSetting("User Test Config Path has not been configured.  Call ConfigureUserTestConfig() first before getting the UserTestConfigPath.");
        private static readonly PathSetting WebResource = new PathSetting("Web Resource Path has not been configured.  Call ConfigureWebResource() first before getting the WebResourcePath.");

        public static PathSetting AssumptionXmlPath { get { return AssumptionXml; } }
        public static NamespaceSetting<OrganizationServiceContext> EarlyBound { get { return EarlyBoundAssembly; } }
        public static NamespaceSetting<EntityBuilder<Entity>> EntityBuilder { get { return Builder; } }
        public static PathSetting UserTestConfigPath { get { return UserTestConfig; } }
        public static PathSetting WebResourcePath { get { return WebResource; } }
    }
}
