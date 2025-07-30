using DLaB.Xrm.CrmSdk;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
#if PRE_MULTISELECT
        private static bool AssertValidAttributeExpressionQuery(QueryExpression query, DelayedException delay)
        {
            return false;
        }

        private static bool ApplyAggregates<T>(List<T> entities, ColumnSet cs, DelayedException delay) where T : Entity
        {
            return false;
        }
#else
        private static bool AssertValidAttributeExpressionQuery(QueryExpression query, DelayedException delay)
        {
            if (query.ColumnSet?.AttributeExpressions?.Any(a => a.HasGroupBy && a.AggregateType != XrmAggregateType.None) == true
                && (query.ColumnSet.AllColumns
                    || query.ColumnSet.Columns.Any()))
            {
                delay.Exception = CrmExceptions.GetAttributeCanNotBeSpecifiedIfAnAggregateOperationIsRequestedException();
                return true;
            }
            return false;
        }


        private const string GroupBySeparator = "A7E62945-B008-4E00-A150-1B9D3A9E15F5";
        private const string NullValue = "84E908C5-EA13-4EF8-9063-258EC65E80BA";
        private static bool ApplyAggregates<T>(List<T> entities, ColumnSet cs, DelayedException delay) where T : Entity
        {
            if (cs?.AttributeExpressions?.Any() != true
                || entities.Count == 0)
            {
                return false;
            }

            if (cs.AttributeExpressions.Any(e => e.AggregateType != XrmAggregateType.None)
                && entities.Count > 50000)
            {
                delay.Exception = CrmExceptions.GetFaultException(ErrorCodes.AggregateQueryRecordLimitExceeded);
                return true;
            }

            var outputEntities = new List<T>();
            var groupByColumns = cs.AttributeExpressions.Where(a => a.HasGroupBy).ToList();
            if(groupByColumns.Count == 0)
            {
                var aggregate = new Entity(entities[0].LogicalName).ToEntity<T>();
                var onlyAlias = true;
                foreach (var column in cs.AttributeExpressions)
                {
                    onlyAlias = onlyAlias && column.AggregateType == XrmAggregateType.None;
                    ApplyAggregation(entities, column, aggregate);
                }
                if (onlyAlias)
                {
                    outputEntities.AddRange(entities);
                }
                else if (aggregate.Attributes.Count > 0)
                {
                    outputEntities.Add(aggregate);
                }
            }
            else
            {
                Dictionary<string, List<T>> groupBy = new Dictionary<string, List<T>>();
                foreach (var entity in entities)
                {
                    var key = string.Join(GroupBySeparator, groupByColumns.Select(c => GetGroupByKey(entity, c)));
                    if (!groupBy.TryGetValue(key, out var grouping))
                    {
                        grouping = [];
                        groupBy[key] = grouping;
                    }

                    grouping.Add(entity);
                }

                foreach (var group in groupBy)
                {
                    var aggregate = new Entity(entities[0].LogicalName).ToEntity<T>();
                    var useAggregate = false;
                    foreach (var column in cs.AttributeExpressions)
                    {
                        useAggregate = useAggregate || column.AggregateType != XrmAggregateType.None;
                        ApplyAggregation(group.Value, column, aggregate);
                    }
                    if (useAggregate && aggregate.Attributes.Count > 0)
                    {
                        foreach(var groupByColumn in groupByColumns)
                        {
                            var firstEntity = group.Value.First();
                            aggregate[groupByColumn.Alias] = new AliasedValue(firstEntity.LogicalName, groupByColumn.AttributeName, GetGroupByValue(firstEntity, groupByColumn));
                        }
                        outputEntities.Add(aggregate);
                    }
                }
            }

            entities.Clear();
            entities.AddRange(outputEntities);
            return false;
        }

        private static string GetGroupByKey<T>(T entity, XrmAttributeExpression c) where T : Entity
        {
            if (entity.GetAttributeValue<object>(c.AttributeName) == null)
            {
                return NullValue;
            }
            return c.DateTimeGrouping == XrmDateTimeGrouping.None
                ? entity[c.AttributeName].ObjectToStringDebug()
                : GetGroupByValue(entity, c).ToString();
        }

        private static object GetGroupByValue<T>(T entity, XrmAttributeExpression c) where T : Entity
        {
            var value = entity[c.AttributeName];
            switch (c.DateTimeGrouping)
            {
                case XrmDateTimeGrouping.None:
                    return value;
                case XrmDateTimeGrouping.Day:
                    return ((DateTime)value).Day;
                case XrmDateTimeGrouping.Week:
                    var cultureInfo = CultureInfo.CurrentCulture;
                    var calendar = cultureInfo.Calendar;
                    var dateTimeFormat = cultureInfo.DateTimeFormat;

                    return calendar.GetWeekOfYear(((DateTime)value), dateTimeFormat.CalendarWeekRule, dateTimeFormat.FirstDayOfWeek);
                case XrmDateTimeGrouping.Month:
                    return ((DateTime)value).Month;
                case XrmDateTimeGrouping.Quarter:
                    return (((DateTime)value).Month/3);
                case XrmDateTimeGrouping.Year:
                    return ((DateTime)value).Year;
                case XrmDateTimeGrouping.FiscalPeriod:
                    return $"Quarter {(((DateTime)value).Month - 1) / 3 + 1} FY{((DateTime)value).Year}";
                case XrmDateTimeGrouping.FiscalYear:
                    return $"FY{((DateTime)value).Year}";
                default:
                    throw new ArgumentOutOfRangeException($"Enum {c.DateTimeGrouping} value {(int)c.DateTimeGrouping} not handled!");
            }
        }

        private static void ApplyAggregation<T>(List<T> entities, XrmAttributeExpression column, T aggregate) where T : Entity
        {
            Type valueType;
            switch (column.AggregateType)
            {
                case XrmAggregateType.Avg:
                {
                    valueType = GetFirstValueType(entities, column);
                    if (valueType == typeof(Money))
                    {
                        var average = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Average(e => e.GetAttributeValue<Money>(column.AttributeName)?.Value);
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, new Money(average ?? 0m));
                    }
                    else
                    {
                        var average = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Average(e => e.GetAttributeValue<dynamic>(column.AttributeName));
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, PrimitiveTypeConverter.ConvertToPrimitiveType(average, valueType));
                    }

                    break;
                }
                case XrmAggregateType.Count:
                    aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, entities.Count);
                    break;

                case XrmAggregateType.CountColumn:
                    var countColumn = entities.Count(e => e.GetAttributeValue<object>(column.AttributeName) != null);
                    aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, countColumn);
                    break;

                case XrmAggregateType.Max:
                    valueType = GetFirstValueType(entities, column);
                    if (valueType == typeof(Money))
                    {
                        var max = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Max(e => e.GetAttributeValue<Money>(column.AttributeName)?.Value);
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, new Money(max ?? 0m));
                    }
                    else
                    {
                        var max = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Max(e => e.GetAttributeValue<dynamic>(column.AttributeName));
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, PrimitiveTypeConverter.ConvertToPrimitiveType(max, valueType));
                    }

                    break;
                case XrmAggregateType.Min:
                    valueType = GetFirstValueType(entities, column);
                    if (valueType == typeof(Money))
                    {
                        var min = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Min(e => e.GetAttributeValue<Money>(column.AttributeName)?.Value);
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, new Money(min ?? 0m));
                    }
                    else
                    {
                        var min = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Min(e => e.GetAttributeValue<dynamic>(column.AttributeName));
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, PrimitiveTypeConverter.ConvertToPrimitiveType(min, valueType));
                    }
                    break;
                case XrmAggregateType.Sum:
                    valueType = GetFirstValueType(entities, column);
                    if (valueType == typeof(Money))
                    {
                        var sum = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Sum(e => e.GetAttributeValue<Money>(column.AttributeName)?.Value);
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, new Money(sum ?? 0m));
                    }
                    else
                    {
                        var sum = entities.Where(e => e.GetAttributeValue<object>(column.AttributeName) != null).Sum(e => e.GetAttributeValue<dynamic>(column.AttributeName));
                        aggregate[column.Alias] = new AliasedValue(aggregate.LogicalName, column.AttributeName, PrimitiveTypeConverter.ConvertToPrimitiveType(sum, valueType));
                    }
                    break;
                case XrmAggregateType.None:
                    foreach(var entity in entities)
                    {
                        var aliasedValue = new AliasedValue(aggregate.LogicalName, column.AttributeName, entity[column.AttributeName]);
                        entity[column.Alias] = aliasedValue;
                        aggregate[column.Alias] = aliasedValue;
                    }
                    break;
            }
        }

        private static Type GetFirstValueType<T>(List<T> entities, XrmAttributeExpression column) where T : Entity
        {
            return entities.FirstOrDefault(e => e.Attributes.ContainsKey(column.AttributeName) && e[column.AttributeName] != null)?[column.AttributeName].GetType();
        }
#endif
    }
}
