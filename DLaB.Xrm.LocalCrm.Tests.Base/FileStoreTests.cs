using System;
using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NET
using OrganizationServiceBuilder = DataverseUnitTest.Builders.OrganizationServiceBuilder;
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using OrganizationServiceBuilder = DLaB.Xrm.Test.Builders.OrganizationServiceBuilder;
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
#endif

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class FileStoreTests : BaseTestClass
    {
        [TestMethod]
        public void LocalCrmTests_Crud_ActivityPartyConstraints()
        {
            var service = GetService();
            var contact = new Contact();
            contact.Id = service.Create(contact);
            var data = GenerateLargeByteArray(); // Generate array of bytes bigger than 4 MB

            service.UploadFile(contact.ToEntityReference(), Contact.Fields.Id, "Test", data);
            var result = service.DownloadFile(contact.ToEntityReference(), Contact.Fields.Id);

            Assert.IsTrue(data.SequenceEqual(result), "The uploaded file should match the downloaded file.");
        }

        private byte[] GenerateLargeByteArray()
        {
            // Generate array of bytes bigger than 4 MB
            var sizeInBytes = (int) (4 * 1024 * 1024 * 2.5); // 4 MB * 2.5
            var data = new byte[sizeInBytes];
            // Fill the array with some data
            for (int i = 0; i < sizeInBytes; i++)
            {
                data[i] = (byte)(i % 256);
            }
            return data;
        }

    }
}
