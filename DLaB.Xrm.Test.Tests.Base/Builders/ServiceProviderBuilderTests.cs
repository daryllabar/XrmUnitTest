using System;
using DLaB.Xrm.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
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

        [TestMethod]
        public void ServiceProviderBuilder_WithUserSpecificServices_Should_ReturnMatchingService()
        {
            TestInitializer.InitializeTestSettings();
            var provider = new ServiceProviderBuilder();

            var defaultService = provider.Build().GetService<IOrganizationServiceFactory>().CreateOrganizationService(null);
            Assert.IsNotNull(defaultService);
            var userId = Guid.NewGuid();
            provider.WithService(defaultService, userId);
            var userService = provider.Build().GetService<IOrganizationServiceFactory>().CreateOrganizationService(userId);
            var response = userService.GetCurrentlyExecutingUserInfo();
            Assert.AreEqual(userId, response.UserId);
        }

        [TestMethod]
        public void ServiceProviderBuilder_CallToGetUndefinedService_Should_ReturnNull()
        {
            var provider = new ServiceProviderBuilder().Build();
            Assert.IsNull(provider.GetService(typeof(ServiceProviderBuilderTests)));
        }

        [TestMethod]
        public void ServiceProviderBuilder_CallToGetServiceShould_UtilizeDefault()
        {
            var defaultProvider = new FakeServiceProvider();
            defaultProvider.AddService("1");
            var provider = new ServiceProviderBuilder()
                .WithDefaultProvider(defaultProvider);
            Assert.AreEqual("1", provider.Build().GetService<string>());

            provider.WithService("2");
            Assert.AreEqual("2", provider.Build().GetService<string>());
        }

        [TestMethod]
        public void ServiceProviderBuilder_BuildInternal_Should_OverrideDefault()
        {
            var defaultProvider = new FakeServiceProvider();
            var builder = new TestServiceBuilder
            {
                Provider = defaultProvider
            };

            var provider = builder.Build();
            Assert.IsTrue(ReferenceEquals(defaultProvider, provider));
            Assert.IsTrue(!ReferenceEquals(builder.BuiltProvider, provider));
        }


        public class TestServiceBuilder: ServiceProviderBuilderBase<TestServiceBuilder>
        {
            protected override TestServiceBuilder This => this;

            public IServiceProvider Provider { get; set;}
            public IServiceProvider BuiltProvider { get; set; }


            protected override IServiceProvider BuildInternal(IServiceProvider provider)
            {
                return Provider;
            }
        }
    }
}
