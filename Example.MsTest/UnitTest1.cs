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
    [TestClass]
    public class UnitTest1
    {

        #region MakeNameMatchCase

        /// <summary>
        /// Example test for running a unit test only in memory
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
        /// Example test for running a unit test only in memory
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald_Fake()
        {
            //
            // Arrange
            //
            var service = new XrmFakedContext { ProxyTypesAssembly = Assembly.GetAssembly(typeof(CrmContext)) }.GetFakedOrganizationService();
            var id = service.Create(new Contact { LastName = "Mcdonald" });

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
        /// Example test for running a unit test only in memory
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

        #endregion MakeNameMatchCase

        #region CreateAccount_Should_RetrieveByName

        /// <summary>
        /// Example test for being able to run a Unit Test in memory, or against CRM.
        /// </summary>
        [TestMethod]
        public void CreateAccount_Should_RetrieveByName()
        {
            new CreateAccount_Should_PopulateAccountNumber().Test();
        }

        // ReSharper disable once InconsistentNaming
        private class CreateAccount_Should_PopulateAccountNumber : TestMethodClassBase // Test Method Class Base Handles Setup and Cleanup
        {
            private struct Ids 
            {
                public static readonly Id<Account> Account = new Id<Account>("64387F79-5B4A-42F0-BA41-E348A1183379");
            }

            /// <summary>
            /// Initializes the entities that need to be created before the test executes
            /// </summary>
            /// <param name="service">The service.</param>
            protected override void InitializeTestData(IOrganizationService service)
            {
                Ids.Account.Entity.Name = "John Doe";

                // The Crm Environment Builder Handles Creating all Entities.  
                // It can also associate entities together and determine which order entities should be created in
                new CrmEnvironmentBuilder().WithEntities(Ids.Account).Create(service);
            }

            /// <summary>
            /// The actual test to perform.  The IOrganization Service passed in is either a local CRM service, or a real connection
            /// Depending on the UnitTestSettings's UseLocalCrm App Setting
            /// </summary>
            /// <param name="service">The service.</param>
            protected override void Test(IOrganizationService service)
            {
                var myEntity = new Entity();
                service = new OrganizationServiceBuilder(service).WithFakeRetrieveMultiple(
                    (s, q) =>
                    {
                        var col = new EntityCollection();
                        col.Entities.Add(myEntity);
                        return col;
                    }).Build();
                using (var context = new CrmContext(service))
                {
                    var account = context.AccountSet.FirstOrDefault(c => c.Name == Ids.Account.Entity.Name);
                    Assert.IsNotNull(account);
                    Assert.IsNotNull(account.AccountNumber, "Account Number should have been populated by Builder");
                    account = context.AccountSet.FirstOrDefault(c => c.Name == "Jane Doe");
                    Assert.IsNull(account, "Jane Doe was not added and the query should have returned null");
                }
            }
        }

        #endregion CreateAccount_Should_RetrieveByName
    }
}