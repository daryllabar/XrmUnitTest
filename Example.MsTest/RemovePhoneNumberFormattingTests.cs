using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using DLaB.Xrm.Entities; // Contains Early Bound Entities
using Example.MsTestBase; // Test Base Project.  Contains code that is shared amoung all Unit Test Projects
using Example.MsTestBase.Builders; // Fluent Builder Namespace.  Builders can be used to create anything that's required, from creating an entity, to a OrganizationService, to a Plugin
using Example.Plugin; // Generic Plugin that contains the plugin to test

namespace Example.MsTest
{
    [TestClass]
    public class RemovePhoneNumberFormattingTests
    {
        #region ContactHasFormatting_Should_RemoveFormatting

        /// <summary>
        /// This is an example Test method for how to test a plugin.  It contains an Arrange section that creates the plugin, target, and context
        /// An Act section which actually executes the plugin, and an Assert section to validate the test was successful
        /// </summary>
        [TestMethod]
        public void RemovePhoneNumberFormatting_ContactHasFormatting_Should_RemoveFormatting()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var contact = new Contact { MobilePhone = "A-1-B-2-C-3" }; // Create Contact to use as target
            var plugin = new RemovePhoneNumberFormatting();
            var context = new PluginExecutionContextBuilder(). // Create Context Which is required by the service provider, which is required by the plugin
                WithRegisteredEvent(plugin.RegisteredEvents.First(e => e.EntityLogicalName == Contact.EntityLogicalName)). // Specifies the plugin event to use in the context
                WithTarget(contact).Build(); // Sets the Target
            var provider = new ServiceProviderBuilder().
                WithContext(context).Build();
            //
            // Act
            //
            plugin.Execute(provider); // Executes the Plugin

            //
            // Assert
            //

            Assert.AreEqual("123", contact.MobilePhone);
        }

        #endregion ContactHasFormatting_Should_RemoveFormatting
    }
}
