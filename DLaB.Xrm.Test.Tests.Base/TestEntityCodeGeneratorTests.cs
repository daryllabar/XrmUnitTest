#nullable enable
using System;
using System.Collections.Generic;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using XrmUnitTest.Test;
#if NET
using DataverseUnitTest;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class TestEntityCodeGeneratorTests : BaseTestClass
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IOrganizationService _service;
        private TestEntityCodeGenerator _sut;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.InitializeTestSettings();
            _service = GetService();
            _sut = new TestEntityCodeGenerator(typeof(Contact).Assembly, typeof(Contact).Namespace);
        }

        [TestMethod]
        public void GenerateCode_Should_SkipUndefinedTypes()
        {
            var now = DateTime.UtcNow.Date;
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                ParentCustomerId = new EntityReference("nonexistent", Guid.NewGuid()),
            };
            var entities = new Dictionary<EntityReference, Entity> { { contact.ToEntityReference(), contact } };

            var lines = _sut.GenerateCode(entities).Split([Environment.NewLine], StringSplitOptions.None);

            var expected = @$"var contact1 = new Contact {{
	ContactId = new Guid(""{contact.Id}""),
	ParentCustomerId = null /* Unable to find EarlyBound Type for nonexistent.  Skipping Generation! */,
}};
";
            AssertEqual(expected, lines);
        }

        [TestMethod]
        public void GenerateCode_Should_SkipAliasedValues()
        {
            var now = DateTime.UtcNow.Date;
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
            };
            contact.AddAliasedValue("contact", "firstname", "A");

            var entities = new Dictionary<EntityReference, Entity> { { contact.ToEntityReference(), contact } };

            var lines = _sut.GenerateCode(entities).Split([Environment.NewLine], StringSplitOptions.None);

            var expected = @$"var contact1 = new Contact {{
	ContactId = new Guid(""{contact.Id}""),
}};
";
            AssertEqual(expected, lines);
        }
        [TestMethod]
        public void GenerateCode_Should_SortAndCreateAllTypes()
        {
            var now = DateTime.UtcNow.Date;
            var contact = new Contact
            {
                AssistantName = "Run \"Fast\"",
                AnnualIncome = new Money(10),
                Id = Guid.NewGuid(),
                ParentCustomerId = new EntityReference(Account.EntityLogicalName, Guid.NewGuid()),
                ParticipatesInWorkflow = true,
                PreferredAppointmentDayCodeEnum = Contact_PreferredAppointmentDayCode.Friday,
                StateCode = ContactState.Inactive,
                BirthDate = now,
                EntityImage = [1, 2, 3],
                Address1_City = null,
                NumberOfChildren = 4
            };
            var contact2 = contact.Clone();
            contact2.Id = Guid.NewGuid();
            var entities = new Dictionary<EntityReference, Entity> {
                { contact.ToEntityReference(), contact },
                { contact2.ToEntityReference(), contact2 }
                };

            var lines = _sut.GenerateCode(entities).Split([Environment.NewLine], StringSplitOptions.None);

            var expected = $@"var contact1 = new Contact {{
	AnnualIncome = new Money(10),
	AssistantName = @""Run """"Fast"""""",
	BirthDate = new DateTime({now.Year}, {now.Month}, {now.Day}, {now.Hour}, {now.Minute}, {now.Second}),
	ContactId = new Guid(""{contact.Id}""),
	EntityImage = new byte[]{{ 1, 2, 3 }},
	NumberOfChildren = 4,
	ParentCustomerId = new EntityReference(Account.EntityLogicalName, new Guid(""{contact.ParentCustomerId.Id}"")),
	ParticipatesInWorkflow = true,
	PreferredAppointmentDayCodeEnum = Contact_PreferredAppointmentDayCode.Friday,
	StateCode = ContactState.Inactive,
}};
var contact2 = new Contact {{
	AnnualIncome = new Money(10),
	AssistantName = @""Run """"Fast"""""",
	BirthDate = new DateTime({now.Year}, {now.Month}, {now.Day}, {now.Hour}, {now.Minute}, {now.Second}),
	ContactId = new Guid(""{contact2.Id}""),
	EntityImage = new byte[]{{ 1, 2, 3 }},
	NumberOfChildren = 4,
	ParentCustomerId = new EntityReference(Account.EntityLogicalName, new Guid(""{contact2.ParentCustomerId.Id}"")),
	ParticipatesInWorkflow = true,
	PreferredAppointmentDayCodeEnum = Contact_PreferredAppointmentDayCode.Friday,
	StateCode = ContactState.Inactive,
}};
";
            AssertEqual(expected, lines);
        }

        private static void AssertEqual(string expected, string[] lines)
        {
            var expectedLines = expected.Split([@"
"], StringSplitOptions.None);
            Assert.AreEqual(expectedLines.Length, lines.Length, "Line Count Mismatch");
            for (var i=0; i<lines.Length; i++){
                Assert.AreEqual(expectedLines[i], lines[i], "Mismatch at line " + i);
            }
        }
    }
}
