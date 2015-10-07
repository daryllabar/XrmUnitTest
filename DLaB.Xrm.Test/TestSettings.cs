using DLaB.Xrm.Test.Builders;
using DLaB.Xrm.Test.Settings;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Test
{
    public class TestSettings
    {
        /// <summary>
        /// Assumption Xml allows an easy way to define assuming particular Entities exist when hitting a real CRM instance, and to download and utilize the same entities when using a local Crm 
        /// </summary>
        /// <value>
        /// The assumption XML path.
        /// </value>
        public static PathSetting AssumptionXmlPath { get; } = new PathSetting("Assumption Xml Path has not been configured.  Call AssumptionXmlPath.Configure() first before getting the AssumptionXmlPath.");

        /// <summary>
        /// Defines the namespace of the EarlyBound entities so entities can be discovered to model the Local CRM
        /// </summary>
        /// <value>
        /// The early bound.
        /// </value>
        public static NamespaceSetting<OrganizationServiceContext> EarlyBound { get; } = new NamespaceSetting<OrganizationServiceContext>("Early Bound Assembly has not been configured.  Call EarlyBoundAssembly.Configure<>() first before getting the Assembly.");

        /// <summary>
        /// Defines the namespace of Entity Builders.  This can then be used to determine what builder to use for a particular entity
        /// </summary>
        /// <value>
        /// The entity builder.
        /// </value>
        public static NamespaceSetting<EntityBuilder<Entity>> EntityBuilder { get; } = new NamespaceSetting<EntityBuilder<Entity>>("Entity Builder Assembly has not been configured.  Call EntityBuilder.Configure<T>() first before getting the Assembly.");

        /// <summary>
        /// The User Test Config Path specifies a config path to be loaded into the current Unit Test's app.Config at runtime.  This allows multiple Unit Testing projects to all use the same config settings.
        /// </summary>
        /// <value>
        /// The user test configuration path.
        /// </value>
        public static PathSetting UserTestConfigPath { get; } = new PathSetting("User Test Config Path has not been configured.  Call ConfigureUserTestConfig() first before getting the UserTestConfigPath.");

        /// <summary>
        /// Gets the web resource path.  This allows unit tests that pull a settings file, to read the value from the WebResource, rather than having to Fake it out.
        /// </summary>
        /// <value>
        /// The web resource path.
        /// </value>
        public static PathSetting WebResourcePath { get; } = new PathSetting("Web Resource Path has not been configured.  Call ConfigureWebResource() first before getting the WebResourcePath.");

        /// <summary>
        /// Gets the test framework provider which implments ITestFrameworkProvider
        /// </summary>
        /// <value>
        /// The test framework provider.
        /// </value>
        public static TestFrameworkProviderSettings TestFrameworkProvider { get; } = new TestFrameworkProviderSettings("Test Framework Provider Settings have not been configured.  Call UnitTestFrameworkProvider.Configure() first before getting the WebResourcePath.");
    }
}
