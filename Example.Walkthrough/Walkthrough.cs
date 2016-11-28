using System.Diagnostics.CodeAnalysis;
using DLaB.Xrm.Test;
using Example.MsTest;
using Example.MsTestBase;
using Example.MsTestBase.Assumptions;
using Example.MsTestBase.Builders;
using Example.Plugin;
using Example.Plugin.Advanced;

namespace Example.Walkthrough
{
    /// <summary>
    /// Walks through different examples, starting from simple --> advanced
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedVariable")]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    internal class Walkthrough
    {

        /// <summary>
        /// Shows how to create the Xrm Context used by the plugin.
        /// </summary>
        private void TestingAPluginThatDoesNotInteractWithCrm()
        {
            // Plugin to remove all formatting of phone numbers.
            // Only interacts with, and updates, the Target Entity.
            new RemovePhoneNumberFormatting();

            // Testing method, showing how to fake the plugin context
            // There is a simple plugin and an advanced plugin.  The only difference is the advanced
            //   plugin inherits from Example.Plugin.PluginBase, which is the recommended approach,
            //   rather than from the DLaBPluginBase directly.
            new SimplePluginTests().RemovePhoneNumberFormatting_ContactHasFormatting_Should_RemoveFormatting_Advanced();
        }

        /// <summary>
        /// Compares and contrasts the difference between XrmUnitTest, and using a Faking Framework, such as MsFakes
        /// </summary>
        private void AdvantageOfXrmUnitTestVsMsFakes()
        {
            // Logic class (not a plugin) to Test
            // Updates the First or Last Name of contacts to match the given casing
            RenameLogic.MakeNameMatchCase(null, null);
            
            // Testing class for testing the logic
            var tests = new MsFakesVsXrmUnitTestExampleTests();
        }

        /// <summary>
        /// Settings required to connect to CRM.
        /// </summary>
        private void SettingUpConnectingToCrm()
        {
            TestSettings. // Test Settings class for XrmUnitTest Framework
                UserTestConfigPath. // Defines the path to a configuration file
                Configure(new PatherFinderProjectOfType( // Defines that path, by the root of the 
                                                         // project with the given type
                    typeof(TestMethodClassBase), // The type to find the project
                    "UnitTestSettings.user.config")); // The name of the config file in the root of the project

            TestSettings.UserTestConfigPath.Configure(
                new PatherFinderProjectOfType(
                    typeof(TestMethodClassBase), 
                    "UnitTestSettings.user.config")); 
        }

        /// <summary>
        /// Demonstrates a plugin that utlizes the Example.MsTestBase.UnitTestSEttings.User.config "UseLocalCrmDatabase" App Settings Key
        /// </summary>
        private void TestingAPluginLocallyOrAgainstCrm()
        {
            // Plugin that triggers when a contact's address was updated, updating the address of all accounts of which it is the primary contaact
            new SyncContactToAccount();

            // Testing method that does not guarantee agnostic environment testings.
            new LocalOrServerPluginTest().SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress_Dirty();

        }

        /// <summary>
        /// Demonstrates a plugin that utlizes the TestMethodClassBase to connect to, setup, and cleanup unit tests
        /// </summary>
        private void TestingAPluginLocallyOrAgainstCrmAgnosticly()
        {
            // Testing method that does guarantee agnostic environment testings.
            new LocalOrServerPluginTest().SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress();
        }


        /// <summary>
        /// Demonstrates using assumptions to decrease required testing data.
        /// </summary>
        private void UsingAssumptionsToDecreaseRequiredTestingData()
        {
            // Defines the assumption that an Install Product Exists
            new Product_Install();

            // Testing Method that utilizes the Assumption
            new AssumptionExampleTests().AssumptionExample_InstallProduct_Should_ContainDescription();
        }

        /// <summary>
        /// Demonstrates using an Entity Builder to decrease code required to create entities.
        /// </summary>
        private void UsingEntityBuildersForCleanerCode()
        {
            // Defines a Builder for Account
            new AccountBuilder();

            // Testing Method that utilizes the AccountBuilder
            new EntityBuilderExampleTests().EntityBuilderExample_CreateWithAccountBuilder_Should_PopulateAccountInfo();
            // Testing Method that utilizes the EnvironmentBuilder
            new EntityBuilderExampleTests().EntityBuilderExample_CreateWithEnvironmentBuilder_Should_PopulateAccountInfo();
            // Testing Method that utilizes the EnvironmentBuidler to assoicate child entity to parent
            new EntityBuilderExampleTests().EntityBuilderExample_CreateChildContact_Should_SetParentAccountOnContact();
        }

        // Entity Builders
    }
}
