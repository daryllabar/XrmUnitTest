﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DLaB.Common;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
#if NET
using DataverseUnitTest.Entities;
using DLaB.Xrm;

namespace DataverseUnitTest
#else
using DLaB.Xrm.Test.Entities;

namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Extension Class for Xrm Tests
    /// </summary>
    public static class Extensions
    {
        #region struct

        /// <summary>
        /// Returns the Guid Fields declared by the given Struct Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Guid> GetIdFields<T>() where T : struct
        {
            return from f in typeof(T).GetFields()
                   where f.FieldType == typeof(Guid)
                   select (Guid)f.GetValue(null);
        }

        #endregion struct

        #region Action<IOrganizationService>

        /// <summary>
        /// Executes the given action, returning a list of Query Expressions that would have been executed against the server.
        /// If the action does something with the results of the query expression, it most likely will fail. i.e. retrieves an entity,
        /// then uses that entity's id to update another entity
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static List<QueryExpression> GetExecutedQueryExpressions(this Action<IOrganizationService> action)
        {
            var expressions = new List<QueryExpression>();
            var mock = new FakeIOrganizationService(new StubOrganizationService())
            {
                AssociateAction = (s, name, id, rel, entities) => { },
                CreateFunc = (s, entity) => Guid.Empty,
                DeleteAction = (s, name, id) => { },
                DisassociateAction = (s, name, id, rel, entities) => { },
                ExecuteFunc = (s, r) => new OrganizationResponse(),
                RetrieveFunc = (s, name, id, cs) =>
                {
                    var type = EntityHelper.GetType(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, name);
                    expressions.Add(new QueryExpression(name).WhereEqual(EntityHelper.GetIdAttributeName(type), id));
                    return new Entity(name);
                },
                RetrieveMultipleFunc = (s, qb) =>
                {
                    if (qb is QueryExpression qe)
                    {
                        expressions.Add(qe);
                    }
                    return new EntityCollection();
                },
                UpdateAction = (s, e) => { },
            };

            action(mock);

            return expressions;
        }

        #endregion Action<IOrganizationService>

        #region Dictionary<,Queue<>>

        /// <summary>
        /// Looks up the queue for the given key, enqueuing the value if the queue is found, or creating a new queue and enqueuing
        /// the value to that queue.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key value to lookup and enqeue the value to the queue of</param>
        /// <param name="value">Value to enqueue</param>
        public static void AddOrEnqueue<TKey, TValue>(this Dictionary<TKey, Queue<TValue>> dict, TKey key, TValue value)
        {
            if (dict.TryGetValue(key, out Queue<TValue> values))
            {
                values.Enqueue(value);
            }
            else
            {
                values = new Queue<TValue>();
                values.Enqueue(value);
                dict.Add(key, values);
            }
        }

        /// <summary>
        /// Looks up the queue for the given key, enqueuing the values if the queue is found, or creating a new queue and enqueuing
        /// the values to that queue.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key value to lookup and enqeue the values to the queue of</param>
        /// <param name="values">Values to enqueue</param>
        public static void AddOrEnqueue<TKey, TValue>(this Dictionary<TKey, Queue<TValue>> dict, TKey key, params TValue[] values)
        {
            if (dict.TryGetValue(key, out Queue<TValue> value))
            {
                value.EnqueueRange(value);
            }
            else
            {
                value = new Queue<TValue>();
                value.EnqueueRange(values);
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// Looks up the queue for the given key, enqueuing the values if the queue is found, or creating a new queue and enqueuing
        /// the values to that queue.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key value to lookup and add the values to the list of</param>
        /// <param name="dictionaryList">Values to enqueue</param>
        public static void AddOrEnqueue<TKey, TValue>(this Dictionary<TKey, Queue<TValue>> dict, TKey key, Dictionary<TKey, List<TValue>> dictionaryList)
        {
            if (!dictionaryList.TryGetValue(key, out List<TValue> listValues))
            {
                // Didn't find any an associated list in the dictionary, nothing to Enqueue
                return;
            }

            dict.AddOrEnqueue(key, listValues.ToArray());
        }

        #endregion Dicitonary<,List<>>

        #region Dictionary<String, List<Guid>>

        /// <summary>
        /// Looks up the list for the given key, adding the value if the list is found, or creating a new list and adding
        /// the value to that list if the list is not found.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="ids"></param>
        public static void AddOrAppend(this Dictionary<String, List<Guid>> dict, params Id[] ids)
        {
            foreach (var id in ids)
            {
                if (dict.TryGetValue(id, out List<Guid> values))
                {
                    values.Add(id);
                }
                else
                {
                    values = new List<Guid> { id };
                    dict.Add(id, values);
                }
            }
        }

        /// <summary>
        /// Used the Entity Logical Name of the Entity Type to lookup the list in the dictionary, and 
        /// adds the given values to the list, 
        /// or 
        /// creates a new list and adds the values to that list if the list isn't found
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dict"></param>
        /// <param name="guids">Values to add to the list</param>
        public static void AddOrAppend<TEntity>(this Dictionary<String, List<Guid>> dict, params Guid[] guids) where TEntity : Entity
        {
            dict.AddOrAppend(EntityHelper.GetEntityLogicalName<TEntity>(), guids);
        }

        /// <summary>
        /// Used the Entity Logical Name of the Entity Type to lookup the list in the dictionary, and 
        /// adds all static properties of the given struct to the list, 
        /// or 
        /// creates a new list and adds the values to that list if the list isn't found
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="dict"></param>
        public static void AddOrAppend<TEntity, TStruct>(this Dictionary<String, List<Guid>> dict)
            where TEntity : Entity
            where TStruct : struct
        {
            dict.AddOrAppend(EntityHelper.GetEntityLogicalName<TEntity>(), GetIdFields<TStruct>().ToArray());
        }

        #endregion Dictionary<String, Guid>

        #region Dictionary<String, Queue<Guid>>

        /// <summary>
        /// Looks up the queue for the given key, enqueuing the values if the queue is found, or creating a new queue and enqueuing
        /// the values to that queue.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="ids">The Ids to Enqueue</param>
        public static void AddOrEnqueue(this Dictionary<string, Queue<Guid>> dict, params Id[] ids)
        {
            foreach (var id in ids)
            {
                if (dict.TryGetValue(id, out Queue<Guid> value))
                {
                    value.Enqueue(id);
                }
                else
                {
                    value = new Queue<Guid>();
                    value.Enqueue(id);
                    dict.Add(id, value);
                }
            }
        }

        /// <summary>
        /// Looks up the queue given the EntityLogical name of the TEntity, enqueuing the values if the queue is found, or creating a new queue and enqueuing
        /// the values to that queue.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dict"></param>
        /// <param name="dictionaryList">Values to enqueue</param>
        public static void AddOrEnqueue<TEntity>(this Dictionary<String, Queue<Guid>> dict, Dictionary<String, List<Guid>> dictionaryList) where TEntity : Entity
        {
            dict.AddOrEnqueue(EntityHelper.GetEntityLogicalName<TEntity>(), dictionaryList);
        }

        #endregion Dictionary<String, Queue<Guid>>

        #region Entity

        /// <summary>
        /// Reloads the attributes from CRM
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static T Reload<T>(this T entity, IOrganizationService service) where T : Entity
        {
            foreach (var aliased in entity.Attributes.Where(a => a.Key.Contains('.') || a.Value is AliasedValue).ToList())
            {
                entity.Attributes.Remove(aliased.Key);
            }

            var columns = new ColumnSet(false);
            columns.AddColumns(entity.Attributes.Keys.ToArray());
            var value = service.GetEntity<T>(entity.Id, columns);

            entity.Attributes = value.Attributes;

            return entity;
        }

        /// <summary>
        /// Assigns the OptionSetProperty if the attribute name of the property is not already in the 
        /// Attributes collection, and the enumValue is not null if set to Null is false
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyExpression">In the form of e =&gt; e.OptionSetProperty</param>
        /// <param name="enumValue"></param>
        /// <param name="setToNull"></param>
        public static void SetValueIfNull<TEntity, TEnum>(this TEntity entity, Expression<Func<TEntity, OptionSetValue>> propertyExpression,
            TEnum? enumValue, bool setToNull = false)
            where TEntity : Entity
            where TEnum : struct
        {
            if (enumValue == null && !setToNull)
            {
                return;
            }

            var name = propertyExpression.GetLowerCasePropertyName();

            if (!entity.Attributes.ContainsKey(name))
            {
                if (enumValue == null)
                {
                    entity[name] = null;
                }
                else
                {
                    entity[name] = new OptionSetValue((int)(object)enumValue.Value);
                }
            }
        }

        /// <summary>
        /// Assigns the EntityReference if the attribute name of the property is not already in the 
        /// Attributes collection, and the EntityReference is not null and the entity reference id is not empty
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyExpression">In the form of e =&gt; e.OptionSetProperty</param>
        /// <param name="entityReference"></param>
        public static void SetValueIfNull<T>(this T entity, Expression<Func<T, EntityReference>> propertyExpression,
            EntityReference entityReference)
            where T : Entity
        {
            if (entityReference == null || entityReference.Id == Guid.Empty)
            {
                return;
            }

            var name = propertyExpression.GetLowerCasePropertyName();

            if (!entity.Attributes.ContainsKey(name))
            {
                entity[name] = entityReference;
            }
        }

        /// <summary>
        /// Assigns the OptionSetProperty if the attribute name of the property is not already in the 
        /// Attributes collection, and the enumValue is not null
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyExpression">In the form of e =&gt; e.OptionSetProperty</param>
        /// <param name="value"></param>
        public static void SetValueIfNull<T>(this T entity, Expression<Func<T, EntityReference>> propertyExpression,
            Entity value)
            where T : Entity
        {
            if (value != null)
            {
                entity.SetValueIfNull(propertyExpression, value.ToEntityReference());
            }
        }

        /// <summary>
        /// Assigns the Attribute if the attribute name of the property is not already in the 
        /// Attributes collection, and the value passed in is not null if set to null is false
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyExpression">In the form of e =&gt; e.OptionSetProperty</param>
        /// <param name="value"></param>
        /// <param name="setToNull"></param>
        public static void SetValueIfNull<T, TAttribute>(this T entity, Expression<Func<T, TAttribute>> propertyExpression,
            TAttribute value, bool setToNull = false)
            where T : Entity
        {
            if (value == null && !setToNull)
            {
                return;
            }

            var name = propertyExpression.GetLowerCasePropertyName();

            if (!entity.Attributes.ContainsKey(name))
            {
                entity[name] = value;
            }
        }

        /// <summary>
        /// Determines whether the current entity's field set contains at least the entire set of the given entity's field set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="compareEntity">The compare entity.</param>
        /// <returns></returns>
        public static bool ContainsAllFieldsInEntity<T>(this T entity, T compareEntity) where T : Entity
        {
            return compareEntity.Attributes.ToList().All(attribute => entity.Attributes.Contains(attribute.Key));
        }

        // This I beleieve was used at one time to help ensure that a user that doesn't have rights to read fields in an entity, doesn't cache some CRM data.
        //public static void AssertIsValidCacheFor<T>(this List<T> list, List<T> nonCacheVersionOfList) where T : Entity
        //{
        //    Assert.AreEqual(list.Count, list.Count, "The lists do not have the same record count");
        //    Assert.IsTrue(list.Count > 0, "The lists do not contain any data.");
        //    Assert.IsTrue(nonCacheVersionOfList.All(i => list.Select(c => c.Id).Contains(i.Id)), "The Cache does not contain one of the Entities");
        //    list.ForEach(l => Assert.IsTrue(l.ContainsAllFieldsInEntity(nonCacheVersionOfList.First(n => n.Id == l.Id)), "The cache item is missing one or more field from the non-cached list entities"));
        //}

        //public static void AssertIsValidCacheFor<T>(this T cacheEntity, T nonCacheEntity) where T : Entity
        //{
        //    Assert.IsTrue(cacheEntity != null && nonCacheEntity != null, "One or both of the entities is null");
        //    Assert.IsTrue(cacheEntity.Id == nonCacheEntity.Id, "The entities' Ids are not equal");
        //    Assert.IsTrue(cacheEntity.ContainsAllFieldsInEntity(nonCacheEntity), "The Cache entity does not contain one or more fields in the non-cache entity");
        //}

        #endregion Entity

        #region IOrganizationService

        /// <summary>
        /// Creates a link between records.
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The record to which the related records are associated.</param>
        /// <param name="relationshipLogicalName">The name of the relationship to be used to create the link.</param>
        /// <param name="ids">Microsoft.Xrm.Sdk.EntityReferenceCollection. A collection of entity references (references to records) to be associated.</param>
        public static void Associate<T1,T2>(this IOrganizationService service, Id<T1> entity, string relationshipLogicalName, params Id<T2>[] ids)
            where T1: Entity
            where T2 : Entity
        {
            service.Associate(entity, relationshipLogicalName, ids.Select(i => i.EntityReference).ToArray());
        }

        /// <summary>
        /// Creates a link between records.
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The record to which the related records are associated.</param>
        /// <param name="relationshipLogicalName">The name of the relationship to be used to create the link.</param>
        /// <param name="ids">Microsoft.Xrm.Sdk.EntityReferenceCollection. A collection of entity references (references to records) to be associated.</param>
        public static void Associate(this IOrganizationService service, Id entity, string relationshipLogicalName, params Id[] ids)
        {
            service.Associate(entity, relationshipLogicalName, ids.Select(i => i.EntityReference).ToArray());
        }

        /// <summary>
        /// Deletes the specified entity
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity to be deleted.</param>
        public static void Delete(this IOrganizationService service, Id entity)
        {
            service.Delete(entity.LogicalName, entity.EntityId);
        }

        /// <summary>
        /// Deletes the specified entity
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity to be deleted.</param>
        public static void Delete<TEntity>(this IOrganizationService service, Id<TEntity> entity) where TEntity: Entity
        {
            service.Delete(entity.LogicalName, entity.EntityId);
        }

        /// <summary>
        /// Handles deleting the business unit by first disabling it.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="businessUnitId">The business unit identifier.</param>
        public static void DeleteBusinessUnit(this IOrganizationService service, Guid businessUnitId)
        {
            var qe = QueryExpressionFactory.Create(BusinessUnit.EntityLogicalName, new ColumnSet(), BusinessUnit.Fields.BusinessUnitId, businessUnitId);
            if (service.GetFirstOrDefault(qe) != null)
            {
                service.SetState(BusinessUnit.EntityLogicalName, businessUnitId, false);
                service.Delete(BusinessUnit.EntityLogicalName, businessUnitId);
            }
        }

        /// <summary>
        /// Deletes a link between records.
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The record to which the related records are associated.</param>
        /// <param name="relationshipLogicalName">The name of the relationship to be used to delete the link.</param>
        /// <param name="ids">Microsoft.Xrm.Sdk.EntityReferenceCollection. A collection of entity references (references to records) to be disassociated.</param>
        public static void Disassociate<T1,T2>(this IOrganizationService service, Id<T1> entity, string relationshipLogicalName, params Id<T2>[] ids)
            where T1 : Entity
            where T2 : Entity
        {
            service.Disassociate(entity, entity, new Relationship(relationshipLogicalName), new EntityReferenceCollection(ids.Select(i => i.EntityReference).ToArray()));
        }

        /// <summary>
        /// Deletes a link between records.
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="entity">The record to which the related records are associated.</param>
        /// <param name="relationshipLogicalName">The name of the relationship to be used to delete the link.</param>
        /// <param name="ids">Microsoft.Xrm.Sdk.EntityReferenceCollection. A collection of entity references (references to records) to be disassociated.</param>
        public static void Disassociate(this IOrganizationService service, Id entity, string relationshipLogicalName, params Id[] ids)
        {
            service.Disassociate(entity, entity, new Relationship(relationshipLogicalName), new EntityReferenceCollection(ids.Select(i => i.EntityReference).ToArray()));
        }

        /// <summary>
        /// Retrieves the Entity for the given Id, with all columns
        /// </summary>
        /// <typeparam name="T">An early bound Entity Type</typeparam>
        /// <param name="service">open IOrganizationService</param>
        /// <param name="id">Typed Id</param>
        /// <returns></returns>
        public static T GetEntity<T>(this IOrganizationService service, Id<T> id) where T : Entity
        {
            return service.GetEntity<T>(id.EntityId);
        }

        /// <summary>
        /// Retrieves the Entity for the given Id, with the columns specified in the anonymousTypeInitializer
        /// </summary>
        /// <typeparam name="T">An early bound Entity Type</typeparam>
        /// <param name="service">open IOrganizationService</param>
        /// <param name="id">Typed Id</param>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add</param>
        /// <returns></returns>
        public static T GetEntity<T>(this IOrganizationService service, Id<T> id, Expression<Func<T, object>> anonymousTypeInitializer) where T : Entity
        {
            return service.GetEntity(id.EntityId, anonymousTypeInitializer);
        }

        /// <summary>
        /// Determines whether the giving service is a Local Crm Service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns> 
        public static bool IsLocalCrmService(this IOrganizationService service)
        {
            return service is IClientSideOrganizationService clientSide && clientSide.GetOrganizationKey().StartsWith("www.localcrmdatabase.com");
        }

        /// <summary>
        /// Returns a unique string key for the given IOrganizationService
        /// If the service is remote, the uri will be used, including the org Name
        /// If the service is on the server (i.e. plugin), the org id will be used
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetOrganizationKey(this IOrganizationService service)
        {
            var uri = service.GetServiceUri();
            if (uri != null)
            {
                return uri.Host.ToLower() + "|" + uri.Segments[1].Replace(@"/", "|").ToLower();
            }

            // Already on the server, grab organization guid
            // Service should be of type Microsoft.Crm.Extensibility.InProcessServiceProxy which is an internal class.  Use reflection to grab the private field Org Id;
            var field = service.GetType().GetField("_organizationId", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return string.Empty;
            }

            var orgId = field.GetValue(service).ToString();
            return "localOrgId" + orgId;
        }

        #region MockOrDefault

        #region Retrieve

        /// <summary>
        /// Will return the mocked entity, when the Id of the entity is searched for in a Retrieve, and all Columns in the columnset
        /// are present in the mocked entity.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="logicalName"></param>
        /// <param name="id"></param>
        /// <param name="set"></param>
        /// <param name="mockedEntity"></param>
        /// <returns></returns>
        public static Entity MockOrDefault(this IOrganizationService service, string logicalName, Guid id, ColumnSet set, Entity mockedEntity)
        {
            if (id == mockedEntity.Id && set.Columns.All(c => mockedEntity.Attributes.ContainsKey(c)))
            {
                return mockedEntity;
            }
            else
            {
                return service.Retrieve(logicalName, id, set);
            }
        }

        #endregion Retrieve

        #region RetrieveMultiple

        /// <summary>
        /// Uses the methodToMock if the call should be mocked.  If the Query Expression in the qb matches the QueryExpression generated by methodToMock,
        /// the mocked Entity will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="methodToMock">The method to mock.</param>
        /// <param name="mockedEntity">The mocked entity.</param>
        /// <returns></returns>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Action<IOrganizationService> methodToMock, Entity mockedEntity)
        {
            if (qb is QueryExpression qe && methodToMock.GetExecutedQueryExpressions().Any(qe.IsEqual))
            {
                return mockedEntity == null ? new EntityCollection() : new EntityCollection(new[] { mockedEntity });
            }

            return s.RetrieveMultiple(qb);
        }

        /// <summary>
        /// Uses useMock to determine if the call should be mocked.  If useMock returns true,
        /// the mocked Entity will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="useMock">Func to determine if the given Query Expression should be mocked.</param>
        /// <param name="mockedEntity">The mocked entity.</param>
        /// <returns></returns>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Func<QueryExpression, bool> useMock, Entity mockedEntity)
        {
            if (qb is QueryExpression qe && useMock(qe))
            {
                return mockedEntity == null ? new EntityCollection() : new EntityCollection(new[] { mockedEntity });
            }

            return s.RetrieveMultiple(qb);
        }

        /// <summary>
        /// Uses the methodtoMock to if the call should be mocked.  If the Query Expression in the qb matches the QueryExpression generated by methodToMock,
        /// the mocked entities will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="methodToMock">The method to mock.</param>
        /// <param name="mockedEntities">The mocked entities to return.</param>
        /// <returns></returns>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Action<IOrganizationService> methodToMock, IEnumerable<Entity> mockedEntities)
        {
            return s.MockOrDefault(qb, qe => methodToMock.GetExecutedQueryExpressions().Any(qe.IsEqual), (mockedEntities ?? new List<Entity>()).ToArray());
        }

        /// <summary>
        /// Uses useMock to determine if the call should be mocked.  If useMock returns true,
        /// the mocked entities will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="useMock">Func to determine if the given Query Expression should be mocked.</param>
        /// <param name="mockedEntities">The mocked entities.</param>
        /// <returns></returns>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Func<QueryExpression, bool> useMock, IEnumerable<Entity> mockedEntities)
        {
            return MockOrDefault(s, qb, useMock, (mockedEntities ?? new List<Entity>()).ToArray());
        }

        /// <summary>
        /// Uses the methodtoMock to if the call should be mocked.  If the Query Expression in the qb matches the QueryExpression generated by methodToMock,
        /// the mocked entities will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="methodToMock">The method to mock.</param>
        /// <param name="mockedEntities">The mocked entities to return.</param>
        /// <returns></returns>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Action<IOrganizationService> methodToMock, params Entity[] mockedEntities)
        {
            return s.MockOrDefault(qb, qe => methodToMock.GetExecutedQueryExpressions().Any(qe.IsEqual), mockedEntities);
        }

        /// <summary>
        /// Uses useMock to determine if the call should be mocked.  If useMock returns true,
        /// the mocked entities will be returned, else the Query will execute as normal.
        /// </summary>
        /// <param name="s">The service.</param>
        /// <param name="qb">The Query Base.</param>
        /// <param name="useMock">Func to determine if the given Query Expression should be mocked.</param>
        /// <param name="mockedEntities">The mocked entities.</param>
        public static EntityCollection MockOrDefault(this IOrganizationService s, QueryBase qb, Func<QueryExpression, bool> useMock, params Entity[] mockedEntities)
        {
            if (qb is QueryExpression qe && useMock(qe))
            {
                return new EntityCollection(mockedEntities ?? new Entity[0]);
            }
            else
            {
                return s.RetrieveMultiple(qb);
            }
        }

        #endregion RetrieveMultiple

        #endregion MockOrDefault

        #endregion IOrganizationService

        #region IServiceProvider

        /// <summary>
        /// Loads the given OrganizationRequest from the input parameters of the IPluginExecutionContext.
        /// </summary>
        /// <param name="provider"></param>
        /// <typeparam name="T">Organization Response</typeparam>
        /// <returns></returns>
        public static T GetRequest<T>(this IServiceProvider provider) where T : OrganizationRequest, new()
        {
            var request = Activator.CreateInstance<T>();
            var context = provider.GetService<IPluginExecutionContext>();
            if(context == null)
            {
                throw new ArgumentException("The IServiceProvider did not contain an IPluginExecutionContext");
            }
            request.Parameters = context.InputParameters;
            return request;
        }

        /// <summary>
        /// Loads the given OrganizationResponse from the output parameters of the IPluginExecutionContext.
        /// </summary>
        /// <param name="provider"></param>
        /// <typeparam name="T">Organization Response</typeparam>
        /// <returns></returns>
        public static T GetResponse<T>(this IServiceProvider provider) where T : OrganizationResponse, new()
        {
            var response = Activator.CreateInstance<T>();
            var context = provider.GetService<IPluginExecutionContext>();
            if (context == null)
            {
                throw new ArgumentException("The IServiceProvider did not contain an IPluginExecutionContext");
            }
            response.Results = context.OutputParameters;
            return response;
        }


        /// <summary>
        /// Retrieves the Fake Service from the Service Provider
        /// </summary>
        /// <typeparam name="TFake">The Fake Service to retrieve.  Must implement IServiceFaked&lt;&gt;></typeparam>
        /// <param name="provider">The Provider</param>
        /// <returns></returns>
        public static TFake GetFake<TFake>(this IServiceProvider provider) where TFake : IFakeService
        {
            var @interface = GetFakedInterface(typeof(TFake));

            return (TFake)provider.GetService(@interface.GetGenericArguments()[0]);
        }

        /// <summary>
        /// Gets the generic type of the first IServiceFaked interface defined in the type hierarchy
        /// </summary>
        /// <param name="T">The Type</param>
        /// <returns></returns>
        private static Type GetFakedInterface(Type T)
        {
            while (true)
            {
                var interfaces = T.GetInterfaces()
                                  .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceFaked<>))
                                  .ToList();
                switch (interfaces.Count)
                {
                    case 0:
                        throw new Exception("Must Define Type");
                    case 1:
                        return @interfaces[0];
                    default:
                        if (T.BaseType == null)
                        {
                            throw new Exception(T.FullName + " Defines Type multiple times");
                        }

                        break;
                }

                var currentInterfaces = interfaces.Except(T.BaseType.GetInterfaces()).ToList();

                switch (currentInterfaces.Count)
                {
                    case 0:
                        T = T.BaseType;
                        continue;
                    case 1:
                        return currentInterfaces[0];
                    default:
                        throw new Exception(T.FullName + " Defines Type multiple times");
                }
            }
        }

        #endregion IServiceProvider

        #region Object

        /// <summary>
        /// Shortcut for throwing a ArgumentNullException
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [DebuggerHidden]
        public static void ThrowIfNull(this Object data, string name)
        {
            if (data == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        #endregion Object

        #region Type

        public static IEnumerable<Id> GetIds(this Type type)
        {
            if (type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)))
            {
                // lambda enclosures will generate display classes.  This could be of type Id, but static, not instance, causing errors.
                yield break;
            }
            var idType = typeof(Id);
            foreach (var field in type.GetFields().Where(field => idType.IsAssignableFrom(field.FieldType)))
            {
                yield return GetValue(field);
            }
            foreach (var id in type.GetNestedTypes().SelectMany(GetIds))
            {
                yield return id;
            }
        }

        /// <summary>
        /// Gets the Id value of the field.  It is very easy to declare an Id&lt;Entity&gt;, but this isn't valid
        /// This will make the error much more readable and understandable
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        [DebuggerHidden]
        private static Id GetValue(FieldInfo field)
        {
            try
            {
                return (Id)field.GetValue(null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException?.InnerException != null)
                {
                    var idEx = ex.InnerException.InnerException;
                    if (idEx.Message.Contains("\"Entity\" is not a valid entityname"))
                    {
                        throw idEx;
                    }
                }
                throw;
            }
        }

        #endregion Type
    }
}
