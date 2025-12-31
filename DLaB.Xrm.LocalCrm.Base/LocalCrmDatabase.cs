#nullable enable
using DLaB.Xrm.CrmSdk;
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NMemory;
using NMemory.Exceptions;
using NMemory.Tables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DLaB.Xrm.LocalCrm
{
#if !DEBUG_XRM_UNIT_TEST_CODE
    [System.Diagnostics.DebuggerNonUserCode]
#endif
    internal partial class LocalCrmDatabase : Database
    {
        private static readonly LocalCrmDatabase Default = new LocalCrmDatabase();
        private static readonly ConcurrentDictionary<string, LocalCrmDatabase> Databases = new ConcurrentDictionary<string, LocalCrmDatabase>();
        private static readonly object DatabaseCreationLock = new object();

        // ReSharper disable once InconsistentNaming
        private readonly ConcurrentDictionary<string, ITable> _tables = new ConcurrentDictionary<string, ITable>();
        internal static readonly EntityPropertiesCache PropertiesCache = EntityPropertiesCache.Instance;

        private static Entity GetDatabaseEntity(LocalCrmDatabaseInfo info, string logicalName, Guid id)
        {
            return (Entity)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(info, logicalName, nameof(GetDatabaseEntity), BindingFlags.NonPublic | BindingFlags.Static, info, id);
        }

        private static T? GetDatabaseEntity<T>(LocalCrmDatabaseInfo info, Guid id) where T : Entity
        {
            return SchemaGetOrCreate<T>(info).Where("Id == @0", id).FirstOrDefault();
        }

        private static ITable<T> SchemaGetOrCreate<T>(LocalCrmDatabaseInfo info) where T : Entity
        {
            var db = GetDatabaseForService(info);
            var logicalName = EntityHelper.GetEntityLogicalName<T>();

            if (db._tables.TryGetValue(logicalName, out var table))
            {
                return (ITable<T>) table;
            }

            table = db.Tables.Create<T, Guid>(e => e.Id, null!);
            if (db._tables.TryAdd(logicalName, table))
            {
                return (ITable<T>) table;
            }

            if (!db._tables.TryGetValue(logicalName, out table))
            {
                throw new Exception("Could Not Create Table " + EntityHelper.GetEntityLogicalName<T>());
            }

            return (ITable<T>) table;
        }

        private static LocalCrmDatabase GetDatabaseForService(LocalCrmDatabaseInfo info)
        {
            LocalCrmDatabase? db;
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

        private static int Compare(Entity e, string attributeName, object? compareTo)
        {
            IComparable? value = null;
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

                return -2; // Value was null
            }

            if (compareTo == null)
            {
                return 1;
            }

            var compareToType = compareTo.GetType();
            if (compareToType == typeof(string) && value is string strValue)
            {
                // Handle String Casing Issues
                return string.Compare(strValue, (string) compareTo, StringComparison.OrdinalIgnoreCase);
            }

            if (compareToType == typeof(DateTime) && value is DateTime)
            {
                return DateTime.Compare((DateTime) value, ((DateTime) compareTo).RemoveMilliseconds());
            }

            if(value is Guid && compareTo is string){
                return value.CompareTo(new Guid(compareTo.ToString()!));
            }

            return value.CompareTo(compareTo);
        }

        private static object? ConvertCrmTypeToBasicComparable(Entity e, string attributeName)
        {
            if (e.Attributes.ContainsKey(attributeName))
            {
                return ConvertCrmTypeToBasicComparable(e[attributeName]);
            }

            return null;
        }

        private static string? GetString(Entity e, string attributeName)
        {
            return e.GetAttributeValue<string>(attributeName);
        }

#if !PRE_MULTISELECT
        private static OptionSetValueCollection? GetOptionSetValueCollection(Entity e, string attributeName)
        {
            return e.Attributes.ContainsKey(attributeName) ? e.GetAttributeValue<OptionSetValueCollection>(attributeName) : null;
        }
#endif
        internal static Type GetType(LocalCrmDatabaseInfo info, string logicalName)
        {
            try
            {
                return EntityHelper.GetType(info.EarlyBoundEntityAssembly, info.EarlyBoundNamespace, logicalName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Entity with logical name '{logicalName}' was not found in '{info.EarlyBoundNamespace}' namespace of assembly '{info.EarlyBoundEntityAssembly}'.", ex);
            }
        }

        private static IComparable? ConvertCrmTypeToBasicComparable(object? o)
        {
            if (o == null)
            {
                return null;
            }

            if (o is EntityReference reference)
            {
                return reference.GetIdOrDefault();
            }

            if (o is OptionSetValue osv)
            {
                return osv.GetValueOrDefault();
            }

            if (o is AliasedValue aliasedValue)
            {
                return ConvertCrmTypeToBasicComparable(aliasedValue.Value);
            }

            if (o is Money money)
            {
                return money.GetValueOrDefault();
            }

            return (IComparable) o;
        }

        private static Guid Create<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
            // Clone entity so no changes will affect actual entity
            entity = entity.Clone(true);

            AssertTypeContainsColumns<T>(entity.Attributes.Keys);
            if (AssertValidAttributeTypes<T>(entity, exception))
            {
                return Guid.Empty;
            }
            AssertEntityReferencesExists(service, entity);
            SimulateCrmAttributeManipulations(entity);
            if (SimulateCrmCreateActionPrevention(service, entity, exception))
            {
                return Guid.Empty;
            }

            var table = SchemaGetOrCreate<T>(service.Info);
            PopulateAutoPopulatedAttributes(service, entity, true);

            // Clear non Attribute Related Values
            entity.FormattedValues.Clear();
#if !PRE_KEYATTRIBUTE
            entity.KeyAttributes.Clear();
#endif
            //var relatedEntities = entity.RelatedEntities.ToList();
            entity.RelatedEntities.Clear();

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            try
            {
                table.Insert(entity);
            }
            catch (MultipleUniqueKeyFoundException)
            {
                throw new Exception("Cannot insert duplicate key");
            }

            PostCreateActions(service, entity);

            return entity.Id;
        }

        private static void PostCreateActions<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            CreateActivityPointer(service, entity);
            CreateActivityParties(service, entity);
            CreateMirroredConnection(service, entity);
            SetCachePrimaryName(service, entity);
        }

        private static T Read<T>(LocalCrmDatabaseOrganizationService service, Guid id, ColumnSet cs, DelayedException exception) where T : Entity
        {
            var query = SchemaGetOrCreate<T>(service.Info).Where("Id == @0", id);
            var entity = query.FirstOrDefault();
            if (entity == null)
            {
                entity = Activator.CreateInstance<T>();
                entity.Id = id;
                exception.Exception = CrmExceptions.GetEntityDoesNotExistException(entity);
                return null!;
            }

            return ProcessEntityForReturn(service, cs, entity, false);
        }

        private static T ProcessEntityForReturn<T>(LocalCrmDatabaseOrganizationService service, ColumnSet cs, T entity, bool checkForAliasedValues) where T : Entity
        {
            cs = cs ?? new ColumnSet(false);
            if (!cs.AllColumns)
            {
                foreach (var key in entity.Attributes.Keys.Where(k => !cs.Columns.Contains(k)
                    && (!checkForAliasedValues || !(entity[k] is AliasedValue))).ToList())
                {
                    entity.Attributes.Remove(key);
                }
            }

            service.RemoveFieldsCrmDoesNotReturn(entity);
            PopulateReferenceNames(service, entity);
            PopulateFormattedValues(service.Info, entity);
            return entity.Clone(true);
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

            // Utilize the QueryExpression aggregates that are mapped in the ConvertFetchToQueryExpression if available, else use older, incomplete implementation.
#if PRE_MULTISELECT
            return fe.aggregateSpecified ? PerformAggregation<T>(entities, fe) : entities;
#else
            return entities;
#endif
        }

        private static EntityCollection ReadEntitiesByAttribute<T>(LocalCrmDatabaseOrganizationService service, QueryByAttribute query, DelayedException delay) where T : Entity
        {
            if (AssertValidQueryByAttribute(query, delay)) { return null!; }
            
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

        private static EntityCollection ReadEntities<T>(LocalCrmDatabaseOrganizationService service, QueryExpression qe, DelayedException delay) where T : Entity
        {
            // Need to assert value
            if (AssertNonCyclicalQuery(qe, delay)
                ||AssertValidAttributeExpressionQuery(qe, delay)
                || AssertValidQueryTypes(qe, delay)
                || AssertValidCount(qe, delay))
            {
                return null!;
            }
            PopulateLinkEntityAliases(qe.LinkEntities);
            var query = SchemaGetOrCreate<T>(service.Info).AsQueryable();
            
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - this updates the query expression
            HandleFilterExpressionsWithAliases(qe, qe.Criteria).ToList();
            //var linkedEntities = GetLinkedEntitiesWithMappedAssociations(qe.LinkEntities);
            query = qe.LinkEntities.Aggregate(query, (q, e) => CallJoin(service.Info, q, e));
            query = ApplyFilter(query, qe.Criteria, new QueryContext(service.Info));

            var entities = query.ToList();

            if (ApplyAggregates(entities, qe.ColumnSet, delay))
            {
                return null!;
            }
            ;
            if (qe.Orders.Any())
            {
                // Sort
                var ordered = entities.Order(qe.Orders[0]);
                entities = qe.Orders.Skip(1).Aggregate(ordered, (current, t) => current.Order(t)).ToList();
            }

            var result = new EntityCollection();
            var cs = qe.ColumnSet;
            if (qe.Distinct)
            {
                entities = entities.GroupBy(e => GetUniqueDistinctKey(e, qe)).Select(g => g.First()).ToList();
            }

            var maxCount = qe.TopCount ?? qe.PageInfo?.Count ?? entities.Count;
            maxCount = maxCount == 0 ? entities.Count : maxCount;
            var count = 0;
            foreach (var entity in entities.TakeWhile(_ => count++ < maxCount))
            {
                result.Entities.Add(ProcessEntityForReturn(service, cs, entity, true));
            }

            return result;
        }

        private static bool AssertNonCyclicalQuery(QueryExpression query, DelayedException delay)
        {
            var visited = new HashSet<FilterExpression>();
            var toVisit = new Stack<FilterExpression>();
            toVisit.Push(query.Criteria);

            var visitedLinks = new HashSet<LinkEntity>();
            var toVisitLinks = new Stack<LinkEntity>();
            foreach (var link in query.LinkEntities)
            {
                toVisitLinks.Push(link);
            }
            while (toVisitLinks.Count > 0)
            {
                var current = toVisitLinks.Pop();
                if (!visitedLinks.Add(current))
                {
                    delay.Exception = CrmExceptions.GetLinkEntityCyclicalException();
                    return true;
                }
                if (current.LinkCriteria != null)
                {
                    toVisit.Push(current.LinkCriteria);
                }
                foreach (var child in current.LinkEntities)
                {
                    toVisitLinks.Push(child);
                }
            }

            while (toVisit.Count > 0)
            {
                var current = toVisit.Pop();
                if (!visited.Add(current))
                {
                    delay.Exception = CrmExceptions.GetFilterExpressionCyclicalException();
                    return true;
                }
                foreach (var child in current.Filters)
                {
                    toVisit.Push(child);
                }
            }

            return false;
        }

        private static bool AssertValidQueryTypes(QueryExpression query, DelayedException delay)
        {
            var filtersToSearch = new Stack<FilterExpression>();
            filtersToSearch.Push(query.Criteria);
            while (filtersToSearch.Count > 0)
            {
                var filter = filtersToSearch.Pop();
                foreach (var child in filter.Filters)
                {
                    filtersToSearch.Push(child);
                }

                foreach (var condition in filter.Conditions)
                {
                    // This potentially could be expanded to include most references types.
                    var value = condition.Values.FirstOrDefault(v => v != null && v.GetType().IsEnum);
                    if (value != null)
                    {
                        delay.Exception = CrmExceptions.GetFormatterException(value.GetType());
                        return true;
                    }
                }
            }

            return false;
        }
        private static bool AssertValidCount(QueryExpression query, DelayedException delay)
        {
            if (query.TopCount.HasValue
                && (query.PageInfo.Count != 0
                    || query.PageInfo.PageNumber != 0))
            {
                delay.Exception = CrmExceptions.GetTopCountCantBeSpecifiedWithPagingInfoException(query.TopCount.Value);
                return true;
            }
            return false;
        }

        private static string GetUniqueDistinctKey(Entity entity, QueryExpression qe)
        {
            var unique = new Entity();

            if (!qe.ColumnSet.AllColumns)
            {
                foreach (var column in qe.ColumnSet.Columns)
                {
                    if (entity.Contains(column))
                    {
                        unique[column] = entity[column];
                    }
                }
            }

            AddLinkedEntityColumns(entity, qe.LinkEntities, unique);

            return unique.ToStringDebug();
        }

        private static void AddLinkedEntityColumns(Entity entity, DataCollection<LinkEntity> linkedEntities, Entity unique)
        {
            foreach(var link in linkedEntities)
            {
                AddLinkedEntityColumns(entity, link.LinkEntities, unique);
                if (link.Columns.AllColumns)
                {
                    continue;
                }

                foreach (var column in link.Columns.Columns.Select(c => link.EntityAlias + "." + c))
                {
                    if (entity.Contains(column))
                    {
                        unique[column] = entity.GetAttributeValue<AliasedValue>(column)?.Value;
                    }
                }
            }
        }

        private static void PopulateLinkEntityAliases(DataCollection<LinkEntity> linkEntities)
        {
            var count = 1;
            var searchQueue = new Queue<DataCollection<LinkEntity>>();
            searchQueue.Enqueue(linkEntities);
            while (searchQueue.Count > 0)
            {
                foreach (var link in searchQueue.Dequeue())
                {
                    if (link.LinkEntities != null && link.LinkEntities.Count > 0)
                    {
                        searchQueue.Enqueue(link.LinkEntities);
                    }
                    if (string.IsNullOrWhiteSpace(link.EntityAlias))
                    {
                        link.EntityAlias = link.LinkToEntityName + count++;
                    }
                }
            }
        }

        private static void PopulateFormattedValues<T>(LocalCrmDatabaseInfo info, T entity) where T : Entity
        {
            if (!entity.Attributes.Values.Any(HasFormattedAttribute))
            {
                return;
            }
            var properties = PropertiesCache.For<T>();
            AddOptionSetValueNames(info, entity, properties);
            AddMoneyLookupAndDateFormattedValues(entity);
        }

        private static void AddOptionSetValueNames<T>(LocalCrmDatabaseInfo info, T entity, EntityProperties properties) where T : Entity
        {
            foreach (var osvAttribute in entity.Attributes.Where(a => a.Value is OptionSetValue || (a.Value as AliasedValue)?.Value is OptionSetValue))
            {
                PropertyInfo property;
                if (osvAttribute.Key == Email.Fields.StateCode)
                {
                    property = properties.GetProperty(osvAttribute.Key);
                }
                else if (!properties.PropertiesByLowerCaseName.TryGetValue(osvAttribute.Key + "enum", out var lowerCaseProperties))
                {
                    if (!(osvAttribute.Value is AliasedValue aliased))
                    {
                        continue;
                    }

                    // Handle Aliased Value
                    var aliasedDictionary = PropertiesCache.For(info, aliased.EntityLogicalName).PropertiesByLowerCaseName;
                    if (!aliasedDictionary.TryGetValue(aliased.AttributeLogicalName + "enum", out lowerCaseProperties))
                    {
                        continue;
                    }

                    property = lowerCaseProperties.First(p => p.PropertyType.GenericTypeArguments.Length >= 1);
                    entity.FormattedValues.Add(osvAttribute.Key, Enum.ToObject(property.PropertyType.GenericTypeArguments[0], ((OptionSetValue) aliased.Value).Value).ToString());
                    continue;
                }
                else
                {
                    property = lowerCaseProperties.First(p => p.PropertyType.GenericTypeArguments.Length >= 1);
                }

                entity.FormattedValues.Add(osvAttribute.Key, property.GetValue(entity)!.ToString());
            }
        }

        private static void AddMoneyLookupAndDateFormattedValues<T>(T entity) where T : Entity
        {
            foreach (var stringyAttribute in entity.Attributes.Where(a => !(a.Value is OptionSetValue)
                && !((a.Value as AliasedValue)?.Value is OptionSetValue)
                && HasFormattedAttribute(a.Value)))
            {
                var att = (stringyAttribute.Value as AliasedValue)?.Value ?? stringyAttribute.Value;
                if (att is Money money)
                {
                    att = money.Value.ToString("C", CultureInfo.CurrentCulture);
                }
                else if (att is DateTime time)
                {
                    att = time.ToString("g");
                }
                else if (att is EntityReference lookup)
                {
                    if (lookup.Name == null)
                    {
                        continue;
                    }
                    att = lookup.Name;
                }

                entity.FormattedValues.Add(stringyAttribute.Key, att.ToString());
            }
        }
        
        private static bool HasFormattedAttribute(object value)
        {
            while (value != null)
            {
                if (!(value is AliasedValue aliased))
                {
                    return value is OptionSetValue || value is Money || value is bool || value is DateTime || value is EntityReference;
                }
                value = aliased.Value;
            }

            return false;
        }

        private static void AssertEntityIdsMatch(Entity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                return;
            }

            var id = entity.Attributes.Values.FirstOrDefault(v => v is Guid);
            if (id != null && !id.Equals(entity.Id))
            {
                throw CrmExceptions.GetEntityIdMustBeTheSameException();
            }
        }

        private static void AssertEntityReferencesExists(LocalCrmDatabaseOrganizationService service, Entity entity)
        {
            foreach (var foreign in entity.Attributes.Select(attribute => attribute.Value).OfType<EntityReference>())
            {
#if !PRE_KEYATTRIBUTE
                if (foreign.Id == Guid.Empty && foreign.KeyAttributes.Count > 0)
                {
                    var kvps = new List<object>();
                    foreach (var kvp in foreign.KeyAttributes)
                    {
                        kvps.Add(kvp.Key);
                        kvps.Add(kvp.Value);
                    }

                    var reference = service.GetFirstOrDefault(foreign.LogicalName, kvps.ToArray());
                    if (reference == null)
                    {
                        throw CrmExceptions.GetFaultException(ErrorCodes.RecordNotFoundByEntityKey, foreign.LogicalName);
                    }
                    foreign.Id = reference.Id;
                }
                else
                {
#endif
                    service.Retrieve(foreign.LogicalName, foreign.Id, new ColumnSet(false));
#if !PRE_KEYATTRIBUTE
                }
            }
            foreach(var att in entity.Attributes.Select(a => new { a.Key, Value = a.Value as EntityReference })
                                     .Where(a => a.Value?.KeyAttributes.Count > 0).ToList())
            {
                entity[att.Key] = new EntityReference(att.Value!.LogicalName, att.Value.Id)
                {
                    Name = att.Value.Name
                };
#endif
            }
        }

        private static bool SimulateCrmCreateActionPrevention<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
            switch (entity.LogicalName)
            {
                case Incident.EntityLogicalName:
                    AssertIncidentHasCustomer(entity, exception);
                    break;
                case OpportunityProduct.EntityLogicalName:
                    AssertOpportunityProductHasUoM(entity, exception);
                    break;
                case Connection.EntityLogicalName:
                    AssertConnectionRolesArePopulated(entity, false, exception);
                    AssertConnectionRolesAreAssociated(service, entity, false, exception);
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

        private static void AssertTypeContainsColumns<T>(IEnumerable<string> cols) where T: Entity
        {
            var properties = PropertiesCache.For<T>();
            foreach (var col in cols.Where(c => !properties.ContainsProperty(c)))
            {
                throw new Exception($"Type {typeof(T).Name} does not contain a property named {col}, or a property with an AttributeLogicalNameAttribute of {col}.");
            }
        }

        private static bool AssertValidAttributeTypes<T>(Entity entity, DelayedException exception) where T : Entity { 
            var properties = PropertiesCache.For<T>();
            foreach (var attribute in entity.Attributes)
            {
                var property = properties.GetProperty(attribute.Key);
                if (property == null)
                {
                    continue;
                }

                var attributeValue = attribute.Value;
                if (attributeValue == null)
                {
                    // Null value is allowed for nullable reference types / nullables; skip type check
                    continue;
                }

                // Unwrap Nullable<> on the property if present
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var attributeType = attributeValue.GetType();
                if (propertyType.IsAssignableFrom(attributeType)
                    || propertyType.IsEnum && (typeof(int).IsAssignableFrom(attributeType) || typeof(OptionSetValue).IsAssignableFrom(attributeType))
                    || IsGuidEntityReference(propertyType, attributeType, attributeValue)
                    || attributeType == typeof(EntityCollection) 
                        && (propertyType == typeof(EntityReference)
                            || propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                && typeof(Entity).IsAssignableFrom(propertyType.GetGenericArguments()[0])
                        )
#if !PRE_MULTISELECT
                    || attributeType == typeof(OptionSetValueCollection) 
                        && propertyType.IsGenericType
                        && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        && (propertyType.GetGenericArguments()[0].IsEnum || propertyType.GetGenericArguments()[0] == typeof(int))
#endif
                )
                {
                    continue;
                }
                exception.Exception = CrmExceptions.GetInvalidAttributeTypeException(attributeType);
            }

            return exception.Exception != null;

            bool IsGuidEntityReference(Type p1, Type p2, object value)
            {
                return (p1 == typeof(Guid) || p1 == typeof(EntityReference)) && (
                        p2 == typeof(EntityReference)
                        || p2 == typeof(Guid)
                        || value is string guidStr && Guid.TryParse(guidStr, out var _));
            }
        }

        private static void AssertConnectionRolesArePopulated(Entity entity, bool isUpdate, DelayedException exception)
        {
            var record1Null = (entity.GetAttributeValue<EntityReference>(Connection.Fields.Record1RoleId) 
                ?? entity.GetAttributeValue<EntityReference>(Connection.Fields.Record1Id)) == null;
            var record2Null = (entity.GetAttributeValue<EntityReference>(Connection.Fields.Record2RoleId)
                ?? entity.GetAttributeValue<EntityReference>(Connection.Fields.Record2Id)) == null;
            var aConnectionIsMissing = record1Null || record2Null;

            if (isUpdate)
            {
                var containsRecord1 = entity.Contains(Connection.Fields.Record1Id)
                    || entity.Contains(Connection.Fields.Record1RoleId);
                var containsRecord2 = entity.Contains(Connection.Fields.Record2Id)
                    || entity.Contains(Connection.Fields.Record2RoleId);
                aConnectionIsMissing = containsRecord1 && record1Null
                    || containsRecord2 && record2Null;

            }

            if(aConnectionIsMissing)
            {
                exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.BothConnectionSidesAreNeeded);
            }
        }

        private static void AssertConnectionRolesAreAssociated(LocalCrmDatabaseOrganizationService service, Entity entity, bool isUpdate, DelayedException exception)
        {
            var role1 = entity.GetAttributeValue<EntityReference>(Connection.Fields.Record1RoleId);
            var role2 = entity.GetAttributeValue<EntityReference>(Connection.Fields.Record2RoleId);

            if (isUpdate)
            {
                if (role1 == null && role2 == null)
                {
                    // Role never got set, exit
                    return;
                }

                if (role1 == null || role2 == null)
                {
                    // One is null, attempt to populate it
                    var dbVersion = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                    var dbRole1 = dbVersion.GetAttributeValue<EntityReference>(Connection.Fields.Record1RoleId);
                    var dbRole2 = dbVersion.GetAttributeValue<EntityReference>(Connection.Fields.Record2RoleId);

                    if (role1 == null)
                    {
                        role1 = role2!.NullSafeEquals(dbRole1)
                            ? dbRole2
                            : dbRole1;
                    }
                    else
                    {
                        role2 = role1.NullSafeEquals(dbRole2)
                            ? dbRole1
                            : dbRole2;
                    }
                }
            }

            if (role1 == null
                || role2 == null)
            {
                return;
            }

            var qe = new QueryExpression
            {
                ColumnSet = new ColumnSet(true),
                EntityName = ConnectionRoleAssociation.EntityLogicalName
            };
            qe.First().WhereEqual(
                ConnectionRoleAssociation.Fields.ConnectionRoleId, role1.Id,
                ConnectionRoleAssociation.Fields.AssociatedConnectionRoleId, role2.Id,
                LogicalOperator.Or,
                ConnectionRoleAssociation.Fields.ConnectionRoleId, role2.Id,
                ConnectionRoleAssociation.Fields.AssociatedConnectionRoleId, role1.Id);

            if (!service.RetrieveMultiple(qe).Entities.Any())
            {
                exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.UnrelatedConnectionRoles);
            }
        }

        /// <summary>
        /// Simulates the CRM attribute manipulations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        private static void SimulateCrmAttributeManipulations<T>(T entity) where T : Entity
        {
            var properties = typeof(T).GetProperties().GroupBy(p => p.Name.ToLower()).ToDictionary(p => p.Key, p => p.ToList());
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

            switch (entity.LogicalName)
            {
                case Incident.EntityLogicalName:
#if Xrm2015
                    break;
#else
                    if (service.CurrentRequestName != new CloseIncidentRequest().RequestName  && 
                        entity.GetAttributeValue<OptionSetValue>(Incident.Fields.StateCode).GetValueOrDefault() == (int) IncidentState.Resolved)
                    {
                        // Not executing as a part of a CloseIncidentRequest.  Disallow updating the State Code to Resolved.
                        exception.Exception = CrmExceptions.GetFaultException(ErrorCodes.UseCloseIncidentRequest);
                    }
                    break;
#endif
                case Connection.EntityLogicalName:
                    AssertConnectionRolesArePopulated(entity, true, exception);
                    AssertConnectionRolesAreAssociated(service, entity, true, exception);
                    break;
            }
            return exception.Exception != null;
        }

        /// <summary>
        /// CRM will convert non typed arrays into an IEnumerable&lt;T&gt;.  Handle that conversion here
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key.</param>
        /// <param name="properties">The properties.</param>
        private static void ConvertEntityArrayToEntityCollection<T>(T entity, string key, Dictionary<string, List<PropertyInfo>> properties) where T : Entity
        {
            if (!(entity[key] is Array value) || value.Length == 0)
            {
                return;
            }
            var prop = properties[key].FirstOrDefault(p => p.PropertyType.GetGenericArguments().Length > 0);
            var genericArgs = prop?.PropertyType.GetGenericArguments()[0];
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (genericArgs != null && value is IEnumerable<Entity> entityEnumerable && IsSameOrSubclass(typeof(Entity), genericArgs))
            {
                var entities = new EntityCollection();
                foreach (var att in entityEnumerable)
                {
                    entities.Entities.Add(GenericMethodCaller.InvokeToEntity(att, genericArgs));
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
            if (!(entity[key] is DateTime date) || date.Millisecond == 0)
            {
                return;
            }
            entity[key] = date.RemoveMilliseconds();
        }

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        public static void Assign<T>(LocalCrmDatabaseOrganizationService service, EntityReference target, EntityReference assignee) where T : Entity
        {
            var databaseValue = SchemaGetOrCreate<T>(service.Info).First(e => e.Id == target.Id);
            databaseValue[ActivityParty.Fields.OwnerId] = assignee;
            SchemaGetOrCreate<T>(service.Info).Update(databaseValue);
        }

        private static void Update<T>(LocalCrmDatabaseOrganizationService service, T entity, DelayedException exception) where T : Entity
        {
            AssertEntityIdsMatch(entity);
            if (AssertValidAttributeTypes<T>(entity, exception))
            {
                return;
            }
            AssertTypeContainsColumns<T>(entity.Attributes.Keys);
            AssertEntityReferencesExists(service, entity);
            SimulateCrmAttributeManipulations(entity);
            if (SimulateCrmUpdateActionPrevention(service, entity, exception))
            {
                return;
            }

            var schema = SchemaGetOrCreate<T>(service.Info);

            // Get the Entity From the database
            var databaseValue = schema.FirstOrDefault(e => e.Id == entity.Id);
            if (databaseValue == null)
            {
                exception.Exception = CrmExceptions.GetEntityDoesNotExistException(entity);
                return;
            }

            // Clone Entity attributes so updating a non-primitive attribute type does not cause changes to the database value
            entity = entity.Clone(true);

            // Update all of the attributes from the entity passed in, to the database entity
            foreach (var attribute in entity.Attributes)
            {
                databaseValue[attribute.Key] = attribute.Value;
            }
            
            // Set all Auto populated values
            PopulateAutoPopulatedAttributes(service, databaseValue, false);

            schema.Update(databaseValue);

            UpdateActivityPointer(service, databaseValue);
            CreateActivityParties(service, entity);
            SetCachePrimaryName(service, schema.First(e => e.Id == entity.Id));
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
    }
}
