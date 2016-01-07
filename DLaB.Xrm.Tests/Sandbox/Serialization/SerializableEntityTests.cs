using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DLaB.Common;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm;
using DLaB.Xrm.Sandbox.Serialization;
using DLaB.Xrm.Test.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XrmUnitTest.Test;

namespace DLaB.Xrm.Tests.Sandbox.Serialization
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

        [TestMethod]
        public void SerializableEntity_NamespaceMatchesXrm()
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

            var xrmXml = contact.ToSdkEntity().Serialize();
            //
            // Act
            //
            var xml = new SerializableEntity(contact).Serialize();


            // Was Generating:
            //<Entity xmlns:i="http://www.w3.org/2001/XMLSchema-instance" z:Id="1" z:Type="DLaB.Xrm.Sandbox.Serialization.SerializableEntity" z:Assembly="DLaB.Xrm, Version=1.0.5850.20711, Culture=neutral, PublicKeyToken=fbff471681eb6c24" xmlns:z="http://schemas.microsoft.com/2003/10/Serialization/" xmlns="http://schemas.microsoft.com/xrm/2011/Contracts">
            //    <Attributes xmlns:d2p1="http://schemas.microsoft.com/xrm/2011/Contracts1" z:Id="2" xmlns:d2p2="http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization" z:Size="2">
            //        <d2p1:KeyValuePairOfstringanyType z:Id="3">
            //            <d2p2:key z:Id="4">firstname</d2p2:key>
            //            <d2p2:value z:Id="5" z:Type="System.String" z:Assembly="0">Test</d2p2:value>
            //        </d2p1:KeyValuePairOfstringanyType>
            //        <d2p1:KeyValuePairOfstringanyType z:Id="6">
            //            <d2p2:key z:Id="7">donotphone</d2p2:key>
            //            <d2p2:value z:Id="8" z:Type="System.Boolean" z:Assembly="0">true</d2p2:value>
            //        </d2p1:KeyValuePairOfstringanyType>
            //    </Attributes>
            //    <EntityState i:nil="true" />
            //    <FormattedValues z:Id="9" xmlns:d2p1="http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization" z:Size="0" />
            //    <Id>00000000-0000-0000-0000-000000000000</Id>
            //    <KeyAttributes xmlns:d2p1="http://schemas.microsoft.com/xrm/7.1/Contracts" z:Id="10" xmlns:d2p2="http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization" z:Size="0" />
            //    <LogicalName z:Id="11">contact</LogicalName>
            //    <RelatedEntities z:Id="12" xmlns:d2p1="http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization" z:Size="0" />
            //    <RowVersion i:nil="true" />
            //</Entity>

            // The d2p2="http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization" wasn't right 

            //
            // Assert
            //
            var genericNamespace = @"xmlns:d2p2=""http://schemas.datacontract.org/2004/07/System.Collections.Generic";
            var sandboxNamespace = @"=""http://schemas.datacontract.org/2004/07/DLaB.Xrm.Sandbox.Serialization""";
            Assert.IsTrue(xrmXml.Contains(genericNamespace));
            Assert.IsTrue(xml.Contains(genericNamespace));
            Assert.IsFalse(xml.Contains(sandboxNamespace));

        }


    }
}
