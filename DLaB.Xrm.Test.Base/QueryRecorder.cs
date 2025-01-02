using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using DLaB.Common;
using System.Text;
using System.Reflection;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Attempts to record all data required to make queries work so the required objects can be created as a part of the test.
    /// </summary>
    public class QueryRecorder
    {
        /// <summary>
        /// The recorded entities required by the query.
        /// </summary>
        public Dictionary<EntityReference, Entity> Entities { get; private set; } = new();

        /// <summary>
        /// Records the result of a Retrieve operation.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="logicalName">The logical name of the entity.</param>
        /// <param name="id">The ID of the entity.</param>
        /// <param name="cs">The column set.</param>
        /// <param name="result">The retrieved entity.</param>
        public void RecordRetrieve(IOrganizationService service, string logicalName, Guid id, ColumnSet cs, Entity result)
        {
            RecordEntity(result);
        }

        private void RecordEntity(Entity entity)
        {
            if (Entities.TryGetValue(entity.ToEntityReference(), out var existing))
            {
                foreach (var keyValue in entity.Attributes
                                         .Where(kvp => kvp.Value is not AliasedValue
                                            && !existing.Contains(kvp.Key)))
                {
                    existing[keyValue.Key] = keyValue.Value;
                }
            }
            else
            {
                Entities.Add(entity.ToEntityReference(), entity.Clone());
            }
        }

        /// <summary>
        /// Records the result of a RetrieveMultiple operation.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="query">The query executed.</param>
        /// <param name="result">The retrieved entity collection.</param>
        public void RecordRetrieveMultiple(IOrganizationService service, QueryBase query, EntityCollection result)
        {
            switch (query)
            {
                case FetchExpression fe:
                    {
                        var response = (FetchXmlToQueryExpressionResponse)service.Execute(new FetchXmlToQueryExpressionRequest { FetchXml = fe.Query });
                        result = ProcessQuery(service, response.Query, result);
                        break;
                    }
                case QueryExpression qe:
                    result = ProcessQuery(service, qe, result);
                    break;
                case QueryByAttribute qba:
                    var localQe = new QueryExpression
                    {
                        ColumnSet = qba.ColumnSet,
                        EntityName = qba.EntityName
                    };
                    foreach (var att in qba.Attributes)
                    {
                        localQe.WhereIn(att, qba.Values);
                    }
                    result = ProcessQuery(service, localQe, result);
                    break;
            }

            foreach (var entity in result.Entities)
            {
                RecordEntity(entity);
                var aliasEntityNames = new HashSet<string>(entity.Attributes.Keys.Where(k => k.Contains(".")).Select(k => k.Split('.').First()));
                foreach (var aliasEntityName in aliasEntityNames)
                {
                    RecordEntity(GetAliasedEntity(entity, aliasEntityName));
                }
            }
        }

        private EntityCollection ProcessQuery(IOrganizationService service, QueryExpression qe, EntityCollection result)
        {
            var queryUpdated = false;
            // Add all filtered attributes to the column sets for the top level entity, and all link entities
            // Clone to ensure no changes are made to the Query Expression
            qe = qe.SerializeToJson().DeserializeJson<QueryExpression>();

            var idNames = GetIdAttributeLogicalNames(result);
            var attributes = GetFilteredAttributes(qe.Criteria);
            foreach (var attribute in qe.LinkEntities.Select(l => l.LinkFromAttributeName))
            {
                attributes.Add(attribute);
            }

            var columnsToQueryFor = new HashSet<string>(attributes.Where(a =>
                !qe.ColumnSet.Columns.Contains(a)
                && (!idNames.TryGetValue(qe.EntityName, out var idName)
                    || idName != a)));
            if (columnsToQueryFor.Count > 0)
            {
                queryUpdated = true;
                qe.ColumnSet.AddColumns(columnsToQueryFor.ToArray());
            }

            queryUpdated = ProcessLinkedEntities(qe.LinkEntities, idNames) || queryUpdated;
            return queryUpdated
                ? service.RetrieveMultiple(qe)
                : result;
        }

        private static Dictionary<string, string> GetIdAttributeLogicalNames(EntityCollection result)
        {
            var aliasedValuesByLogicalName = new Dictionary<string, string>();
            foreach (var entity in result.Entities)
            {
                foreach (var attribute in entity.Attributes.Select(a => a.Value as AliasedValue)
                             .Where(a => a is { Value: Guid }))
                {
                    aliasedValuesByLogicalName[attribute.EntityLogicalName] = attribute.AttributeLogicalName;
                }
            }

            return aliasedValuesByLogicalName;
        }

        private HashSet<string> GetFilteredAttributes(FilterExpression filter)
        {
            var attributes = new HashSet<string>();
            AddAttributesFromFilter(filter);

            return attributes;

            void AddAttributesFromFilter(FilterExpression localFilter)
            {
                foreach (var condition in localFilter.Conditions)
                {
                    attributes.Add(condition.AttributeName);
                }

                foreach (var subFilter in localFilter.Filters)
                {
                    AddAttributesFromFilter(subFilter);
                }
            }
        }

        private bool ProcessLinkedEntities(DataCollection<LinkEntity> linkedEntities, Dictionary<string, string> idNames)
        {
            var queryUpdated = false;
            foreach (var link in linkedEntities)
            {
                var attributes = GetFilteredAttributes(link.LinkCriteria);
                foreach (var attribute in link.LinkEntities.Select(l => l.LinkFromAttributeName))
                {
                    attributes.Add(attribute);
                }

                attributes.Add(link.LinkToAttributeName);

                link.Columns ??= new ColumnSet();
                var columnsToQueryFor = new HashSet<string>(attributes.Where(a =>
                    !link.Columns.Columns.Contains(a)
                    && (!idNames.TryGetValue(link.LinkToEntityName, out var idName)
                        || idName != a)));
                if (columnsToQueryFor.Count > 0)
                {
                    queryUpdated = true;
                    link.Columns.AddColumns(columnsToQueryFor.ToArray());
                }
            }
            return queryUpdated;
        }

        /// <summary>
        /// Gets the aliased entity from the current entity with the given entity name.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="aliasedEntityName">Name of the aliased entity.</param>
        /// <returns>The aliased entity.</returns>
        private Entity GetAliasedEntity(Entity entity, string aliasedEntityName)
        {
            var aliasedAttributes = entity.Attributes.Where(kvp => kvp.Key.StartsWith(aliasedEntityName + ".")).ToList();
            var entityLogicalName = entity.GetAttributeValue<AliasedValue>(aliasedAttributes.First().Key).EntityLogicalName;
            var aliasedEntity = new Entity(entityLogicalName);

            foreach (var aliasedAttribute in aliasedAttributes)
            {
                var aliasedValue = (AliasedValue)aliasedAttribute.Value;
                aliasedEntity[aliasedValue.AttributeLogicalName] = aliasedValue.Value;
                if (aliasedValue.Value is Guid id)
                {
                    aliasedEntity.Id = id;
                }
            }

            return aliasedEntity;
        }

        /// <summary>
        /// Generates code for the recorded entities.
        /// </summary>
        /// <param name="earlyBoundAssembly">The early bound assembly.</param>
        /// <param name="namespace">The namespace for the generated code.</param>
        /// <returns>The generated code as a string.</returns>
        public string GenerateCode(Assembly earlyBoundAssembly, string @namespace)
        {
            var sb = new StringBuilder();
            var countByEntity = new Dictionary<string, int>();
            foreach (var entity in Entities.Values)
            {
                var type = EntityHelper.GetType(earlyBoundAssembly, @namespace, entity.LogicalName);
                if(countByEntity.TryGetValue(entity.LogicalName, out var count))
                {
                    countByEntity[entity.LogicalName] = count + 1;
                }
                else
                {
                    countByEntity[entity.LogicalName] = 1;
                }

                GenerateCodeForEntity(sb, entity, type, countByEntity[entity.LogicalName]);
            }

            return sb.ToString();
        }

        private void GenerateCodeForEntity(StringBuilder sb, Entity entity, Type type, int count)
        {
            var tab = "\t";
            sb.AppendLine($"var {entity.LogicalName}{count} = new {type.Name} {{");
            foreach (var att in entity.Attributes.OrderBy(a => a.Key))
            {
                var attributeName = att.Key;
                var attributeValue = att.Value;

                var propertyName = type.GetProperties()
                                       .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == attributeName)
                                       ?.Name;

                if (attributeValue == null)
                {
                    continue;
                }

                if (attributeValue.GetType().IsEnum)
                {
                    var enumType = attributeValue.GetType();
                    sb.Append($"{tab}{propertyName} = {enumType.Name}.{Enum.GetName(enumType, attributeValue)},");
                }
                else if (attributeValue is OptionSetValue)
                {
                    propertyName = type.GetProperties()
                        .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == attributeName
                            && (p.PropertyType.IsEnum || p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum))?.Name ?? propertyName;
                }

                sb.Append($"{tab}{propertyName} = {(dynamic)GetInitValue(attributeName, attributeValue, type)},");
                sb.AppendLine();
            }

            sb.AppendLine("}");
        }

        private string GetInitValue(string name, dynamic value, Type entityType)
        {
            if (value == null)
            {
                return "null";
            }

            try { 
                return GetInitString(name, value, entityType);
            }
            catch(Exception ex)
            {
                throw new Exception($"Unable to get Init Value for type {value.GetType()} and value {value}.", ex);
            }
        }

        // ReSharper disable UnusedParameter.Local
        private string GetInitString(string name, bool bol, Type entityType)
        {
            return bol.ToString().ToLower();
        }

        private string GetInitString(string name, byte[] bytes, Type entityType)
        {
            return $"new byte[]{{ {string.Join(", ", bytes)} }}";
        }

        // ReSharper disable once RedundantAssignment
        private string GetInitString(string name, EntityReference reference, Type entityType)
        {
            name = EntityHelper.GetType(entityType.Assembly, entityType.Namespace ?? "", reference.LogicalName).Name;
            return $"new EntityReference({name}.EntityLogicalName, new Guid(\"{reference.Id}\"))";
        }

        private string GetInitString(string name, DateTime date, Type entityType)
        {
            return $"new DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second})";
        }

        private string GetInitString(string name, Guid id, Type entityType)
        {
            return $"new Guid(\"{id}\")";
        }

        private string GetInitString(string name, Money money, Type entityType)
        {
            return $"new Money({money.Value})";
        }

        private string GetInitString(string name, OptionSetValue osv, Type entityType)
        {
            var property = entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == name
                && (p.PropertyType.IsEnum || p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum));

            if (property == null)
            {
                return $"new OptionSetValue({osv.Value}";
            }

            var type = property.PropertyType.IsEnum
                ? property.PropertyType
                : property.PropertyType.GenericTypeArguments.First();
            return $"{type.Name}.{Enum.GetName(type, osv.Value)}";
        }

        private string GetInitString(string name, string str, Type entityType)
        {
            return str;
        }

        private string GetInitString(string name, object obj, Type entityType)
        {
            return obj.ToString();
        }

        // ReSharper restore UnusedParameter.Local
    }
}
