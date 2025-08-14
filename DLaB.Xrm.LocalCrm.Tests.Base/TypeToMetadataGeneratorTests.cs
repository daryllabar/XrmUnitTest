using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class TypeToMetadataGeneratorTests
    {
        [TestMethod]
        public void Generate_EntityType_ReturnsProperEntityMetadata()
        {
            // Arrange
            var entityType = typeof(TestEntity);
            var logicalName = "test_entity";
            var primaryName = "test_name";
            var languageCode = 1033;

            // Act
            var result = TypeToMetadataGenerator.Generate(entityType, logicalName, primaryName, languageCode);

            // Assert
            var displayNames = result.DisplayCollectionName.LocalizedLabels.Select(l => l.Label);
            Assert.AreEqual(logicalName, displayNames.FirstOrDefault());
            Assert.AreEqual(primaryName, result.PrimaryNameAttribute);

            var mappings = new Dictionary<string, Tuple<Type,string>>
            {
                { "teststring", new Tuple<Type, string>(typeof(StringAttributeMetadata), "Test String") },
                { "testint", new Tuple<Type, string>(typeof(IntegerAttributeMetadata), "Test Int") },
                { "testdatetime", new Tuple<Type, string>(typeof(DateTimeAttributeMetadata), "Test Date Time") },
                { "testbool", new Tuple<Type, string>(typeof(BooleanAttributeMetadata), "Test Bool") },
                { "testentityreference", new Tuple<Type, string>(typeof(LookupAttributeMetadata), "Test Entity Reference") },
                { "testoptionsetvalue", new Tuple<Type, string>(typeof(PicklistAttributeMetadata), "Test Option Set Value Enum") },
                { "testmultioptionsetvalue", new Tuple<Type, string>(typeof(MultiSelectPicklistAttributeMetadata), "Test Multi Option Set Value") },
                { "testmoney", new Tuple<Type, string>(typeof(MoneyAttributeMetadata), "Test Money") },
                { "testguid", new Tuple<Type, string>(typeof(UniqueIdentifierAttributeMetadata), "Test Guid") },
                { "testbytearray", new Tuple<Type, string>(typeof(FileAttributeMetadata), "Test Byte Array") },
            };

            var attributes = result.Attributes.ToDictionary(a => a.LogicalName);
            foreach (var map in mappings)
            {
                var attribute = attributes[map.Key];
                Assert.AreEqual("testentity", attribute.EntityLogicalName);
                Assert.AreEqual(map.Value.Item1, attribute.GetType());
                Assert.AreEqual(map.Value.Item2, attribute.DisplayName.GetLocalOrDefaultText());
            }

            var optionSeMetadata = (PicklistAttributeMetadata)attributes["testoptionsetvalue"];
            var options = optionSeMetadata.OptionSet.Options;
            Assert.AreEqual(7, options.Count);
            AssertOption(LogLevel.Critical);
            AssertOption(LogLevel.Debug);
            AssertOption(LogLevel.Error);
            AssertOption(LogLevel.Information);
            AssertOption(LogLevel.None);
            AssertOption(LogLevel.Trace);
            AssertOption(LogLevel.Warning);
            return;

            void AssertOption(LogLevel value)
            {
                Assert.AreEqual(value.ToString(), options[(int)value].Label.GetLocalOrDefaultText());
                Assert.AreEqual((int)value, options[(int)value].Value);
            }
        }

        [Microsoft.Xrm.Sdk.Client.EntityLogicalName("testentity")]
        public class TestEntity : Entity
        {
            [AttributeLogicalName("teststring")]
            public string TestString { get; set; }
            [AttributeLogicalName("testint")]
            public int? TestInt { get; set; }
            [AttributeLogicalName("testdatetime")]
            public DateTime? TestDateTime { get; set; }
            [AttributeLogicalName("testbool")]
            public bool? TestBool { get; set; }
            [AttributeLogicalName("testentityreference")]
            public EntityReference TestEntityReference { get; set; }
            [AttributeLogicalName("testoptionsetvalue")]
            public OptionSetValue TestOptionSetValue { get; set; }
            [AttributeLogicalName("testoptionsetvalue")]
            public LogLevel TestOptionSetValueEnum { get; set; }
            [AttributeLogicalName("testmultioptionsetvalue")]
            public IEnumerable<OptionSetValue> TestMultiOptionSetValue { get; set; }
            [AttributeLogicalName("testmultioptionsetvalue")]
            public IEnumerable<LogLevel> TestMultiOptionSetValueEnum { get; set; }
            [AttributeLogicalName("testmoney")]
            public Money TestMoney { get; set; }
            [AttributeLogicalName("testguid")]
            public Guid? TestGuid { get; set; }
            [AttributeLogicalName("testbytearray")]
            public byte[] TestByteArray { get; set; }
        } 
    }
}
