using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

// ReSharper disable once CheckNamespace
namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        #region Process FetchXml

        public static QueryExpression ConvertFetchToQueryExpression(LocalCrmDatabaseOrganizationService service, FetchType fe)
        {
            var fetchEntity = (FetchEntityType)fe.Items[0];
            var qe = MapToQueryExpression(fe, fetchEntity);
            var idName = EntityHelper.GetIdAttributeName(service.GetType(fetchEntity.name));
            var entityLink = qe.AddLink(fetchEntity.name, idName, idName);
            foreach (dynamic item in fetchEntity.Items)
            {
                ProcessFetchXmlItem(service, entityLink, item);
            }
            qe.ColumnSet.AddColumns(entityLink.Columns.Columns.ToArray());
            qe.ColumnSet.AttributeExpressions.AddRange(entityLink.Columns.AttributeExpressions);
            entityLink.Columns.Columns.Clear();
            qe.LinkEntities.Clear();
            qe.Criteria = entityLink.LinkCriteria;
            qe.LinkEntities.AddRange(entityLink.LinkEntities);

            return qe;
        }

        private static QueryExpression MapToQueryExpression(FetchType fe, FetchEntityType fetchEntity)
        {
            var qe = new QueryExpression(fetchEntity.name);
            if (!string.IsNullOrWhiteSpace(fe.top))
            {
                qe.TopCount = int.Parse(fe.top);
            }
            if (!fe.distinctSpecified)
            {
                qe.Distinct = fe.distinct;
            }
            qe.NoLock = fe.nolock;
            if (!string.IsNullOrWhiteSpace(fe.page))
            {
                qe.PageInfo = new PagingInfo
                {
                    PageNumber = int.Parse(fe.page),
                    PagingCookie = fe.pagingcookie,
                };

                if (!string.IsNullOrWhiteSpace(fe.count))
                {
                    qe.PageInfo.Count = int.Parse(fe.count);
                }
            }
            var orders = ((FetchEntityType?)fe.Items.FirstOrDefault())?.Items.OfType<FetchOrderType>().ToList();
            if (orders != null)
            {
                foreach(var order in orders)
                {
                    qe.AddOrder(order.attribute, order.descending ? OrderType.Descending : OrderType.Ascending);
                }
            }
            return qe;
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ProcessFetchXmlItem(LocalCrmDatabaseOrganizationService service, LinkEntity entityLink, FetchAttributeType attribute)
        {
            if (!string.IsNullOrWhiteSpace(attribute.alias))
            {
                var aggregates = entityLink.Columns.AttributeExpressions;
                aggregates.Add(
                    new XrmAttributeExpression(
                        attributeName: attribute.name,
                        alias: attribute.alias,
                        aggregateType: GetAggregateType(attribute))
                    {
                        DateTimeGrouping = GetDateGrouping(attribute),
                        HasGroupBy = attribute.groupbySpecified
                    }
                );
                return;
            }

            if (attribute is { aggregateSpecified: false, groupbySpecified: false })
            {
                if (!entityLink.Columns.Columns.Contains(attribute.name))
                {
                    entityLink.Columns.AddColumn(attribute.name);
                }
            }
        }

        private static XrmAggregateType GetAggregateType(FetchAttributeType att)
        {
            if (!att.aggregateSpecified)
            {
                return XrmAggregateType.None;
            }

            return att.aggregate switch
            {
                AggregateType.count => XrmAggregateType.Count,
                AggregateType.countcolumn => XrmAggregateType.CountColumn,
                AggregateType.sum => XrmAggregateType.Sum,
                AggregateType.avg => XrmAggregateType.Avg,
                AggregateType.min => XrmAggregateType.Min,
                AggregateType.max => XrmAggregateType.Max,
                _ => throw new NotImplementedException(att.aggregate.ToString())
            };
        }

        private static XrmDateTimeGrouping GetDateGrouping(FetchAttributeType att)
        {
            if (!att.dategroupingSpecified)
            {
                return XrmDateTimeGrouping.None;
            }

            return att.dategrouping switch
            {
                DateGroupingType.day => XrmDateTimeGrouping.Day,
                DateGroupingType.week => XrmDateTimeGrouping.Week,
                DateGroupingType.month => XrmDateTimeGrouping.Month,
                DateGroupingType.quarter => XrmDateTimeGrouping.Quarter,
                DateGroupingType.year => XrmDateTimeGrouping.Year,
                DateGroupingType.fiscalperiod => XrmDateTimeGrouping.FiscalPeriod,
                DateGroupingType.fiscalyear => XrmDateTimeGrouping.FiscalYear,
                _ => throw new NotImplementedException(att.dategrouping.ToString())
            };
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ProcessFetchXmlItem(LocalCrmDatabaseOrganizationService service, LinkEntity entityLink, filter filter)
        {
            var linkedEntitiesByAliasName = entityLink.LinkEntities.Where(l => l.EntityAlias != null).ToDictionary(e => e.EntityAlias);
             entityLink.LinkCriteria.AddFilter(
                GetFilterExpression(service, service.GetType(entityLink.LinkToEntityName), linkedEntitiesByAliasName, filter));
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ProcessFetchXmlItem(LocalCrmDatabaseOrganizationService service, LinkEntity entityLink, FetchLinkEntityType link)
        {
            var joinType = link.linktype == "outer" ? JoinOperator.LeftOuter : JoinOperator.Inner;

            var childLink = new LinkEntity(entityLink.LinkFromEntityName, link.name, link.to, link.@from, joinType)
            {
                EntityAlias = link.alias,
            };
            entityLink.LinkEntities.Add(childLink);
            var items = link.Items;
            if (items == null)
            {
                return;
            }

            foreach (dynamic item in items)
            {
                ProcessFetchXmlItem(service, childLink, item);
            }
        }

        // ReSharper disable UnusedParameter.Local
        // ReSharper disable once UnusedMember.Local
        private static void ProcessFetchXmlItem(LocalCrmDatabaseOrganizationService service, LinkEntity entityLink, FetchOrderType order)
        {
            // Ignore
        }


        private static void ProcessFetchXmlItem(LocalCrmDatabaseOrganizationService service, LinkEntity entityLink, object item)
        {
            throw new NotSupportedException("Item type " + item.GetType().Name + " Not Supported");
        }
        // ReSharper restore UnusedParameter.Local

        private static FilterExpression GetFilterExpression(LocalCrmDatabaseOrganizationService service, Type entityType, Dictionary<String, LinkEntity> links, filter filter)
        {
            var criteria = new FilterExpression(filter.type == filterType.and ? LogicalOperator.And : LogicalOperator.Or);
            foreach (var item in filter.Items)
            {
                AddToFilter(service, entityType, links, criteria, (dynamic)item);
            }
            return criteria;
        }

        // ReSharper disable UnusedParameter.Local
        private static void AddToFilter(LocalCrmDatabaseOrganizationService service, Type entityType, Dictionary<String, LinkEntity> links, FilterExpression criteria, object item)
        // ReSharper restore UnusedParameter.Local
        {
            // Do nothing.  Should this error?
        }

        private static void AddToFilter(LocalCrmDatabaseOrganizationService service, Type entityType, Dictionary<String, LinkEntity> links, FilterExpression criteria, filter item)
        {
            criteria.AddFilter(GetFilterExpression(service, entityType, links, item));
        }

        private static void AddToFilter(LocalCrmDatabaseOrganizationService service, Type entityType, Dictionary<String, LinkEntity> links, FilterExpression criteria, condition condition)
        {
            string? entityName = null;
            if (!string.IsNullOrWhiteSpace(condition.entityname))
            {
                entityName = condition.entityname;
                var logicalName = links.TryGetValue(condition.entityname, out var linkRef)
                    ? linkRef.LinkToEntityName
                    : condition.entityname;
                entityType = service.GetType(logicalName);
            }
            var property = entityType.GetProperty(condition.attribute) ??
                           entityType.GetProperty(condition.attribute, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                throw new Exception("No Property '" + condition.attribute + "' found for entity type: " + entityType.Name);
            }

            var op = Convert(condition.@operator);
            criteria.AddCondition(entityName, condition.attribute, op, GetConditionValue(condition, property.PropertyType, op));
        }

        private static object?[]? GetConditionValue(condition condition, Type attributeType, ConditionOperator op)
        {
            if (op == ConditionOperator.Null || op == ConditionOperator.NotNull)
            {
                return null;
            }

            object?[]? value;
            if (attributeType == typeof (Guid) || attributeType == typeof (EntityReference))
            {
                value = ParseValue(condition, Guid.Parse);
            }
            else if (attributeType == typeof (OptionSetValue))
            {
                value = ParseValue(condition, int.Parse);
            }
            else if (attributeType.IsGenericType && attributeType.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                attributeType = attributeType.GetGenericArguments()[0];
                if (condition.value == string.Empty)
                {
                    value = null;
                }
                else if (attributeType == typeof (int) || attributeType.IsEnum)
                {
                    value = ParseNullableValue(condition, int.Parse);
                }
                else if (attributeType == typeof (DateTime))
                {
                    value = ParseNullableValue(condition, DateTime.Parse);
                }
                else if (attributeType == typeof (Decimal))
                {
                    value = ParseNullableValue(condition, Decimal.Parse);
                }
                else if (attributeType == typeof (Guid))
                {
                    value = ParseNullableValue(condition, Guid.Parse);
                }
                else
                {
                    value = ParseNullableValue(condition, v => v);
                }
            }
            else
            {
                value = ParseValue(condition, v => v);
            }
            return value;
        }

        private static object?[] ParseNullableValue<T>(condition condition, Func<string, T> parse) 
        {
            var count = condition.Items == null! ? 0 : condition.Items.Count();
            if (count > 0 && condition.value != null)
            {
                throw new ArgumentException("Condition contains both Items and a value.  Not sure how CRM handles this...  Error for now", "condition");
            }
            if (count > 0)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return condition.Items!.Select(i => i.Value == null ? (object?)null : parse(i.Value)).ToArray();
            }
            else
            {
                return [condition.value == null ? null : parse(condition.value)];
            }
        }

        private static object?[] ParseValue<T>(condition condition, Func<string, T> parse)
        {
            var count = condition.Items == null! ? 0 : condition.Items.Count();
            if (count > 0 && condition.value != null)
            {
                throw new ArgumentException("Condition contains both Items and a value.  Not sure how CRM handles this...  Error for now", "condition");
            }
            if (count > 0)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return condition.Items!.Select(i => (object?)parse(i.Value)).ToArray();
            }
            else
            {
                return [parse(condition.value ?? string.Empty)];
            }
        }

        private static ConditionOperator Convert(@operator op)
        {
            switch (op)
            {
                case @operator.eq:
                    return ConditionOperator.Equal;
                case @operator.neq:
                case @operator.ne:
                    return ConditionOperator.NotEqual;
                case @operator.gt:
                    return ConditionOperator.GreaterThan;
                case @operator.ge:
                    return ConditionOperator.GreaterEqual;
                case @operator.le:
                    return ConditionOperator.LessEqual;
                case @operator.lt:
                    return ConditionOperator.LessThan;
                case @operator.like:
                    return ConditionOperator.Like;
                case @operator.notlike:
                    return ConditionOperator.NotLike;
                case @operator.@in:
                    return ConditionOperator.In;
                case @operator.notin:
                    return ConditionOperator.NotIn;
                case @operator.between:
                    return ConditionOperator.Between;
                case @operator.notbetween:
                    return ConditionOperator.NotBetween;
                case @operator.@null:
                    return ConditionOperator.Null;
                case @operator.notnull:
                    return ConditionOperator.NotNull;
                case @operator.yesterday:
                    return ConditionOperator.Yesterday;
                case @operator.today:
                    return ConditionOperator.Today;
                case @operator.tomorrow:
                    return ConditionOperator.Tomorrow;
                case @operator.lastsevendays:
                    return ConditionOperator.Last7Days;
                case @operator.nextsevendays:
                    return ConditionOperator.Next7Days;
                case @operator.lastweek:
                    return ConditionOperator.LastWeek;
                case @operator.thisweek:
                    return ConditionOperator.ThisWeek;
                case @operator.nextweek:
                    return ConditionOperator.NextWeek;
                case @operator.lastmonth:
                    return ConditionOperator.LastMonth;
                case @operator.thismonth:
                    return ConditionOperator.ThisMonth;
                case @operator.nextmonth:
                    return ConditionOperator.NextMonth;
                case @operator.@on:
                    return ConditionOperator.On;
                case @operator.onorbefore:
                    return ConditionOperator.OnOrBefore;
                case @operator.onorafter:
                    return ConditionOperator.OnOrAfter;
                case @operator.lastyear:
                    return ConditionOperator.LastYear;
                case @operator.thisyear:
                    return ConditionOperator.ThisYear;
                case @operator.nextyear:
                    return ConditionOperator.NextYear;
                case @operator.lastxhours:
                    return ConditionOperator.LastXHours;
                case @operator.nextxhours:
                    return ConditionOperator.NextXHours;
                case @operator.lastxdays:
                    return ConditionOperator.LastXDays;
                case @operator.nextxdays:
                    return ConditionOperator.NextXDays;
                case @operator.lastxweeks:
                    return ConditionOperator.LastXWeeks;
                case @operator.nextxweeks:
                    return ConditionOperator.NextXWeeks;
                case @operator.lastxmonths:
                    return ConditionOperator.LastXMonths;
                case @operator.nextxmonths:
                    return ConditionOperator.NextXMonths;
                case @operator.olderthanxmonths:
                    return ConditionOperator.OlderThanXMonths;
                case @operator.lastxyears:
                    return ConditionOperator.LastXYears;
                case @operator.nextxyears:
                    return ConditionOperator.NextXYears;
                case @operator.equserid:
                    return ConditionOperator.EqualUserId;
                case @operator.neuserid:
                    return ConditionOperator.NotEqualUserId;
                case @operator.equserteams:
                    return ConditionOperator.EqualUserTeams;
                case @operator.eqbusinessid:
                    return ConditionOperator.EqualBusinessId;
                case @operator.nebusinessid:
                    return ConditionOperator.NotEqualBusinessId;
                case @operator.equserlanguage:
                    return ConditionOperator.EqualUserLanguage;
                case @operator.thisfiscalyear:
                    return ConditionOperator.ThisFiscalYear;
                case @operator.thisfiscalperiod:
                    return ConditionOperator.ThisFiscalPeriod;
                case @operator.nextfiscalyear:
                    return ConditionOperator.NextFiscalYear;
                case @operator.nextfiscalperiod:
                    return ConditionOperator.NextFiscalPeriod;
                case @operator.lastfiscalyear:
                    return ConditionOperator.LastFiscalYear;
                case @operator.lastfiscalperiod:
                    return ConditionOperator.LastFiscalPeriod;
                case @operator.lastxfiscalyears:
                    return ConditionOperator.LastXFiscalYears;
                case @operator.lastxfiscalperiods:
                    return ConditionOperator.LastXFiscalPeriods;
                case @operator.nextxfiscalyears:
                    return ConditionOperator.NextXFiscalYears;
                case @operator.nextxfiscalperiods:
                    return ConditionOperator.NextXFiscalPeriods;
                case @operator.infiscalyear:
                    return ConditionOperator.InFiscalYear;
                case @operator.infiscalperiod:
                    return ConditionOperator.InFiscalPeriod;
                case @operator.infiscalperiodandyear:
                    return ConditionOperator.InFiscalPeriodAndYear;
                case @operator.inorbeforefiscalperiodandyear:
                    return ConditionOperator.InOrBeforeFiscalPeriodAndYear;
                case @operator.inorafterfiscalperiodandyear:
                    return ConditionOperator.InOrAfterFiscalPeriodAndYear;
                case @operator.beginswith:
                    return ConditionOperator.BeginsWith;
                case @operator.notbeginwith:
                    return ConditionOperator.DoesNotBeginWith;
                case @operator.endswith:
                    return ConditionOperator.EndsWith;
                case @operator.notendwith:
                    return ConditionOperator.DoesNotEndWith;
                default:
                    throw new NotImplementedException(op.ToString());
            }
        }

        #endregion Process FetchXml

        #region Convert Query Expression To Fetch Xml

        public static string ConvertQueryExpressionToFetchXml(QueryExpression qe)
        { 
            var items = new List<object>();

            // Add Column Set
            items.AddRange(qe.ColumnSet.Columns.Select(c => new FetchAttributeType{ name = c }));

            // Add Filters
            items.Add(CreateFilter(qe.Criteria));

            // Add Links
            items.AddRange(qe.LinkEntities.Select(CreateLink));

            // FetchOrderType order
            items.AddRange(qe.Orders.Select(c => new FetchOrderType{ attribute = c.AttributeName, descending = c.OrderType == OrderType.Descending}));

            var fetch = new FetchType
            {
                Items = new Object []{ 
                    new FetchEntityType{
                        name = qe.EntityName,
                        Items = items.ToArray()
                    }
                },
                count = qe.TopCount.HasValue ? qe.TopCount.ToString() : null,
            };

            var xmlserializer = new XmlSerializer(typeof(FetchType));
            using (var stringWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter,new XmlWriterSettings{Indent = true}))
                {
                    xmlserializer.Serialize(writer, fetch);
                    return stringWriter.ToString();
                }    
            }
        }

        private static FetchLinkEntityType CreateLink(LinkEntity link)
        {
            var items = new List<Object>();
            items.AddRange(link.LinkEntities.Select(CreateLink));
            items.Add(CreateFilter(link.LinkCriteria));

            return new FetchLinkEntityType()
            {
                alias = link.EntityAlias,
                from = link.LinkFromAttributeName,
                name = link.LinkToEntityName,
                to = link.LinkToAttributeName,
                linktype = Convert(link.JoinOperator),
                Items = items.ToArray()
            };
        }

        private static filter CreateFilter(FilterExpression fe)
        {
            var items = new List<object>();
            items.AddRange(fe.Conditions.Select(CreateCondition));

            items.AddRange(fe.Filters.Select(CreateFilter));

            return new filter
            {
                type = fe.FilterOperator == LogicalOperator.Or ? filterType.or : filterType.and,
                isquickfindfields = fe.IsQuickFindFilter,
                Items = items.ToArray(),
            };
        }

        private static condition CreateCondition(ConditionExpression condition)
        {
            return new condition
            {
                alias = condition.EntityName,
                column = condition.AttributeName,
                @operator = Convert(condition.Operator),
                value = condition.Values.ToString(),
            };
        }

        private static string Convert(JoinOperator op) {
            switch (op)
            {
                case JoinOperator.Inner:
                    return "inner";
                case JoinOperator.LeftOuter:
                    return "outer";
                case JoinOperator.Natural:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        private static @operator Convert(ConditionOperator op)
        {
            switch (op)
            {
                				case ConditionOperator.Equal:
                    return @operator.eq;
                case ConditionOperator.NotEqual:
                    return @operator.neq;
                case ConditionOperator.GreaterThan:
                    return @operator.gt;
                case ConditionOperator.GreaterEqual:
                    return @operator.ge;
                case ConditionOperator.LessEqual:
                    return @operator.le;
                case ConditionOperator.LessThan:
                    return @operator.lt;
                case ConditionOperator.Like:
                    return @operator.like;
                case ConditionOperator.NotLike:
                    return @operator.notlike;
                case ConditionOperator.In:
                    return @operator.@in;
                case ConditionOperator.NotIn:
                    return @operator.notin;
                case ConditionOperator.Between:
                    return @operator.between;
                case ConditionOperator.NotBetween:
                    return @operator.notbetween;
                case ConditionOperator.Null:
                    return @operator.@null;
                case ConditionOperator.NotNull:
                    return @operator.notnull;
                case ConditionOperator.Yesterday:
                    return @operator.yesterday;
                case ConditionOperator.Today:
                    return @operator.today;
                case ConditionOperator.Tomorrow:
                    return @operator.tomorrow;
                case ConditionOperator.Last7Days:
                    return @operator.lastsevendays;
                case ConditionOperator.Next7Days:
                    return @operator.nextsevendays;
                case ConditionOperator.LastWeek:
                    return @operator.lastweek;
                case ConditionOperator.ThisWeek:
                    return @operator.thisweek;
                case ConditionOperator.NextWeek:
                    return @operator.nextweek;
                case ConditionOperator.LastMonth:
                    return @operator.lastmonth;
                case ConditionOperator.ThisMonth:
                    return @operator.thismonth;
                case ConditionOperator.NextMonth:
                    return @operator.nextmonth;
                case ConditionOperator.On:
                    return @operator.@on;
                case ConditionOperator.OnOrBefore:
                    return @operator.onorbefore;
                case ConditionOperator.OnOrAfter:
                    return @operator.onorafter;
                case ConditionOperator.LastYear:
                    return @operator.lastyear;
                case ConditionOperator.ThisYear:
                    return @operator.thisyear;
                case ConditionOperator.NextYear:
                    return @operator.nextyear;
                case ConditionOperator.LastXHours:
                    return @operator.lastxhours;
                case ConditionOperator.NextXHours:
                    return @operator.nextxhours;
                case ConditionOperator.LastXDays:
                    return @operator.lastxdays;
                case ConditionOperator.NextXDays:
                    return @operator.nextxdays;
                case ConditionOperator.LastXWeeks:
                    return @operator.lastxweeks;
                case ConditionOperator.NextXWeeks:
                    return @operator.nextxweeks;
                case ConditionOperator.LastXMonths:
                    return @operator.lastxmonths;
                case ConditionOperator.NextXMonths:
                    return @operator.nextxmonths;
                case ConditionOperator.OlderThanXMonths:
                    return @operator.olderthanxmonths;
                case ConditionOperator.LastXYears:
                    return @operator.lastxyears;
                case ConditionOperator.NextXYears:
                    return @operator.nextxyears;
                case ConditionOperator.EqualUserId:
                    return @operator.equserid;
                case ConditionOperator.NotEqualUserId:
                    return @operator.neuserid;
                case ConditionOperator.EqualUserTeams:
                    return @operator.equserteams;
                case ConditionOperator.EqualBusinessId:
                    return @operator.eqbusinessid;
                case ConditionOperator.NotEqualBusinessId:
                    return @operator.nebusinessid;
                case ConditionOperator.EqualUserLanguage:
                    return @operator.equserlanguage;
                case ConditionOperator.ThisFiscalYear:
                    return @operator.thisfiscalyear;
                case ConditionOperator.ThisFiscalPeriod:
                    return @operator.thisfiscalperiod;
                case ConditionOperator.NextFiscalYear:
                    return @operator.nextfiscalyear;
                case ConditionOperator.NextFiscalPeriod:
                    return @operator.nextfiscalperiod;
                case ConditionOperator.LastFiscalYear:
                    return @operator.lastfiscalyear;
                case ConditionOperator.LastFiscalPeriod:
                    return @operator.lastfiscalperiod;
                case ConditionOperator.LastXFiscalYears:
                    return @operator.lastxfiscalyears;
                case ConditionOperator.LastXFiscalPeriods:
                    return @operator.lastxfiscalperiods;
                case ConditionOperator.NextXFiscalYears:
                    return @operator.nextxfiscalyears;
                case ConditionOperator.NextXFiscalPeriods:
                    return @operator.nextxfiscalperiods;
                case ConditionOperator.InFiscalYear:
                    return @operator.infiscalyear;
                case ConditionOperator.InFiscalPeriod:
                    return @operator.infiscalperiod;
                case ConditionOperator.InFiscalPeriodAndYear:
                    return @operator.infiscalperiodandyear;
                case ConditionOperator.InOrBeforeFiscalPeriodAndYear:
                    return @operator.inorbeforefiscalperiodandyear;
                case ConditionOperator.InOrAfterFiscalPeriodAndYear:
                    return @operator.inorafterfiscalperiodandyear;
                case ConditionOperator.BeginsWith:
                    return @operator.beginswith;
                case ConditionOperator.DoesNotBeginWith:
                    return @operator.notbeginwith;
                case ConditionOperator.EndsWith:
                    return @operator.endswith;
                case ConditionOperator.DoesNotEndWith:
                    return @operator.notendwith;
                default:
                    throw new NotImplementedException(op.ToString());
            }
        }

        #endregion Convert Query Expression To Fetch Xml
    }
}
