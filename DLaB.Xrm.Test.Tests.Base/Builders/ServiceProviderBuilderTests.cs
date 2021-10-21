using System;
using DLaB.Xrm.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class ServiceProviderBuilderTests
    {
        [TestMethod]
        public void ServiceProviderBuilder_WithContext_GivenContext_Should_ReturnClonedContext()
        {
            //
            // Arrange
            //
            var context = new FakePluginExecutionContext();
            context.SharedVariables.Add("TEST", 1);
            var provider = new ServiceProviderBuilder(null,null).
                WithContext(context).Build();

            //
            // Act
            //
            var providerContext = (IPluginExecutionContext)provider.GetService(typeof (IPluginExecutionContext));

            //
            // Assert
            //
            Assert.IsFalse(ReferenceEquals(providerContext, context), "The Context Retrieved should have been the context set");
            Assert.AreEqual(providerContext.GetFirstSharedVariable<int>("TEST"), 1);
        }

        public void ServiceProviderBuilder_WithUserSpecificServices_Should_ReturnMatchingService()
        {
            var provider = new ServiceProviderBuilder();

            var defaultService = provider.Build().GetService<IOrganizationServiceFactory>().CreateOrganizationService(null);
            Assert.IsNotNull(defaultService);
            var userId = Guid.NewGuid();
            provider.WithService(defaultService, userId);
            var userService = provider.Build().GetService<IOrganizationServiceFactory>().CreateOrganizationService(userId);
            var response = userService.GetCurrentlyExecutingUserInfo();
            Assert.AreEqual(userId, response.UserId);
        }
    }

}
