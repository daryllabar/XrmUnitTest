using IdGenerator;

namespace IdGeneratorTests
{
    [TestClass]
    public sealed class LogicTests
    {
        public TestContext TestContext { get; set; }

		private Logic _sut = null!;

		[TestInitialize]
		public void Initialize()
		{
            var testName = TestContext.TestName ?? "Unknown";
            var settings = new IdGeneratorSettings
            {
                UseClassIds = !testName.Contains("_WithStructs_"),
                UseTargetTypedNew = !testName.Contains("_WithNewName_")
            };
			_sut = new Logic(settings, new IncrementedGuidGenerator());

        }

        [TestMethod]
        public void CreateEntitiesText_Should_ProcessMultipleEntityTypes()
        {
            //
            // Arrange
            //
            var initialIds = _sut.ParseEntityTypes("Account\r\nContact 4\r\nSystemUser 2");
            var ids = _sut.GenerateOutput(initialIds.Values);
            var input = IdFieldInfo.ParseIdFields(ids);

            //
            // Act
            //
            var output = _sut.CreateEntitiesText(input.IdFieldInfos);

            //
            // Assert
            //
            Assert.AreEqual($"Account,Account{Environment.NewLine}Contact,Contacts,A,B,C,D{Environment.NewLine}SystemUser,SystemUsers,A,B", output);
        }

        [TestMethod]
        public void GenerateOutput_MixedEntities_Should_GenerateCorrectFormat()
        {
            //
            // Arrange
            //
            var ids = _sut.ParseEntityTypes("Account\r\nContact 4\r\nSystemUser 2");

            //
            // Act
            //
            var output = _sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public Id<Account> Account { get; } = new(""00000001-0000-0000-0000-000000000000"");
public ContactIds Contacts { get; } = new();
public SystemUserIds SystemUsers { get; } = new();

public class ContactIds
{
    public Id<Contact> A { get; } = new(""00000002-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""00000003-0000-0000-0000-000000000000"");
    public Id<Contact> C { get; } = new(""00000004-0000-0000-0000-000000000000"");
    public Id<Contact> D { get; } = new(""00000005-0000-0000-0000-000000000000"");
}

public class SystemUserIds
{
    public Id<SystemUser> A { get; } = new(""00000006-0000-0000-0000-000000000000"");
    public Id<SystemUser> B { get; } = new(""00000007-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void GenerateOutput_MultipleEntities_Should_GeneratePropertyAndClass()
        {
            //
            // Arrange
            //
            var ids = _sut.ParseEntityTypes("Contact 4");

            //
            // Act
            //
            var output = _sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public ContactIds Contacts { get; } = new();

public class ContactIds
{
    public Id<Contact> A { get; } = new(""00000001-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""00000002-0000-0000-0000-000000000000"");
    public Id<Contact> C { get; } = new(""00000003-0000-0000-0000-000000000000"");
    public Id<Contact> D { get; } = new(""00000004-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void GenerateOutput_SingleEntity_Should_GenerateProperty()
        {
            //
            // Arrange
            //
            var ids = _sut.ParseEntityTypes("Account");

            //
            // Act
            //
            var output = _sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            Assert.AreEqual("public Id<Account> Account { get; } = new(\"00000001-0000-0000-0000-000000000000\");", output);
        }

        [TestMethod]
        public void GenerateOutput_WithNewName_Should_UseFullType()
        {
            //
            // Arrange
            //
            var ids = _sut.ParseEntityTypes("Account");

            //
            // Act
            //
            var output = _sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            Assert.AreEqual("public Id<Account> Account { get; } = new Id<Account>(\"00000001-0000-0000-0000-000000000000\");", output);
        }

        [TestMethod]
        public void GenerateOutput_WithStructs_Should_GenerateOriginalFormat()
        {
            //
            // Arrange
            //
            var ids = _sut.ParseEntityTypes("Contact 2");

            //
            // Act
            //
            var output = _sut.GenerateOutput(ids.Values);

            //
            // Assert
            //
            var expected = @"public struct Contacts
{
    public static readonly Id<Contact> A = new(""00000001-0000-0000-0000-000000000000"");
    public static readonly Id<Contact> B = new(""00000002-0000-0000-0000-000000000000"");
}";
            Assert.AreEqual(expected, output);
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
    public Id<Contact> A { get; } = new(""00000001-0000-0000-0000-000000000000"");
    public Id<Contact> B { get; } = new(""00000002-0000-0000-0000-000000000000"");
}";

            //
            // Act
            //
            var results = IdFieldInfo.ParseIdFields(input).IdFieldInfos;

            //
            // Assert
            //
            Assert.HasCount(2, results);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("ContactIds", results[0].ContainerName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("ContactIds", results[1].ContainerName);
        }

        [TestMethod]
        public void IdFieldInfo_ParseIdFields_Should_ParseStructBasedFormat()
        {
            //
            // Arrange
            //
            var input = @"public struct Contacts
{
    public static readonly Id<Contact> A = new(""00000001-0000-0000-0000-000000000000"");
    public static readonly Id<Contact> B = new(""00000002-0000-0000-0000-000000000001"");
}";

            //
            // Act
            //
            var results = IdFieldInfo.ParseIdFields(input).IdFieldInfos;

            //
            // Assert
            //
            Assert.HasCount(2, results);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("Contacts", results[0].ContainerName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("Contacts", results[1].ContainerName);
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
            var results = IdFieldInfo.ParseIdFields(input).IdFieldInfos;

            //
            // Assert
            //
            Assert.HasCount(3, results);
            Assert.AreEqual("A", results[0].FieldName);
            Assert.AreEqual("Contact", results[0].IdType);
            Assert.AreEqual("ContactIds", results[0].ContainerName);
            Assert.AreEqual("B", results[1].FieldName);
            Assert.AreEqual("Contact", results[1].IdType);
            Assert.AreEqual("ContactIds", results[1].ContainerName);
            Assert.AreEqual("Account", results[2].FieldName);
            Assert.AreEqual("Account", results[2].IdType);
            Assert.IsNull(results[2].ContainerName);
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
            Assert.AreEqual("Contacts", info.ContainerName);
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
            Assert.AreEqual("Employees", info.ContainerName);
            Assert.HasCount(2, info.Names);
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
            Assert.AreEqual("Employees", info.ContainerName);
            Assert.HasCount(3, info.Names);
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
            Assert.AreEqual("Employees", info.ContainerName);
			Assert.HasCount(2, info.Names);
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
            Assert.AreEqual("Employees", info.ContainerName);
            Assert.HasCount(4, info.Names);
            Assert.AreEqual("A,B,C,D", string.Join(",", info.Names));
        }
    }
}
