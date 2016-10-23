using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using DLaB.Common;
using DLaB.Xrm.CrmSdk;
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NMemory;
using NMemory.Tables;

namespace DLaB.Xrm.LocalCrm
{
    [DebuggerNonUserCode]
    internal partial class LocalCrmDatabase : Database
    {
        private static readonly LocalCrmDatabase Default = new LocalCrmDatabase();
        private static readonly ConcurrentDictionary<string, LocalCrmDatabase> Databases = new ConcurrentDictionary<string, LocalCrmDatabase>();
        private static readonly object DatabaseCreationLock = new object();
        private readonly ConcurrentDictionary<string, ITable> _tables = new ConcurrentDictionary<string, ITable>();

        private static ITable<T> SchemaGetOrCreate<T>(LocalCrmDatabaseInfo info) where T : Entity
        {
            var db = GetDatabaseForService(info);
            var logicalName = EntityHelper.GetEntityLogicalName<T>();

            ITable table;
            if (db._tables.TryGetValue(logicalName, out table)) { return (ITable<T>)table; }
            table = db.Tables.Create<T, Guid>(e => e.Id, null);
            if (db._tables.TryAdd(logicalName, table)) { return (ITable<T>)table; }

            if (!db._tables.TryGetValue(logicalName, out table))
            {
                throw new Exception("Could Not Create Table " + EntityHelper.GetEntityLogicalName<T>());
            }
            return (ITable<T>)table;
        }

        private static LocalCrmDatabase GetDatabaseForService(LocalCrmDatabaseInfo info)
        {
            LocalCrmDatabase db;
            if (info.DatabaseName == null)
            {
                db = Default;
            }
            else
            {
                // ReSharper disable once InconsistentlySynchronizedField
                if (Databases.TryGetValue(info.DatabaseName, out db))
                {
                    return db;
                }
                lock (DatabaseCreationLock)
                {
                    if (Databases.TryGetValue(info.DatabaseName, out db))
                    {
                        return db;
                    }
                    db = new LocalCrmDatabase();
                    Databases.AddOrUpdate(info.DatabaseName, db, (s, d) => { throw new Exception("Lock Failed Creating Database!"); });
                }
            }
            return db;
        }

        #region LinkEntity Join

        private static IQueryable<T> CallJoin<T>(LocalCrmDatabaseInfo service, IQueryable<T> query, LinkEntity link) where T : Entity
        {
            try
            {
                var tFrom = typeof(T);
                var tTo = EntityHelper.GetType(tFrom.Assembly, tFrom.Namespace, link.LinkToEntityName);
                return (IQueryable<T>)typeof(LocalCrmDatabase).GetMethod("Join", BindingFlags.NonPublic | BindingFlags.Static).
                                                                 MakeGenericMethod(tFrom, tTo).Invoke(null, new object[] { service, query, link });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static IQueryable<TFrom> Join<TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TFrom> query, LinkEntity link)
            where TFrom : Entity
            where TTo : Entity
        {
            IQueryable<LinkEntityTypes<TFrom, TTo>> result;
            if (link.JoinOperator == JoinOperator.Inner)
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName) equals ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName)
                         select new LinkEntityTypes<TFrom, TTo>(f, t, link.EntityAlias);

                result = ApplyLinkFilter(result, link.LinkCriteria);
            }
            else
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on
                            new
                            {
                                Id = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName),
                                FilterConditions = true
                            }
                            equals
                            new
                            {
                                Id = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName),
                                FilterConditions = EvaluateFilter(t, link.LinkCriteria)
                            }
                            into joinResult
                         from t in joinResult.DefaultIfEmpty()
                         select new LinkEntityTypes<TFrom, TTo>(f, t, link.EntityAlias);
            }

            // Apply any Conditions on the Link Entity


            return link.LinkEntities.Aggregate(result.Select(e => AddAliasedColumns(e.Root, e.Current, e.Alias, link.Columns)),
                                               (current, childLink) => current.Intersect(CallChildJoin(info, result, childLink)));
        }

        private static IQueryable<LinkEntityTypes<TFrom, TTo>> ApplyLinkFilter<TFrom, TTo>(IQueryable<LinkEntityTypes<TFrom, TTo>> query, FilterExpression filter)
            where TFrom : Entity
            where TTo : Entity
        {
            return query.Where(l => EvaluateFilter(l.Current, filter));
        }

        private static IQueryable<TRoot> CallChildJoin<TRoot, TFrom>(LocalCrmDatabaseInfo info, IQueryable<LinkEntityTypes<TRoot, TFrom>> query, LinkEntity link)
            where TRoot : Entity
            where TFrom : Entity
        {
            try
            {
                var tRoot = typeof(TRoot);
                var tTo = EntityHelper.GetType(tRoot.Assembly, tRoot.Namespace, link.LinkToEntityName);
                return ((IQueryable<TRoot>)typeof(LocalCrmDatabase).GetMethod("ChildJoin", BindingFlags.NonPublic | BindingFlags.Static)
                                                                     .MakeGenericMethod(tRoot, typeof(TFrom), tTo)
                                                                     .Invoke(null, new object[] { info, query, link }));
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }


        // ReSharper disable once UnusedMember.Local
        private static IQueryable<TRoot> ChildJoin<TRoot, TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<LinkEntityTypes<TRoot, TFrom>> query, LinkEntity link)
            where TRoot : Entity
            where TFrom : Entity
            where TTo : Entity
        {
            IQueryable<LinkEntityTypes<TRoot, TTo>> result;
            if (link.JoinOperator == JoinOperator.Inner)
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable(f.Current, link.LinkFromAttributeName) equals
                             ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName)
                         select new LinkEntityTypes<TRoot, TTo>(f.Root, t, link.EntityAlias);
            }
            else
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable(f.Current, link.LinkFromAttributeName) equals
                             ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName) into joinResult
                         from t in joinResult.DefaultIfEmpty()
                         select new LinkEntityTypes<TRoot, TTo>(f.Root, t, link.EntityAlias);
            }

            // Apply any Conditions on the Link Entity
            result = ApplyLinkFilter(result, link.LinkCriteria);

            return link.LinkEntities.Aggregate(result.Select(e => AddAliasedColumns(e.Root, e.Current, e.Alias, link.Columns)),
                                               (current, childLink) => current.Intersect(CallChildJoin(info, result, childLink)));
        }

        internal class LinkEntityTypes<TRoot, TCurrent>
            where TRoot : Entity
            where TCurrent : Entity
        {
            public TRoot Root { get; }
            public TCurrent Current { get; }
            public string Alias { get; set; }

            public LinkEntityTypes(TRoot root, TCurrent current, string alias)
            {
                Root = root;
                Current = current;
                Alias = alias;
            }
        }


        [ThreadStatic]
        private static int _aliasedEntityCount;
        private static TFrom AddAliasedColumns<TFrom, TTo>(TFrom fromEntity, TTo toEntity, string alias, ColumnSet columns)
            where TFrom : Entity
            where TTo : Entity
        {
            if (toEntity == null) { return fromEntity; }

            alias = alias ?? toEntity.LogicalName + _aliasedEntityCount--;
            // Since the Projection is modifying the underlying objects, a HasAliasedAttribute Call is required.  
            if (columns.AllColumns)
            {
                foreach (var attribute in toEntity.Attributes.Where(a => !fromEntity.HasAliasedAttribute(alias + "." + a.Key)))
                {
                    fromEntity.AddAliasedValue(alias, toEntity.LogicalName, attribute.Key, attribute.Value);
                }
            }
            else
            {
                foreach (var c in columns.Columns.Where(v => toEntity.Attributes.Keys.Contains(v) && !fromEntity.HasAliasedAttribute(alias + "." + v)))
                {
                    fromEntity.AddAliasedValue(alias, toEntity.LogicalName, c, toEntity[c]);
                }
            }
            return fromEntity;
        }

        #endregion LinkEntity Join

        private static int Compare(Entity e, string attributeName, object compareTo)
        {
            IComparable value = null;
            if (e.Attributes.ContainsKey(attributeName))
            {
                value = ConvertCrmTypeToBasicComparable(e[attributeName]);
            }
            if (value == null)
            {
                if (compareTo == null)
                {
                    return 0;
                }

                return -1;
            }

            if (compareTo == null)
            {
                return 1;
            }

            var compareToType = compareTo.GetType();
            // This potentially could be expanded to include most references types.
            if (compareToType.IsEnum)
            {
                throw new FaultException(string.Format(@"The formatter threw an exception while trying to deserialize the message: There was an error while trying to deserialize parameter http://schemas.microsoft.com/xrm/2011/Contracts/Services:query. The InnerException message was 'Error in line 1 position 1978. Element 'http://schemas.microsoft.com/2003/10/Serialization/Arrays:anyType' contains data from a type that maps to the name " +
                                         "'{0}:{1}'.The deserializer has no knowledge of any type that maps to this name. Consider changing the implementation of the ResolveName method on your DataContractResolver to return a non-null value for name '{1}' and namespace '{0}'.'. Please see InnerException for more details.", compareToType.Namespace, compareToType.Name));
            }
            if (compareToType == typeof(string) && value is string)
            {
                // Handle String Casing Issues
               return string.Compare((string) value, (string) compareTo, StringComparison.OrdinalIgnoreCase);
            }

            return value.CompareTo(compareTo);
        }

        private static object ConvertCrmTypeToBasicComparable(Entity e, string attributeName)
        {
            if (e.Attributes.ContainsKey(attributeName))
            {
                return ConvertCrmTypeToBasicComparable(e[attributeName]);
            }

            return null;
        }

        private static string GetString(Entity e, string attributeName)
        {
            if (e.Attributes.ContainsKey(attributeName))
            {
                return e.GetAttributeValue<string>(attributeName);
            }

            return null;
        }

        private static IComparable ConvertCrmTypeToBasicComparable(object o)
        {
            var reference = o as EntityReference;
            if (reference != null)
            {
                return reference.GetIdOrDefault();
            }
            
            var osv = o as OptionSetValue;
            if (osv != null)
            {
                return osv.GetValueOrDefault();
            }

            var aliasedValue = o as AliasedValue;
            if (aliasedValue != null)
            {
                return ConvertCrmTypeToBasicComparable(aliasedValue.Value);
            }

            return (IComparable)o;
        }

        private static Guid Create<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
            // Clone entity so no changes will affect actual entity
            entity = entity.Serialize().DeserializeEntity<T>();

            AssertTypeContainsColumns<T>(entity.Attributes.Keys);
            AssertEntityReferencesExists(service, entity);
            SimulateCrmAttributeManipulations(entity);
            if (SimulateCrmCreateActionPrevention(entity, exception))
            {
                return Guid.Empty;
            }
            var table = SchemaGetOrCreate<T>(service.Info);
            service.PopulateAutoPopulatedAttributes(entity, true);

            // Clear non Attribute Related Values
            entity.FormattedValues.Clear();
            entity.KeyAttributes.Clear();
            //var relatedEntities = entity.RelatedEntities.ToList();
            entity.RelatedEntities.Clear();

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            table.Insert(entity);

            CreateActivityPointer(service, entity);

            return entity.Id;
        }

        /// <summary>
        /// Creates the activity pointer if the Entity is an Activty Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        private static void CreateActivityPointer<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (EntityHelper.GetEntityLogicalName<T>() == ActivityPointer.EntityLogicalName)
            {
                return; // Type is already an activity pointer, no need to recreated
            }

            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name);
            if (!IsActivityType<T>(properties))
            {
                return; // Type is not an activity type
            }

            // Copy over matching values and create
            service.Create(GetActivtyPointerForActivityEntity(entity, properties));
        }

        private static bool IsActivityType<T>(Dictionary<string, PropertyInfo> properties = null) where T : Entity
        {
            properties = properties ?? typeof(T).GetProperties().ToDictionary(p => p.Name);
            return properties.ContainsKey("ActivityId");
        }

        private static Entity GetActivtyPointerForActivityEntity<T>(T entity, Dictionary<string, PropertyInfo> properties = null) where T : Entity
        {
            properties = properties ?? typeof(T).GetProperties().ToDictionary(p => p.Name);
            var pointerFields = typeof(ActivityPointer.Fields).GetFields();
            var pointer = new Entity(ActivityPointer.EntityLogicalName, entity.Id);
            PropertyInfo prop;
            foreach (var att in pointerFields.Where(p => properties.TryGetValue(p.Name, out prop)).
                Select(field => field.GetRawConstantValue().ToString()).
                Where(entity.Contains))
            {
                pointer[att] = entity[att];
            }
            return pointer;
        }

        private static T Read<T>(LocalCrmDatabaseOrganizationService service, Guid id, ColumnSet cs, DelayedException exception) where T : Entity
        {
            var query = SchemaGetOrCreate<T>(service.Info).
                Where("Id == @0", id);
            var entity = query.FirstOrDefault();
            if (entity == null)
            {
                entity = Activator.CreateInstance<T>();
                entity.Id = id;
                exception.Exception = CrmExceptions.GetEntityDoesNotExistException(entity);
                return null;
            }

            if (!cs.AllColumns)
            {
                foreach (var key in entity.Attributes.Keys.Where(k => !cs.Columns.Contains(k)).ToList())
                {
                    entity.Attributes.Remove(key);
                }
            }

            service.RemoveFieldsCrmDoesNotReturn(entity);
            PopulateFormattedValues<T>(entity);
            return entity;
        }

        /// <summary>
        /// This is a hackish method but no time to improve...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="fe"></param>
        /// <returns></returns>
        public static EntityCollection ReadFetchXmlEntities<T>(LocalCrmDatabaseOrganizationService service, FetchType fe) where T : Entity
        {
            var entities = ReadEntities<T>(service, ConvertFetchToQueryExpression(service, fe));

            return fe.aggregateSpecified ? PerformAggregation<T>(entities, fe) : entities;
        }

        private static EntityCollection ReadEntitiesByAttribute<T>(LocalCrmDatabaseOrganizationService service, QueryByAttribute query, DelayedException delay) where T : Entity
        {
            if (AssertValidQueryByAttribute(query, delay)) { return null; }
            
            var qe = new QueryExpression(query.EntityName)
            {
                ColumnSet = query.ColumnSet,
                PageInfo = query.PageInfo,
                TopCount = query.TopCount,
            };

            qe.Orders.AddRange(query.Orders);

            for (var i = 0; i < query.Attributes.Count; i++)
            {
                qe.WhereEqual(query.Attributes[i], query.Values[i]);
            }
            return ReadEntities<T>(service, qe);
        }

        private static bool AssertValidQueryByAttribute(QueryByAttribute query, DelayedException delay)
        {
            if (!query.Attributes.Any())
            {
                delay.Exception = CrmExceptions.GetFaultException(ErrorCodes.QueryBuilderByAttributeNonEmpty);
                return true;
            }
            if (query.Attributes.Count != query.Values.Count)
            {
                delay.Exception = CrmExceptions.GetFaultException(ErrorCodes.QueryBuilderByAttributeMismatch);
                return true;
            }
            return false;
        }
        
        public static EntityCollection ReadEntities<T>(LocalCrmDatabaseOrganizationService service, QueryExpression qe) where T : Entity
        {
            _aliasedEntityCount = GetLinkedEntitiesWithoutAliasNameCount(qe);
            var query = SchemaGetOrCreate<T>(service.Info).AsQueryable();
            
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - this updates the query expression
            HandleFilterExpressionsWithAliases(qe, qe.Criteria).ToList();

            query = qe.LinkEntities.Aggregate(query, (q, e) => CallJoin(service.Info, q, e));

            query = ApplyFilter(query, qe.Criteria);

            var entities = query.ToList();

            if (qe.Orders.Any())
            {
                // Sort
                var ordered = entities.Order(qe.Orders[0]);
                entities = qe.Orders.Skip(1).Aggregate(ordered, (current, t) => current.Order(t)).ToList();
            }

            if (!qe.ColumnSet.AllColumns)
            {
                foreach (var entity in entities)
                {
                    foreach (var key in entity.Attributes.Keys.Where(k => !qe.ColumnSet.Columns.Contains(k) && !(entity[k] is AliasedValue)).ToList())
                    {
                        entity.Attributes.Remove(key);
                    }
                }
            }

            foreach (var entity in entities)
            {
                service.RemoveFieldsCrmDoesNotReturn(entity);
                PopulateFormattedValues<T>(entity);
            }

            var result = new EntityCollection();
            result.Entities.AddRange(entities);
            return result;
        }

        private static int GetLinkedEntitiesWithoutAliasNameCount(QueryExpression qe)
        {
            var count = 0;
            var searchQueue = new Queue<DataCollection<LinkEntity>>();
            searchQueue.Enqueue(qe.LinkEntities);
            while (searchQueue.Count > 0)
            {
                foreach (var link in searchQueue.Dequeue())
                {
                    if (link.LinkEntities != null && link.LinkEntities.Count > 0)
                    {
                        searchQueue.Enqueue(link.LinkEntities);
                    }
                    if (link.EntityAlias == null)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private static IEnumerable<FilterExpression> HandleFilterExpressionsWithAliases(QueryExpression qe, FilterExpression fe) {

            var condFilter = new FilterExpression(fe.FilterOperator);
            condFilter.Conditions.AddRange(HandleConditionsWithAliases(qe, fe));
            if (condFilter.Conditions.Any())
            {
                yield return condFilter;
            }

            // Handle Adding filter for Conditions where the Entity Name is referencing a LinkEntity.
            // This is used primarily for Outer Joins, where the attempt is to see if the join entity does not exist.
            foreach (var child in fe.Filters.SelectMany(filter => HandleFilterExpressionsWithAliases(qe, filter))) {
                yield return child;
            }
        }

        private static IEnumerable<ConditionExpression> HandleConditionsWithAliases(QueryExpression qe, FilterExpression filter)
        {
            // Handle Adding filter for Conditions where the Entity Name is referencing a LinkEntity.
            // This is used primarily for Outer Joins, where the attempt is to see if the join entity does not exist.
            foreach (var condition in filter.Conditions.Where(c => !string.IsNullOrWhiteSpace(c.EntityName)))
            {
                var link = qe.GetLinkEntity(condition.EntityName);

                // Condition is not aliasing a Linked Entity, or it is already attached to the linked entity.  In this case it will serve as a filter on the join
                if (link == null || link.LinkCriteria.Conditions.Contains(condition))
                {
                    continue;
                }
                // Add attribute to columns set.  This will be used to check to see if the found joined entity has the condition after the join...
                link.Columns.AddColumn(condition.AttributeName);
                // Return a Condition Expression that has the correct name for looking up the attribute later...
                yield return new ConditionExpression(link.EntityAlias ?? link.LinkToEntityName + "." + condition.AttributeName, condition.Operator, condition.Values);
            }
        }

        private static void PopulateFormattedValues<T>(Entity entity) where T : Entity
        {
            // TODO: Handle Names?
            if (!entity.Attributes.Values.Any(HasFormattedAttribute))
            {
                return;
            }
            var type = typeof (T);
            var properties = type.GetProperties().ToDictionary(p => p.Name.ToLower());
            foreach (var osvAttribute in entity.Attributes.Where(a => a.Value is OptionSetValue || (a.Value as AliasedValue)?.Value is OptionSetValue))
            {
                PropertyInfo property;
                if (osvAttribute.Key == Email.Fields.StateCode)
                {
                    property = properties[osvAttribute.Key];
                }
                else if (!properties.TryGetValue(osvAttribute.Key + "enum", out property))
                {
                    var aliased = osvAttribute.Value as AliasedValue;
                    if (aliased == null)
                    {
                        continue;
                    }
                    // Handle Aliased Value
                    var aliasedDictionary = EntityHelper.GetType(type.Assembly, type.Namespace, aliased.EntityLogicalName).GetProperties().ToDictionary(p => p.Name.ToLower());
                    if (!aliasedDictionary.TryGetValue(aliased.AttributeLogicalName + "enum", out property))
                    {
                        continue;
                    }
                    entity.FormattedValues.Add(osvAttribute.Key, Enum.ToObject(property.PropertyType.GenericTypeArguments[0], ((OptionSetValue)aliased.Value).Value).ToString());
                    continue;
                }
                entity.FormattedValues.Add(osvAttribute.Key, property.GetValue(entity).ToString());
            }
            foreach (var stringyAttribute in entity.Attributes.Where(a => !(a.Value is OptionSetValue)
                                                                          && !((a.Value as AliasedValue)?.Value is OptionSetValue)
                                                                          && HasFormattedAttribute(a.Value)))
            {
                var att = (stringyAttribute.Value as AliasedValue)?.Value ?? stringyAttribute.Value;
                if (att is Money)
                {
                    att = (att as Money).Value.ToString("C", CultureInfo.CurrentCulture);
                }
                if (att is DateTime)
                {
                    att = ((DateTime)att).ToString("g");
                }
                entity.FormattedValues.Add(stringyAttribute.Key, att.ToString());
            }
        }

        private static bool HasFormattedAttribute(object value)
        {
            while (value != null)
            {
                var aliased = value as AliasedValue;
                if (aliased == null)
                {
                    return value is OptionSetValue || value is Money || value is bool? || value is DateTime;
                }
                value = aliased.Value;
            }

            return false;
        }

        private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, FilterExpression filter) where T : Entity
        {
            return query.Where(e => EvaluateFilter(e, filter));
        }

        /// <summary>
        /// Returns true if the entity satisfies all of the constraints of the filter, otherwise, false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static bool EvaluateFilter<T>(T entity, FilterExpression filter) where T : Entity
        {
            if (entity == null) { return true; } // This should only happen for Left Outer Joins

            bool matchesFilter;
            if (filter.FilterOperator == LogicalOperator.And)
            {
                matchesFilter = filter.Conditions.All(c => ConditionIsTrue(entity, c)) && filter.Filters.All(f => EvaluateFilter(entity, f));
            }
            else
            {
                matchesFilter = filter.Conditions.Any(c => ConditionIsTrue(entity, c)) || filter.Filters.Any(f => EvaluateFilter(entity, f));
            }

            return matchesFilter;
        }

        private static bool ConditionIsTrue<T>(T entity, ConditionExpression condition) where T : Entity
        {
            // Date Time Details: https://community.dynamics.com/crm/b/gonzaloruiz/archive/2012/07/29/date-and-time-operators-in-crm-explained

            int days;
            bool value;
            var name = condition.GetQualifiedAttributeName();
            switch (condition.Operator)
            {
                case ConditionOperator.Equal:
                    value = Compare(entity, name, condition.Values[0]) == 0;
                    break;
                case ConditionOperator.NotEqual:
                    value = Compare(entity, name, condition.Values[0]) != 0;
                    break;
                case ConditionOperator.GreaterThan:
                    value = Compare(entity, name, condition.Values[0]) > 0;
                    break;
                case ConditionOperator.LessThan:
                    value = Compare(entity, name, condition.Values[0]) < 0;
                    break;
                case ConditionOperator.GreaterEqual:
                    value = Compare(entity, name, condition.Values[0]) >= 0;
                    break;
                case ConditionOperator.LessEqual:
                    value = Compare(entity, name, condition.Values[0]) <= 0;
                    break;
                case ConditionOperator.Like:
                    var str = GetString(entity, name);
                    if (str == null)
                    {
                        value = condition.Values[0] == null;
                    }
                    else
                    {
                        var likeCondition = (string) condition.Values[0];
                        // http://stackoverflow.com/questions/5417070/c-sharp-version-of-sql-like
                        value = new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(likeCondition.ToUpper(), ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(str.ToUpper());
                    }
                    break;
                case ConditionOperator.NotLike:
                    value = !ConditionIsTrue(entity, new ConditionExpression(condition.EntityName, condition.AttributeName, ConditionOperator.Like));
                    break;
                case ConditionOperator.In:
                    value = condition.Values.Any(v => v.Equals(ConvertCrmTypeToBasicComparable(entity, name)));
                    break;
                case ConditionOperator.NotIn:
                    value = !condition.Values.Any(v => v.Equals(ConvertCrmTypeToBasicComparable(entity, name)));
                    break;
                //case ConditionOperator.Between:
                //    break;
                //case ConditionOperator.NotBetween:
                //    break;
                case ConditionOperator.Null:
                    value = Compare(entity, name, null) == 0;
                    break;
                case ConditionOperator.NotNull:
                    value = Compare(entity, name, null) != 0;
                    break;
                case ConditionOperator.Yesterday:
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date);
                    break;
                case ConditionOperator.Today:
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1));
                    break;
                case ConditionOperator.Tomorrow:
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2));
                    break;
                case ConditionOperator.Last7Days:
                    condition.Operator = ConditionOperator.LastXDays;
                    condition.Values.Add(7);
                    value = ConditionIsTrue(entity, condition);
                    break;
                case ConditionOperator.Next7Days:
                    condition.Operator = ConditionOperator.NextXDays;
                    condition.Values.Add(7);
                    value = ConditionIsTrue(entity, condition);
                    break;
                //case ConditionOperator.LastWeek:
                //    break;
                //case ConditionOperator.ThisWeek:
                //    break;
                //case ConditionOperator.NextWeek:
                //    break;
                //case ConditionOperator.LastMonth:
                //    break;
                //case ConditionOperator.ThisMonth:
                //    break;
                //case ConditionOperator.NextMonth:
                //    break;
                //case ConditionOperator.On:
                //    break;
                //case ConditionOperator.OnOrBefore:
                //    break;
                //case ConditionOperator.OnOrAfter:
                //    break;
                //case ConditionOperator.LastYear:
                //    break;
                //case ConditionOperator.ThisYear:
                //    break;
                //case ConditionOperator.NextYear:
                //    break;
                //case ConditionOperator.LastXHours:
                //    break;
                //case ConditionOperator.NextXHours:
                //    break;
                case ConditionOperator.LastXDays:
                    days = condition.GetIntValueFromIntOrString();
                    if (days <= 0)
                    {
                        throw CrmExceptions.GetConditionValueGreaterThan0Exception();
                    }
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(-1d*days), DateTime.UtcNow);
                    break;
                case ConditionOperator.NextXDays:
                    days = condition.GetIntValueFromIntOrString();
                    if (days <= 0)
                    {
                        throw CrmExceptions.GetConditionValueGreaterThan0Exception();
                    }
                    value = IsBetween(entity, condition, DateTime.UtcNow, DateTime.UtcNow.Date.AddDays(days+1));
                    break;
                //case ConditionOperator.LastXWeeks:
                //    break;
                //case ConditionOperator.NextXWeeks:
                //    break;
                //case ConditionOperator.LastXMonths:
                //    break;
                //case ConditionOperator.NextXMonths:
                //    break;
                //case ConditionOperator.LastXYears:
                //    break;
                //case ConditionOperator.NextXYears:
                //    break;
                //case ConditionOperator.EqualUserId:
                //    break;
                //case ConditionOperator.NotEqualUserId:
                //    break;
                //case ConditionOperator.EqualBusinessId:
                //    break;
                //case ConditionOperator.NotEqualBusinessId:
                //    break;
                //case ConditionOperator.ChildOf:
                //    break;
                //case ConditionOperator.Mask:
                //    break;
                //case ConditionOperator.NotMask:
                //    break;
                //case ConditionOperator.MasksSelect:
                //    break;
                //case ConditionOperator.Contains:
                //    break;
                //case ConditionOperator.DoesNotContain:
                //    break;
                //case ConditionOperator.EqualUserLanguage:
                //    break;
                //case ConditionOperator.NotOn:
                //    break;
                //case ConditionOperator.OlderThanXMonths:
                //    break;
                //case ConditionOperator.BeginsWith:
                //    break;
                //case ConditionOperator.DoesNotBeginWith:
                //    break;
                //case ConditionOperator.EndsWith:
                //    break;
                //case ConditionOperator.DoesNotEndWith:
                //    break;
                //case ConditionOperator.ThisFiscalYear:
                //    break;
                //case ConditionOperator.ThisFiscalPeriod:
                //    break;
                //case ConditionOperator.NextFiscalYear:
                //    break;
                //case ConditionOperator.NextFiscalPeriod:
                //    break;
                //case ConditionOperator.LastFiscalYear:
                //    break;
                //case ConditionOperator.LastFiscalPeriod:
                //    break;
                //case ConditionOperator.LastXFiscalYears:
                //    break;
                //case ConditionOperator.LastXFiscalPeriods:
                //    break;
                //case ConditionOperator.NextXFiscalYears:
                //    break;
                //case ConditionOperator.NextXFiscalPeriods:
                //    break;
                //case ConditionOperator.InFiscalYear:
                //    break;
                //case ConditionOperator.InFiscalPeriod:
                //    break;
                //case ConditionOperator.InFiscalPeriodAndYear:
                //    break;
                //case ConditionOperator.InOrBeforeFiscalPeriodAndYear:
                //    break;
                //case ConditionOperator.InOrAfterFiscalPeriodAndYear:
                //    break;
                //case ConditionOperator.EqualUserTeams:
                //    break;
                default:
                    throw new NotImplementedException(condition.Operator.ToString());
            }
            return value;
        }

        private static bool IsBetween<T>(T entity, ConditionExpression condition, DateTime start, DateTime end, bool inclusiveStart = true, bool inclusiveEnd = false) where T : Entity
        {
            var isGreaterThan = inclusiveStart ? ConditionOperator.GreaterThan : ConditionOperator.GreaterEqual;
            var isLessThan = inclusiveEnd ? ConditionOperator.LessThan : ConditionOperator.LessEqual;
            return ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isGreaterThan, start)) 
                && ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isLessThan, end));
        }

        private static void AssertEntityReferencesExists(LocalCrmDatabaseOrganizationService service, Entity entity)
        {
            foreach (var foreign in entity.Attributes.Select(attribute => attribute.Value).OfType<EntityReference>())
            {
                service.Retrieve(foreign.LogicalName, foreign.Id, new ColumnSet(true));
            }
        }
        private static bool SimulateCrmCreateActionPrevention<T>(T entity, DelayedException exception) where T : Entity
        {
            switch (entity.LogicalName)
            {
                case Incident.EntityLogicalName:
                    AssertIncidentHasCustomer(entity, exception);
                    break;
                case OpportunityProduct.EntityLogicalName:
                    AssertOpportunityProductHasUoM(entity, exception);
                    break;
            }
            return exception.Exception != null;
        }

        private static void AssertIncidentHasCustomer(Entity entity, DelayedException exception)
        {
            if (entity.GetAttributeValue<EntityReference>(Incident.Fields.CustomerId) == null)
            {
                exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.unManagedidsincidentparentaccountandparentcontactnotpresent);
            }
        }

        private static void AssertOpportunityProductHasUoM(Entity entity, DelayedException exception)
        {
            if (entity.GetAttributeValue<EntityReference>(OpportunityProduct.Fields.UoMId) == null)
            {
                exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.MissingUomId);
            }
        }

        private static void AssertTypeContainsColumns<T>(IEnumerable<string> cols)
        {
            var properties = new HashSet<string>(typeof (T).GetProperties().Select(p => p.Name.ToLower()));
            var invalid = cols.FirstOrDefault(c => !properties.Contains(c));
            if (invalid != null)
            {
                //TODO: Need to lookup error message
                throw new Exception("Type " + typeof(T).Name + " does not contain an attribute with name " + invalid);
            }
        }

        /// <summary>
        /// Simulates the CRM attribute manipulations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        private static void SimulateCrmAttributeManipulations<T>(T entity) where T : Entity
        {
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLower());
            foreach (var key in entity.Attributes.Keys.ToList())
            {
                ConvertEntityArrayToEntityCollection(entity, key, properties);
                TrimMillisecondsFromDateTimeFields(entity, key);
            }
        }

        /// <summary>
        /// Simulates CRM update action preventions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static bool SimulateCrmUpdateActionPrevention<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
#if Xrm2015
                return false;
#endif
            switch (entity.LogicalName)
            {
                case Incident.EntityLogicalName:
                    if (service.CurrentRequestName != new CloseIncidentRequest().RequestName  && 
                        entity.GetAttributeValue<OptionSetValue>(Incident.Fields.StateCode).GetValueOrDefault() == (int) IncidentState.Resolved)
                    {
                        // Not executing as a part of a CloseIncidentRequest.  Disallow updating the State Code to Resolved.
                        exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.UseCloseIncidentRequest);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// CRM will convert non typed arrays into an IEnumerable&lt;T&gt;.  Handle that conversion here
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="properties">The properties.</param>
        private static void ConvertEntityArrayToEntityCollection<T>(T entity, string key, Dictionary<string, PropertyInfo> properties) where T : Entity
        {
            var value = entity[key] as Array;
            if (value == null || value.Length == 0)
            {
                return;
            }
            var prop = properties[key];
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (value is IEnumerable<Entity> && IsSameOrSubclass(typeof(Entity), prop.PropertyType.GetGenericArguments()[0]))
            {
                var entities = new EntityCollection();
                foreach (var att in value)
                {
                    var method = typeof(Entity).GetMethod("ToEntity");
                    method.MakeGenericMethod(prop.PropertyType.GetGenericArguments()[0]).Invoke(att, null);
                    entities.Entities.Add((Entity)method.MakeGenericMethod(prop.PropertyType.GetGenericArguments()[0]).Invoke(att, null));
                }
                entity[key] = entities;
            }
        }

        /// <summary>
        /// CRM doesn't include milliseconds when saving DateTime values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        private static void TrimMillisecondsFromDateTimeFields<T>(T entity, string key) where T : Entity
        {
            var value = entity[key] as DateTime?;
            if (value == null || value.Value.Millisecond == 0)
            {
                return;
            }
            var time = value.Value;
            entity[key] = (DateTime?)(time.AddMilliseconds(-time.Millisecond));
        }

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        public static void Assign<T>(LocalCrmDatabaseOrganizationService service, EntityReference target, EntityReference assignee) where T : Entity
        {
            var databaseValue = SchemaGetOrCreate<T>(service.Info).First(e => e.Id == target.Id);
            databaseValue["ownerid"] = assignee;
            SchemaGetOrCreate<T>(service.Info).Update(databaseValue);
        }

        private static void Update<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
            AssertTypeContainsColumns<T>(entity.Attributes.Keys);
            AssertEntityReferencesExists(service, entity);
            SimulateCrmAttributeManipulations(entity);
            if (SimulateCrmUpdateActionPrevention(service, entity, exception))
            {
                return;
            }
            
            // Get the Entity From the database
            var databaseValue = SchemaGetOrCreate<T>(service.Info).FirstOrDefault(e => e.Id == entity.Id);
            if (databaseValue == null)
            {
                exception.Exception = CrmExceptions.GetEntityDoesNotExistException(entity);
                return;
            }

            // Update all of the attributes from the entity passed in, to the database entity
            foreach (var attribute in entity.Attributes)
            {
                databaseValue[attribute.Key] = attribute.Value;
            }
            
            // Set all Autopopulated values
            service.PopulateAutoPopulatedAttributes(databaseValue, false);

            SchemaGetOrCreate<T>(service.Info).Update(databaseValue);

            UpdateActivityPointer(service, databaseValue);
        }

        private static void UpdateActivityPointer<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (entity.LogicalName == ActivityPointer.EntityLogicalName || !IsActivityType<T>())
            {
                return; // Type is already an activity pointer, no need to reupdate
            }

            service.Update(GetActivtyPointerForActivityEntity(entity));
        }

        private static void Delete<T>(LocalCrmDatabaseOrganizationService service, Guid id, DelayedException exception) where T : Entity
        {
            var entity = Activator.CreateInstance<T>();
            entity.Id = id;
            if (!SchemaGetOrCreate<T>(service.Info).Any(e => e.Id == id))
            {
                exception.Exception = CrmExceptions.GetEntityDoesNotExistException(entity);
                return;
            }

            SchemaGetOrCreate<T>(service.Info).Delete(entity);
            DeleteActivityPointer<T>(service, id);
        }

        private static void DeleteActivityPointer<T>(LocalCrmDatabaseOrganizationService service, Guid id) where T : Entity
        {
            if (EntityHelper.GetEntityLogicalName<T>() == ActivityPointer.EntityLogicalName || !IsActivityType<T>())
            {
                return; // Type is already an activity pointer, no need to redelete, or type is not an activity type
            }

            service.Delete(ActivityPointer.EntityLogicalName, id);
        }
    }
}
