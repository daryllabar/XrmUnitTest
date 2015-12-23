using System;
using System.Linq;
using System.Reflection;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class EntityDependencyTests
    {

        #region Add_MultipleDependencies_Should_CreateValidCreationOrder

        [TestMethod]
        public void EntityDependency_Add_MultipleDependencies_Should_CreateValidCreationOrder()
        {
            new Add_MultipleDependencies_Should_CreateValidCreationOrder().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class Add_MultipleDependencies_Should_CreateValidCreationOrder : TestMethodClassBase
        {


            protected override void InitializeTestData(IOrganizationService service)
            {
                // No Data Required
            }

            protected override void Test(IOrganizationService service)
            {
                var constructorInfo = typeof(EntityDependency).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
                var a = Account.EntityLogicalName;
                var b = Contact.EntityLogicalName;
                var c = Lead.EntityLogicalName;
                TestAdditionOrder(constructorInfo, a, b, c);
                TestAdditionOrder(constructorInfo, a, c, b);
                TestAdditionOrder(constructorInfo, b, a, c);
                TestAdditionOrder(constructorInfo, b, c, a);
                TestAdditionOrder(constructorInfo, c, a, b);
                TestAdditionOrder(constructorInfo, c, b, a);
            }

            private static void TestAdditionOrder(ConstructorInfo constructorInfo, string first, string second, string third)
            {
                //
                // Arrange
                //
                var mapper = (EntityDependency) constructorInfo.Invoke(null);

                //
                // Act
                //
                mapper.Add(first);
                mapper.Add(second);
                mapper.Add(third);

                //
                // Assert
                //
                var order = mapper.EntityCreationOrder.ToArray();
                Assert.AreEqual(Account.EntityLogicalName, order[0], "Account Should have been first.");
                Assert.AreEqual(Contact.EntityLogicalName, order[1], "Contact should have been second.");
                Assert.AreEqual(Lead.EntityLogicalName, order[2], "Lead should have been third.");
            }
        }

        #endregion Add_MultipleDependencies_Should_CreateValidCreationOrder
    }
}
