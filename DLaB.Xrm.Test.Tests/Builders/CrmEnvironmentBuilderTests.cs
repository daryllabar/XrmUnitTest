using System;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests.Builders
{
    [TestClass]
    public class CrmEnvironmentBuilderTests
    {

        [TestMethod]
        public void CrmEnvironmentBuilder_WithEntities_GivenIdStruct_Should_CreateAll()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            //
            // Act
            //
            var builder = new CrmEnvironmentBuilder().WithEntities<Ids>();
            builder.Create(service);


            //
            // Assert
            //

            AssertCrm.Exists(service, Ids.Value1);
            AssertCrm.Exists(service, Ids.Value2);
            AssertCrm.Exists(service, Ids.Nested.Value1);
            AssertCrm.Exists(service, Ids.Nested.Value2);
        }

        [TestMethod]
        public void CrmEnvironmentBuilder_ExceptEntities_GivenIdStruct_Should_CreateAllExceptExcluded()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString()));

            //
            // Act
            //
            var builder = new CrmEnvironmentBuilder().
                WithEntities<Ids>().
                ExceptEntities<Ids.Nested>();
            builder.Create(service);

            //
            // Assert
            //

            AssertCrm.Exists(service, Ids.Value1);
            AssertCrm.Exists(service, Ids.Value2);
            AssertCrm.NotExists(service, Ids.Nested.Value1);
            AssertCrm.NotExists(service, Ids.Nested.Value2);
        }

        public struct Ids
        {
            public struct Nested
            {
                // ReSharper disable MemberHidesStaticFromOuterClass
                public static readonly Id Value1 = new Id<Contact>(Guid.NewGuid());
                public static readonly Id Value2 = new Id<Contact>(Guid.NewGuid());
                // ReSharper restore MemberHidesStaticFromOuterClass
            }
            public static readonly Id Value1 = new Id<Contact>(Guid.NewGuid());
            public static readonly Id Value2 = new Id<Contact>(Guid.NewGuid());
        }

    }
}
