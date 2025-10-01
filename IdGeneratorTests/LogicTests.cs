using IdGenerator;

namespace IdGeneratorTests
{
    [TestClass]
    public sealed class LogicTests
    {
		private Logic _sut = null!;

		[TestInitialize]
		public void Initialize()
		{
			_sut = new Logic(new IdGeneratorSettings(), new IncrementedGuidGenerator());

        }

        [TestMethod]
        [DataRow(3, "A,B,C", DisplayName = "Three Names")]
        [DataRow(2, "A,B", DisplayName = "Two Names")]
        public void ParseEntityTypes_WithDuplicateTypes_Should_Combine(int names, string expected)
        {
            //
            // Act
            //
            var (entityType, info) = _sut.ParseEntityTypes(string.Join(" ", new int[names].Select(_ => "Contact"))).Single();

            //s
            // Assert
            //
            Assert.AreEqual("Contact", entityType);
            Assert.AreEqual("Contacts", info.StructName);
            Assert.AreEqual(names, info.Names.Count);
            Assert.AreEqual(expected, string.Join(",", info.Names));
        }

        [TestMethod]
        public void ParseEntityTypes_WithNamesAndEqualCount_Should_NotAddMore()
        {
            //
            // Act
            //
            var (entityType, info) = _sut.ParseEntityTypes("Contact,Employees,One,Two 2").Single();

            //s
            // Assert
            //
            Assert.AreEqual("Contact", entityType);
            Assert.AreEqual("Employees", info.StructName);
            Assert.AreEqual(2, info.Names.Count);
            Assert.AreEqual("One,Two", string.Join(",", info.Names));
        }

        [TestMethod]
        public void ParseEntityTypes_WithNamesAndHigherCount_Should_NotAddMore()
        {
            //
            // Act
            //
            var (entityType, info) = _sut.ParseEntityTypes("Contact,Employees,One,Two 3").Single();

            //s
            // Assert
            //
            Assert.AreEqual("Contact", entityType);
            Assert.AreEqual("Employees", info.StructName);
            Assert.AreEqual(3, info.Names.Count);
            Assert.AreEqual("A,One,Two", string.Join(",", info.Names));
        }

        [TestMethod]
        public void ParseEntityTypes_WithStructNameAndCount_Should_KeepStructNameAndGenerateIdNames()
        {
			//
			// Act
			//
			var (entityType, info) = _sut.ParseEntityTypes("Contact,Employees 2").Single();

			//s
			// Assert
			//
			Assert.AreEqual("Contact", entityType);
            Assert.AreEqual("Employees", info.StructName);
			Assert.AreEqual(2, info.Names.Count);
            Assert.AreEqual("A,B", string.Join(",", info.Names));
        }

		[TestMethod]
        public void ParseEntityTypes_WithStructNameAndCountAndOtherCount_Should_AddToCount()
        {
            //
            // Act
            //
            var (entityType, info) = _sut.ParseEntityTypes("Contact,Employees 2 Contact 2").Single();

            //s
            // Assert
            //
            Assert.AreEqual("Contact", entityType);
            Assert.AreEqual("Employees", info.StructName);
            Assert.AreEqual(4, info.Names.Count);
            Assert.AreEqual("A,B,C,D", string.Join(",", info.Names));
        }

        [TestMethod]
        public void GenerateOutput_WithClassIds_SingleEntity_Should_GenerateProperty()
        {
            //
            // Arrange
            //
            var settings = new IdGeneratorSettings { UseClassIds = true, UseTargetTypedNew = true };
            var sut = new Logic(settings, new IncrementedGuidGenerator());
            var ids = sut.ParseEntityTypes("Account");

            //
            // Act
            //
            var output = sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            Assert.AreEqual("public Id<Account> Account { get; } = new(\"00000000-0000-0000-0000-000000000000\");", output);
        }

        [TestMethod]
        public void GenerateOutput_WithClassIds_MultipleEntities_Should_GeneratePropertyAndClass()
        {
            //
            // Arrange
            //
            var settings = new IdGeneratorSettings { UseClassIds = true, UseTargetTypedNew = true };
            var sut = new Logic(settings, new IncrementedGuidGenerator());
            var ids = sut.ParseEntityTypes("Contact 4");

            //
            // Act
            //
            var output = sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public ContactIds Contacts { get; } = new();
public class ContactIds
{
    public Id<Contact> A { get; } = new(""00000000-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""01000000-0000-0000-0000-000000000000"");
    public Id<Contact> C { get; } = new(""02000000-0000-0000-0000-000000000000"");
    public Id<Contact> D { get; } = new(""03000000-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void GenerateOutput_WithClassIds_MixedEntities_Should_GenerateCorrectFormat()
        {
            //
            // Arrange
            //
            var settings = new IdGeneratorSettings { UseClassIds = true, UseTargetTypedNew = true };
            var sut = new Logic(settings, new IncrementedGuidGenerator());
            var ids = sut.ParseEntityTypes("Account\r\nContact 4\r\nSystemUser 2");

            //
            // Act
            //
            var output = sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public Id<Account> Account { get; } = new(""00000000-0000-0000-0000-000000000000"");
public ContactIds Contacts { get; } = new();
public class ContactIds
{
    public Id<Contact> A { get; } = new(""01000000-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""02000000-0000-0000-0000-000000000000"");
    public Id<Contact> C { get; } = new(""03000000-0000-0000-0000-000000000000"");
    public Id<Contact> D { get; } = new(""04000000-0000-0000-0000-000000000000"");
}
public SystemUserIds SystemUsers { get; } = new();
public class SystemUserIds
{
    public Id<SystemUser> A { get; } = new(""05000000-0000-0000-0000-000000000000"");
    public Id<SystemUser> B { get; } = new(""06000000-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void GenerateOutput_WithStructIds_Should_GenerateOriginalFormat()
        {
            //
            // Arrange
            //
            var settings = new IdGeneratorSettings { UseClassIds = false, UseTargetTypedNew = true };
            var sut = new Logic(settings, new IncrementedGuidGenerator());
            var ids = sut.ParseEntityTypes("Contact 2");

            //
            // Act
            //
            var output = sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public struct Contacts
{
    public static readonly Id<Contact> A = new(""00000000-0000-0000-0000-000000000000"");
    public static readonly Id<Contact> B = new(""01000000-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void GenerateOutput_WithClassIdsAndFullTypedNew_Should_UseFullType()
        {
            //
            // Arrange
            //
            var settings = new IdGeneratorSettings { UseClassIds = true, UseTargetTypedNew = false };
            var sut = new Logic(settings, new IncrementedGuidGenerator());
            var ids = sut.ParseEntityTypes("Account");

            //
            // Act
            //
            var output = sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            Assert.AreEqual("public Id<Account> Account { get; } = new Id<Account>(\"00000000-0000-0000-0000-000000000000\");", output);
        }

        [TestMethod]
        public void IdFieldInfo_ParseIdFields_Should_ParseClassBasedFormat()
        {
            //
            // Arrange
            //
            var input = @"public ContactIds Contacts { get; } = new();
public class ContactIds
{
    public Id<Contact> A { get; } = new(""00000000-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""01000000-0000-0000-0000-000000000000"");
}";

            //
            // Act
            //
            var results = IdFieldInfo.ParseIdFields(input);

            //
            // Assert
            //
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("ContactIds", results[0].StructName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("ContactIds", results[1].StructName);
        }

        [TestMethod]
        public void IdFieldInfo_ParseIdFields_Should_ParseStructBasedFormat()
        {
            //
            // Arrange
            //
            var input = @"public struct Contacts
{
    public static readonly Id<Contact> A = new(""00000000-0000-0000-0000-000000000000"");
    public static readonly Id<Contact> B = new(""01000000-0000-0000-0000-000000000000"");
}";

            //
            // Act
            //
            var results = IdFieldInfo.ParseIdFields(input);

            //
            // Assert
            //
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("Contacts", results[0].StructName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("Contacts", results[1].StructName);
        }

        [TestMethod]
        public void IdFieldInfo_ParseIdFields_Should_ParseMixedFormats()
        {
            //
            // Arrange
            //
            var input = @"public Id<Account> Account { get; } = new(""00000000-0000-0000-0000-000000000000"");
public ContactIds Contacts { get; } = new();
public class ContactIds
{
    public Id<Contact> A { get; } = new(""01000000-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""02000000-0000-0000-0000-000000000000"");
}";

            //
            // Act
            //
            var results = IdFieldInfo.ParseIdFields(input);

            //
            // Assert
            //
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("ContactIds", results[0].StructName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("ContactIds", results[1].StructName);
            Assert.AreEqual("Account", results[2].FieldName);
            Assert.AreEqual("Account", results[2].IdType);
            Assert.IsNull(results[2].StructName);
        }
    }
}
