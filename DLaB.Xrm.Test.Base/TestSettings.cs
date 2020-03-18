using DLaB.Xrm.Test.Builders;
using DLaB.Xrm.Test.Settings;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Defines the TestSettings for the Test framework
    /// </summary>
    public class TestSettings
    {
        /// <summary>
        /// Assumption Xml allows an easy way to define assuming particular Entities exist when hitting a real CRM instance, and to download and utilize the same entities when using a local Crm 
        /// </summary>
        /// <value>
        /// The assumption XML path.
        /// </value>
        public static PathSetting AssumptionXmlPath { get; } = new PathSetting("Assumption Xml Path has not been configured.  Call DLaB.Xrm.Test.TestSettings.AssumptionXmlPath.Configure() first before getting the AssumptionXmlPath.");

        /// <summary>
        /// Defines the namespace of the EarlyBound entities so entities can be discovered to model the Local CRM
        /// </summary>
        /// <value>
        /// The early bound.
        /// </value>
        public static NamespaceSetting<OrganizationServiceContext> EarlyBound { get; } = new NamespaceSetting<OrganizationServiceContext>("Early Bound Assembly has not been configured.  Call DLaB.Xrm.Test.TestSettings.EarlyBoundAssembly.ConfigureDerivedAssembly<>() first before getting the Assembly.");

        /// <summary>
        /// Defines the namespace of Entity Builders.  This can then be used to determine what builder to use for a particular entity
        /// </summary>
        /// <value>
        /// The entity builder.
        /// </value>
        public static NamespaceSetting<EntityBuilder<Entity>> EntityBuilder { get; } = new NamespaceSetting<EntityBuilder<Entity>>("Entity Builder Assembly has not been configured.  Call DLaB.Xrm.Test.TestSettings.EntityBuilder.ConfigureDerivedAssembly<T>() first before getting the Assembly.");

        /// <summary>
        /// The User Test Config Path specifies a config path to be loaded into the current Unit Test's app.Config at runtime.  This allows multiple Unit Testing projects to all use the same config settings.
        /// </summary>
        /// <value>
        /// The user test configuration path.
        /// </value>
        public static PathSetting UserTestConfigPath { get; } = new PathSetting("User Test Config Path has not been configured.  Call DLaB.Xrm.Test.TestSettings.UserTestConfig.Configure first before getting the UserTestConfigPath.");

        /// <summary>
        /// Gets the web resource path.  This allows unit tests that pull a settings file, to read the value from the WebResource, rather than having to Fake it out.
        /// </summary>
        /// <value>
        /// The web resource path.
        /// </value>
        public static PathSetting WebResourcePath { get; } = new PathSetting("Web Resource Path has not been configured.  Call DLaB.Xrm.Test.TestSettings.WebResource.Configure() first before getting the WebResourcePath.");

        /// <summary>
        /// Gets the source control provider which implements ISourceControlProvider
        /// </summary>
        /// <value>
        /// The test framework provider.
        /// </value>
        public static SourceControlProviderSetting SourceControlProvider { get; } = new SourceControlProviderSetting("Source Control Provider Settings has not been configured.  Call DLaB.Xrm.Test.TestSettings.SourceControlProviderSetting.Configure() first before getting the SourceControlProvider.");

        /// <summary>
        /// Gets the test framework provider which implements ITestFrameworkProvider
        /// </summary>
        /// <value>
        /// The test framework provider.
        /// </value>
        public static TestFrameworkProviderSettings TestFrameworkProvider { get; } = new TestFrameworkProviderSettings("Test Framework Provider Settings have not been configured.  Call DLaB.Xrm.Test.TestSettings.UnitTestFrameworkProvider.Configure() first before getting the TestFrameworkProvider.");
    }
}
