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
    }
}
