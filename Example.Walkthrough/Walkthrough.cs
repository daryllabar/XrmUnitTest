using System.Diagnostics.CodeAnalysis;
using Example.MsTest;
using Example.Plugin;
using Example.Plugin.Advanced;

namespace Example.Walkthrough
{
    /// <summary>
    /// Walks through different examples, starting from simple --> advanced
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedVariable")]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    internal class Walkthrough
    {
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
        /// Demonstrates a plugin that utlizes the Example.MsTestBase.UnitTestSEttings.User.config "UseLocalCrmDatabase" App Settings Key
        /// </summary>
        private void TestingAPluginLocallyOrAgainstCRM()
        {
            // Plugin that triggers when a contact's address was updated, updating the address of all accounts of which it is the primary contaact
            new SyncContactToAccount();

            // Testing method that does not guarantee agnostic environment testings.
            new LocalOrServerPluginTest().SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress_Dirty();

            // Testing method that does does guarantee agnostic environment testings.
            new LocalOrServerPluginTest().SyncContactToAccount_UpdateContactAddress_Should_UpdateAccountAddress();
        }
    }
}
