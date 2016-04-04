using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Example.MsTestBase;
using Example.MsTestBase.Assumptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Example.MsTest
{
    [TestClass]
    public class AssumptionExample
    {
        #region InstallProduct_Should_ContainDescription

        [TestMethod]
        public void AssumptionExample_InstallProduct_Should_ContainDescription()
        {
            new InstallProduct_Should_ContainDescription().Test();
        }

        [Product_Install]
        private class InstallProduct_Should_ContainDescription : TestMethodClassBase
        {
            protected override void Test(IOrganizationService service)
            {
                //
                // Act
                //
                var product = AssumedEntities.Get<Product_Install, Product>();
                //
                // Assert
                //
                Assert.IsNotNull(product.Description);
            }
        }

        #endregion InstallProduct_Should_ContainDescription
    }
}
