using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Sandbox.Serialization;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Test.Tests.Sandbox.Serialization
{
    [TestClass]
    public class SerializableEntityTests
    {
        [TestMethod]
        public void SerializableEntity_IsSerializable()
        {
            //
            // Arrange
            //
            var contact = new Contact
            {
                FirstName = "Test",
               // AccountRoleCodeEnum = contact_accountrolecode.Employee,
                DoNotPhone = true
            };

            //
            // Act
            //
            var entity = new SerializableEntity(contact);
            var xml = ToXml(entity);


            //
            // Assert
            //
            Assert.IsNotNull(xml);
        }

        public static string ToXml<T>(T objToXml, bool includeNameSpace = true)
        {
            string buffer;
            var xmlSerializer = new XmlSerializer(objToXml.GetType());
            using (var memStream = new MemoryStream())
            using (var stWriter = new StreamWriter(memStream))
            {
                if (includeNameSpace)
                {
                    xmlSerializer.Serialize(stWriter, objToXml);
                }
                else
                {
                    var xs = new XmlSerializerNamespaces();

                    //To remove namespace and any other inline 
                    //information tag                      
                    xs.Add("", "");
                    xmlSerializer.Serialize(stWriter, objToXml, xs);
                }
                buffer = Encoding.ASCII.GetString(memStream.GetBuffer());
            }

            return buffer;
        }
    }
}
