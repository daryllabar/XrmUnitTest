#nullable enable
using System;
using System.Collections.Generic;
using DLaB.Xrm.Entities;
using DLaB.Xrm.LocalCrm.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmUnitTest.Test;
#if NET
using DataverseUnitTest;
#endif

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class QueryRecorderTests : BaseTestClass
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IOrganizationService _service;
        private QueryRecorder _sut;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void Initialize()
        {
            TestInitializer.InitializeTestSettings();
            _service = GetService();
            _sut = new QueryRecorder();
        }

        #region RecordRetrieve

        [TestMethod]
        public void RecordRetrieve_Should_CloneAndRecordEntity()
        {
            var contact = new Contact
            {
                FirstName = "A",
                Id = Guid.NewGuid(),
                LastName = "B"
            };

            RecordRetrieve(contact);

            contact.FirstName = "C";

            Assert.AreEqual(1, _sut.Entities.Count);
            var recorded = GetRecordedEntity(contact);
            Assert.AreEqual(contact.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreNotEqual(contact.FirstName, recorded.FirstName);
        }

        [TestMethod]
        public void RecordRetrieve_Should_HandleMultipleEntities()
        {
            var contactA = new Contact
            {
                FirstName = "A",
                Id = Guid.NewGuid(),
            };
            var contactB = new Contact
            {
                FirstName = "B",
                Id = Guid.NewGuid(),
            };

            RecordRetrieve(contactA);
            RecordRetrieve(contactB);

            contactA.FirstName = "Updated";
            contactA.LastName = "New";
            RecordRetrieve(contactA);

            contactB.FirstName = "Updated2";
            contactB.LastName = "New2";
            RecordRetrieve(contactB);

            Assert.AreEqual(2, _sut.Entities.Count);
            var recorded = GetRecordedEntity(contactA);
            Assert.AreEqual(contactA.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual("A", recorded.FirstName);
            Assert.AreEqual(contactA.LastName, recorded.LastName);
            recorded = GetRecordedEntity(contactB);
            Assert.AreEqual(contactB.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual("B", recorded.FirstName);
            Assert.AreEqual(contactB.LastName, recorded.LastName);
        }

        [TestMethod]
        public void RecordRetrieve_WithExisting_Should_MergeFirstInWins()
        {
            var contact = new Contact
            {
                FirstName = "A",
                Id = Guid.NewGuid(),
                LastName = "B"
            };

            RecordRetrieve(contact);
            contact.MiddleName = "M";
            contact.LastName = "C";
            RecordRetrieve(contact);

            Assert.AreEqual(1, _sut.Entities.Count);
            var recorded = GetRecordedEntity(contact);
            Assert.AreEqual("A", recorded.FirstName);
            Assert.AreEqual("M", recorded.MiddleName);
            Assert.AreEqual("B", recorded.LastName);
        }

        private void RecordRetrieve(Entity entity)
        {
            _sut.RecordRetrieve(_service, entity.LogicalName, entity.Id, new ColumnSet(), entity);
        }

        #endregion RecordRetrieve

        #region RecordRetrieveMultiple

        // Ensure From entity contains Linked In values
        // Ensure From entity contains Where Values

        [TestMethod]
        public void RecordRetrieveMultiple_Should_AddLinkedEntities()
        {
            //
            // Arrange
            //
            var account = CreateId(new Account
            {
                Name = "Account A"
            });
            var contact = CreateId(new Contact
            {
                FirstName = "A",
                ParentCustomerId = account
            });
            var qe = QueryExpressionFactory.Create<Contact>();
            qe.AddLink<Account>(Contact.Fields.ParentCustomerId, Account.Fields.AccountId, a => new { a.Name });

            //
            // Act
            //
            RecordMultiple(qe.Query);

            //
            // Assert
            //
            Assert.AreEqual(2, _sut.Entities.Count);
            var recorded = GetRecordedEntity<Contact>(contact);
            Assert.AreEqual(contact.EntityReference, recorded.ToEntityReference());
            Assert.AreEqual("A", recorded.FirstName);
        }

        [TestMethod]
        public void RecordRetrieveMultiple_Should_AddWhereAttributes()
        {
            //
            // Arrange
            //
            var contactA = CreateId(new Contact
            {
                FirstName = "A"
            });
            var contactB = CreateId(new Contact
            {
                FirstName = "B"
            });
            var qe = QueryExpressionFactory.CreateIn<Contact>(Contact.Fields.FirstName, "A", "B");

            //
            // Act
            //
            RecordMultiple(qe.Query,
                new Contact { Id = contactA },
                new Contact { Id = contactB });

            //
            // Assert
            //
            Assert.AreEqual(2, _sut.Entities.Count);
            var recorded = GetRecordedEntity<Contact>(contactA);
            Assert.AreEqual("A", recorded.FirstName);
            recorded = GetRecordedEntity<Contact>(contactB);
            Assert.AreEqual("B", recorded.FirstName);
        }

        [TestMethod]
        public void RecordRetrieveMultiple_Should_CloneAndRecordEntities()
        {
            //
            // Arrange
            //
            var contactA = new Contact
            {
                FirstName = "A",
                Id = Guid.NewGuid(),
            };
            var contactB = new Contact
            {
                FirstName = "B",
                Id = Guid.NewGuid(),
            };

            //
            // Act
            //
            RecordMultiple(contactA, contactB);
            contactA.FirstName = "Updated";
            contactB.FirstName = "Updated";

            //
            // Assert
            //
            Assert.AreEqual(2, _sut.Entities.Count);
            var recorded = GetRecordedEntity(contactA); 
            Assert.AreEqual(contactA.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual("A", recorded.FirstName);
            recorded = GetRecordedEntity(contactB);
            Assert.AreEqual(contactB.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual("B", recorded.FirstName);
        }

        [TestMethod]
        public void RecordRetrieveMultiple_Should_RecordAliasedEntities()
        {
            //
            // Arrange
            //
            var accountA = CreateId(new Account {
                Name = "Account A"
            });
            var accountB = CreateId(new Account {
                Name = "Account B"
            });
            var contactA = CreateId(new Contact
            {
                FirstName = "A"
            });

            var contact1 = new Contact
            {
                FirstName = "1",
                Id = Guid.NewGuid(),
            };
            contact1.AddAliasedEntity(accountA);
            contact1.AddAliasedEntity(contactA);
            var contact2 = new Contact
            {
                FirstName = "2",
                Id = Guid.NewGuid()
            };
            contact2.AddAliasedEntity(accountB);
            contact2.AddAliasedEntity(contactA);

            //
            // Act
            //
            RecordMultiple(contact1, contact2);

            //
            // Assert
            //
            Assert.AreEqual(5, _sut.Entities.Count);
            var recorded = GetRecordedEntity(contact1);
            Assert.AreEqual(contact1.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual(contact1.FirstName, recorded.FirstName);
            recorded = GetRecordedEntity(contact2);
            Assert.AreEqual(contact2.ToEntityReference(), recorded.ToEntityReference());
            Assert.AreEqual("2", recorded.FirstName);
            recorded = GetRecordedEntity<Contact>(contactA);
            Assert.AreEqual(contactA.EntityReference, recorded.ToEntityReference());
            Assert.AreEqual(contactA.Entity.FirstName, recorded.FirstName);
            var recordedAccount = GetRecordedEntity<Account>(accountA);
            Assert.AreEqual(accountA.EntityReference, recordedAccount.ToEntityReference());
            Assert.AreEqual(accountA.Entity.Name, recordedAccount.Name);
            recordedAccount = GetRecordedEntity<Account>(accountB);
            Assert.AreEqual(accountB.EntityReference, recordedAccount.ToEntityReference());
            Assert.AreEqual(accountB.Entity.Name, recordedAccount.Name);
        }

        private Id<T> CreateId<T>(T entity) where T : Entity
        {
            entity.Id = _service.Create(entity);
            return new Id<T>(entity.Id)
            {
                Entity = entity
            };
        }

        private void RecordMultiple(params Entity[] entities)
        {
            _sut.RecordRetrieveMultiple(_service, new QueryExpression(), new EntityCollection(new List<Entity>(entities)));
        }

        private void RecordMultiple(QueryBase q, params Entity[] entities)
        {
            var results = entities.Length == 0 
                ? _service.RetrieveMultiple(q)
                : new EntityCollection(new List<Entity>(entities));
            _sut.RecordRetrieveMultiple(_service, q, results);
        }

        #endregion RecordRetrieveMultiple

        private T GetRecordedEntity<T>(T entity) where T : Entity
        {
            return _sut.Entities.TryGetValue(entity.ToEntityReference(), out var recorded) ? recorded.ToEntity<T>() : throw new KeyNotFoundException(entity.ToStringDebug());
        }
    }
}
