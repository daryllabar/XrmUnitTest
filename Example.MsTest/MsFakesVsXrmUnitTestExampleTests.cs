using System;
using DLaB.Xrm;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using Example.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Example.MsTest
{
    /// <summary>
    /// This serves to demonstrate the differences between MS Fakes or similar Faking Frameworks, and XrmUnitTest
    /// </summary>
    [TestClass]
    public class MsFakesVsXrmUnitTestExampleTests
    {
        // MsFakes Isn't suppored by Appveyor.  To test:
        // 1. Right click on the Microsoft.Xrm.Sdk.dll in the references, and select "Add Fakes Assembly"
        // 2. Uncomment this test.
        /// <summary>
        /// Example test for running a unit test using MS Fakes
        /// </summary>
        //[TestMethod]
        //public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald_MsFakes()
        //{
        //    //
        //    // Arrange
        //    //
        //    Entity updatedEntity = null;
        //    IOrganizationService service = new Microsoft.Xrm.Sdk.Fakes.StubIOrganizationService()
        //    {
        //        ExecuteOrganizationRequest = request =>
        //        {
        //            if (request is RetrieveMultipleRequest)
        //            {
        //                var response = new RetrieveMultipleResponse
        //                {
        //                    Results =
        //                    {
        //                        ["EntityCollection"] = new EntityCollection()
        //                    }
        //                };
        //    
        //                response.EntityCollection.Entities.Add(
        //                    new Contact
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        LastName = "Mcdonald"
        //                    });
        //                return response;
        //            }
        //    
        //            var updateRequest = request as UpdateRequest;
        //            if (updateRequest == null)
        //            {
        //                throw new NotImplementedException("Unrecognized Request");
        //            }
        //            updatedEntity = updateRequest.Target;
        //            return new UpdateResponse();
        //        },
        //    };
        //    
        //    // 
        //    // Act
        //    // 
        //    RenameLogic.MakeNameMatchCase(service, "McDonald");
        //    
        //    //
        //    // Assert
        //    // 
        //    Assert.IsNotNull(updatedEntity);
        //    Assert.AreEqual("McDonald", updatedEntity.ToEntity<Contact>().LastName);
        //}

        /// <summary>
        /// Example test for running a unit test only in memory, as compared to the MS Fakes
        /// </summary>
        [TestMethod]
        public void MakeNameMatchCase_NameIsMcdonald_Should_UpdateToMcDonald()
        {
            //
            // Arrange
            //
            var service = LocalCrmDatabaseOrganizationService.CreateOrganizationService<CrmContext>();
            var id = service.Create(new Contact { LastName = "Mcdonald" });

            // 
            // Act
            // 
            RenameLogic.MakeNameMatchCase(service, "McDonald");

            //
            // Assert
            // 
            Assert.AreEqual("McDonald", service.GetEntity<Contact>(id).LastName);
        }
    }
}
