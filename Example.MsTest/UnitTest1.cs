using System;
using System.Linq;
using System.Reflection;
using DLaB.Xrm;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Test;
using Example.MsTestBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Example.MsTestBase.Builders;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk.Messages;

namespace Example.MsTest
{
    /// <summary>
    /// Defines various ways to peform the same test, using MsFakes, FakeXrmEasy, XrmUnitTest local, XrmUnitTest 
    /// </summary>
    [TestClass]
    public class UnitTest1
    {

        #region MakeNameMatchCase

        /// <summary>
        /// Example test for running a unit test using MS Fakes
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald_MsFakes()
        {
            //
            // Arrange
            //
            Entity updatedEntity = null;
            IOrganizationService service = new Microsoft.Xrm.Sdk.Fakes.StubIOrganizationService()
            {
                ExecuteOrganizationRequest = request =>
                {
                    if (request is RetrieveMultipleRequest)
                    {
                        var response = new RetrieveMultipleResponse
                        {
                            Results =
                            {
                                ["EntityCollection"] = new EntityCollection()
                            }
                        };

                        response.EntityCollection.Entities.Add(
                            new Contact
                            {
                                Id = Guid.NewGuid(),
                                LastName = "Mcdonald"
                            });
                        return response;
                    }

                    var updateRequest = request as UpdateRequest;
                    if (updateRequest != null)
                    {
                        updatedEntity = updateRequest.Target;
                        return new UpdateResponse();
                    }

                    throw new NotImplementedException("Unrecognized Request");
                },
            };

            // 
            // Act
            // 
            MakeNameMatchCase(service, "McDonald");

            //
            // Assert
            // 
            Assert.IsNotNull(updatedEntity);
            Assert.AreEqual("McDonald", updatedEntity.ToEntity<Contact>().LastName);
        }

        /// <summary>
        /// Example test for running a unit test using FakeXrmEasy
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald_FakeXrmEasy()
        {
            //
            // Arrange
            //
            var service = new XrmFakedContext { ProxyTypesAssembly = Assembly.GetAssembly(typeof(CrmContext)) }.GetFakedOrganizationService();
            var id = service.Create(new Contact { LastName = "Mcdonald", FirstName = "Ron" });

            // 
            // Act
            // 
            MakeNameMatchCase(service, "McDonald");

            //
            // Assert
            // 
            Assert.AreEqual("McDonald", service.GetEntity<Contact>(id).LastName);
        }

        /// <summary>
        /// Example test for running a unit test using XrmUnitTest's Local CrmDatabase OrganizationService
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald()
        {
            //
            // Arrange
            //
            var service = new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>());
            var id = service.Create(new Contact {LastName = "Mcdonald"});

            // 
            // Act
            // 
            MakeNameMatchCase(service, "McDonald");

            //
            // Assert
            // 
            Assert.AreEqual("McDonald", service.GetEntity<Contact>(id).LastName);
        }

        /// <summary>
        /// Updates the First or Last Name of the contact to match the given case
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="name">The name.</param>
        public static void MakeNameMatchCase(IOrganizationService service, string name)
        {
            using (var context = new CrmContext(service))
            {
                var contacts = (from c in context.ContactSet
                                where c.FirstName == name || c.LastName == name
                                //where c.FirstName == name || c.FirstName == name // Potential Type-o.  Wouldn't fail when using MS Fakes
                                select new Contact {Id = c.Id, FirstName = c.FirstName, LastName = c.LastName}).ToList();

                foreach (var contact in contacts.Where(c => StringsAreEqualButCaseIsNot(c.FirstName, name)))
                {
                    contact.FirstName = name;
                    context.UpdateObject(contact);
                }

                foreach (var contact in contacts.Where(c => StringsAreEqualButCaseIsNot(c.LastName, name)))
                {
                    contact.LastName = name;
                    context.UpdateObject(contact);
                }

                context.SaveChanges();
            }
        }

        private static bool StringsAreEqualButCaseIsNot(string a, string b)
        {
            return a != b && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Example test for running a unit test either in memory, or against CRM
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald_Server()
        {
            //
            // Arrange
            //
            TestInitializer.InitializeTestSettings();
            var service = TestBase.GetOrganizationService();
            var id = service.Create(new Contact { LastName = "Mcdonald" });
            try
            {
                // 
                // Act
                // 
                MakeNameMatchCase(service, "McDonald");

                //
                // Assert
                // 
                Assert.AreEqual("McDonald", service.GetEntity<Contact>(id).LastName);
            }
            finally
            {
                service.Delete(Contact.EntityLogicalName, id);
            }
        }

        #region MakeNameMatchCase_NameisMcdonald_Should_UpdateToMcDonald_TestMethodClass

        /// <summary>
        /// Example test to show how to run a Unit Test in memory, or against CRM, using TestMethodClassBase
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameisMcdonalod_Should_UpdateToMcDonald_TestMethodClass()
        {
            new MakeNameMatchCase_NameisMcdonald_Should_UpdateToMcDonald().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class MakeNameMatchCase_NameisMcdonald_Should_UpdateToMcDonald : TestMethodClassBase // Test Method Class Base Handles Setup and Cleanup via Ids Struct
        {
            // Any Ids listed in this struct, will get cleaned up both Pre (to handle any old data already existing) and Post Test
            private struct Ids
            {
                public static readonly Id<Contact> Contact = new Id<Contact>("F9E88EB1-2675-47D2-8A0F-FD3A84D6C6C8");
            }

            /// <summary>
            /// Initializes the entities that need to be created before the test executes
            /// </summary>
            protected override void InitializeTestData(IOrganizationService service)
            {
                // Set the last name to mcdonald, before it gets created
                Ids.Contact.Entity.LastName = "mcdonald";

                // The Crm Environment Builder Handles Creating all Entities.  
                // It can also associate entities together and determine which order entities should be created in
                new CrmEnvironmentBuilder().WithEntities<Ids>().Create(service);
            }

            /// <summary>
            /// The actual test to perform.  The IOrganization Service passed in is either a local CRM service, or a real connection,
            /// depending on the UnitTestSettings's UseLocalCrm App Setting.
            /// </summary>
            /// <param name="service"></param>
            protected override void Test(IOrganizationService service)
            {
                // 
                // Act
                // 
                MakeNameMatchCase(service, "McDonald");

                //
                // Assert
                // 
                Assert.AreEqual("McDonald", service.GetEntity(Ids.Contact).LastName);
            }
        }

        #endregion MakeNameMatchCase_NameisMcdonald_Should_UpdateToMcDonald_TestMethodClass

        #endregion MakeNameMatchCase
    }
}