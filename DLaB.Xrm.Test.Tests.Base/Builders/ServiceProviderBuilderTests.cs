using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

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
            Assert.IsFalse(Object.ReferenceEquals(providerContext, context), "The Context Retrieved should have been the context set");
            Assert.AreEqual(providerContext.GetFirstSharedVariable<int>("TEST"), 1);
        }
    }

}
