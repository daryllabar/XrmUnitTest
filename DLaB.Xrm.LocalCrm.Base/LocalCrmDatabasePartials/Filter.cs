using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {

        private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, FilterExpression filter, QueryContext context) where T : Entity
        {
            return query.Where(e => EvaluateFilter(e, filter, context));
        }

        /// <summary>
        /// Returns true if the entity satisfies all of the constraints of the filter, otherwise, false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool EvaluateFilter<T>(T entity, FilterExpression filter, QueryContext context) where T : Entity
        {
            if (entity == null) { return true; } // This should only happen for Left Outer Joins

            bool matchesFilter;
            if (filter.FilterOperator == LogicalOperator.And)
            {
                matchesFilter = filter.Conditions.All(c => ConditionIsTrue(entity, c, context)) && filter.Filters.All(f => EvaluateFilter(entity, f, context));
            }
            else
            {
                matchesFilter = filter.Conditions.Any(c => ConditionIsTrue(entity, c, context)) || filter.Filters.Any(f => EvaluateFilter(entity, f, context));
            }

            return matchesFilter;
        }

        private static readonly Dictionary<ConditionOperator, int> ExpectedValuesByConditionOperator = new Dictionary<ConditionOperator, int>
        {

            { ConditionOperator.BeginsWith, 1 },
            { ConditionOperator.Between, 2 },
            { ConditionOperator.ChildOf, 1 },
            { ConditionOperator.Contains, 1 },
            { ConditionOperator.DoesNotBeginWith, 1 },
            { ConditionOperator.DoesNotContain, 1 },
            { ConditionOperator.DoesNotEndWith, 1 },
            { ConditionOperator.EndsWith, 1 },
            { ConditionOperator.Equal, 1 },
            { ConditionOperator.EqualBusinessId, 0 },
            { ConditionOperator.EqualUserId, 0 },
            { ConditionOperator.EqualUserLanguage, 1 },
            { ConditionOperator.EqualUserOrUserTeams, 0 },
            { ConditionOperator.EqualUserTeams, 0 },
            { ConditionOperator.GreaterEqual, 1 },
            { ConditionOperator.GreaterThan, 1 },
            { ConditionOperator.InFiscalPeriod, 0 },
            { ConditionOperator.InFiscalPeriodAndYear, 0 },
            { ConditionOperator.InFiscalYear, 0 },
            { ConditionOperator.InOrAfterFiscalPeriodAndYear, 0 },
            { ConditionOperator.InOrBeforeFiscalPeriodAndYear, 0 },
            { ConditionOperator.Last7Days, 0 },
            { ConditionOperator.LastFiscalPeriod, 0 },
            { ConditionOperator.LastFiscalYear, 0 },
            { ConditionOperator.LastMonth, 0 },
            { ConditionOperator.LastWeek, 0 },
            { ConditionOperator.LastXDays, 1 },
            { ConditionOperator.LastXFiscalPeriods, 1 },
            { ConditionOperator.LastXFiscalYears, 1 },
            { ConditionOperator.LastXHours, 1 },
            { ConditionOperator.LastXMonths, 1 },
            { ConditionOperator.LastXWeeks, 1 },
            { ConditionOperator.LastXYears, 1 },
            { ConditionOperator.LastYear, 0 },
            { ConditionOperator.LessEqual, 1 },
            { ConditionOperator.LessThan, 1 },
            { ConditionOperator.Like, 1 },
            { ConditionOperator.Mask, 1 },
            { ConditionOperator.MasksSelect, 1 },
            { ConditionOperator.Next7Days, 0 },
            { ConditionOperator.NextFiscalPeriod, 0 },
            { ConditionOperator.NextFiscalYear, 0 },
            { ConditionOperator.NextMonth, 0 },
            { ConditionOperator.NextWeek, 0 },
            { ConditionOperator.NextXDays, 1 },
            { ConditionOperator.NextXFiscalPeriods, 1 },
            { ConditionOperator.NextXFiscalYears, 1 },
            { ConditionOperator.NextXHours, 1 },
            { ConditionOperator.NextXMonths, 1 },
            { ConditionOperator.NextXWeeks, 1 },
            { ConditionOperator.NextXYears, 1 },
            { ConditionOperator.NextYear, 0 },
            { ConditionOperator.NotBetween, 2 },
            { ConditionOperator.NotEqual, 1 },
            { ConditionOperator.NotEqualBusinessId, 0 },
            { ConditionOperator.NotEqualUserId, 0 },
            { ConditionOperator.NotLike, 1 },
            { ConditionOperator.NotMask, 1 },
            { ConditionOperator.NotNull, 0 },
            { ConditionOperator.NotOn, 1 },
            { ConditionOperator.Null, 0 },
            { ConditionOperator.OlderThanXMonths, 0 },
            { ConditionOperator.On, 1 },
            { ConditionOperator.OnOrAfter, 1 },
            { ConditionOperator.OnOrBefore, 1 },
            { ConditionOperator.ThisFiscalPeriod, 0 },
            { ConditionOperator.ThisFiscalYear, 0 },
            { ConditionOperator.ThisMonth, 0 },
            { ConditionOperator.ThisWeek, 0 },
            { ConditionOperator.ThisYear, 0 },
            { ConditionOperator.Today, 0 },
            { ConditionOperator.Tomorrow, 0 },
            { ConditionOperator.Yesterday, 0 },
#if !PRE_KEYATTRIBUTE // Values introduced in 2015
            { ConditionOperator.Above, 1 },
            { ConditionOperator.AboveOrEqual, 1 },
            { ConditionOperator.EqualUserOrUserHierarchy, 0 },
            { ConditionOperator.EqualUserOrUserHierarchyAndTeams, 0 },
            { ConditionOperator.NotUnder, 1 },
            { ConditionOperator.OlderThanXDays, 1 },
            { ConditionOperator.OlderThanXHours, 1 },
            { ConditionOperator.OlderThanXMinutes, 1 },
            { ConditionOperator.OlderThanXWeeks, 1 },
            { ConditionOperator.OlderThanXYears, 1 },
            { ConditionOperator.Under, 1 },
            { ConditionOperator.UnderOrEqual, 1 },
#endif
        };

        private static bool ConditionIsTrue<T>(T entity, ConditionExpression condition, QueryContext context) where T : Entity
        {
            // Date Time Details: https://community.dynamics.com/crm/b/gonzaloruiz/archive/2012/07/29/date-and-time-operators-in-crm-explained

            int days;
            bool value;
            DateTime date;
            AssertExpectedNumberOfValues(condition);

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
                    var result = Compare(entity, name, condition.Values[0]);
                    value = result >= -1 && result < 0;
                    break;
                case ConditionOperator.GreaterEqual:
                    value = Compare(entity, name, condition.Values[0]) >= 0;
                    break;
                case ConditionOperator.LessEqual:
                    result = Compare(entity, name, condition.Values[0]);
                    value = result >= -1 && result <= 0;
                    break;
                case ConditionOperator.Like:
                    var str = GetString(entity, name);
                    if (str == null)
                    {
                        value = condition.Values[0] == null;
                    }
                    else
                    {
                        var likeCondition = (string)condition.Values[0];
                        // http://stackoverflow.com/questions/5417070/c-sharp-version-of-sql-like
                        value = new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(likeCondition.ToUpper(), ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(str.ToUpper());
                    }
                    break;
                case ConditionOperator.NotLike:
                    value = !ConditionIsTrue(entity, new ConditionExpression(condition.EntityName, condition.AttributeName, ConditionOperator.Like), context);
                    break;
                case ConditionOperator.In:
                    value = condition.Values.Any(v => Compare(entity, name, v) == 0);
                    break;
                case ConditionOperator.NotIn:
                    value = condition.Values.All(v => Compare(entity, name, v) != 0);
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
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date, context);
                    break;
                case ConditionOperator.Today:
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), context);
                    break;
                case ConditionOperator.Tomorrow:
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2), context);
                    break;
                case ConditionOperator.Last7Days:
                    condition.Operator = ConditionOperator.LastXDays;
                    condition.Values.Add(7);
                    value = ConditionIsTrue(entity, condition, context);
                    break;
                case ConditionOperator.Next7Days:
                    condition.Operator = ConditionOperator.NextXDays;
                    condition.Values.Add(7);
                    value = ConditionIsTrue(entity, condition, context);
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
                case ConditionOperator.On:
                    date = condition.GetDateTimeValueFromDateOrString().Date;
                    var attributeDate = entity.GetAttributeValue<DateTime?>(name);
                    value = attributeDate.HasValue
                        && date == attributeDate.Value.Date;
                    break;
                case ConditionOperator.OnOrBefore:
                    date = condition.GetDateTimeValueFromDateOrString().Date;
                    if(date != DateTime.MaxValue)
                    {
                        date = date.AddDays(1);
                    }
                    value = IsBetween(entity, condition, DateTime.MinValue, date, context);
                    break;
                case ConditionOperator.OnOrAfter:
                    date = condition.GetDateTimeValueFromDateOrString().Date;
                    value = IsBetween(entity, condition, date, DateTime.MaxValue, context);
                    break;
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

                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(-1d * days), DateTime.UtcNow.AddDays(1).Date, context);
                    break;
                case ConditionOperator.NextXDays:
                    days = condition.GetIntValueFromIntOrString();
                    if (days <= 0)
                    {
                        throw CrmExceptions.GetConditionValueGreaterThan0Exception();
                    }
                    value = IsBetween(entity, condition, DateTime.UtcNow, DateTime.UtcNow.Date.AddDays(days + 1), context);
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
                case ConditionOperator.EqualUserId:
                    value = Compare(entity, name, context.UserId) == 0;
                    break;
                case ConditionOperator.NotEqualUserId:
                    value = Compare(entity, name, context.UserId) != 0;
                    break;
                case ConditionOperator.EqualBusinessId:
                    value = Compare(entity, name, context.BusinessUnitId) == 0;
                    break;
                case ConditionOperator.NotEqualBusinessId:
                    value = Compare(entity, name, context.BusinessUnitId) != 0;
                    break;
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
                case ConditionOperator.BeginsWith:
                    var beginsWithStr = GetString(entity, name);
                    if (beginsWithStr == null)
                    {
                        value = condition.Values[0] == null;
                    }
                    else
                    {
                        value = beginsWithStr.StartsWith((string)condition.Values[0]);
                    }
                    break;
                case ConditionOperator.DoesNotBeginWith:
                    condition.Operator = ConditionOperator.BeginsWith;
                    value = !ConditionIsTrue(entity, condition, context);
                    break;
                case ConditionOperator.EndsWith:
                    var endsWithStr = GetString(entity, name);
                    if (endsWithStr == null)
                    {
                        value = condition.Values[0] == null;
                    }
                    else
                    {
                        value = endsWithStr.EndsWith((string)condition.Values[0]);
                    }
                    break;
                case ConditionOperator.DoesNotEndWith:
                    condition.Operator = ConditionOperator.EndsWith;
                    value = !ConditionIsTrue(entity, condition, context);
                    break;
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
#if !PRE_MULTISELECT
                case ConditionOperator.ContainValues:
                    var collection = GetOptionSetValueCollection(entity, name);
                    value = collection != null && condition.Values.All(v => collection.Contains(new OptionSetValue((int)v)));
                    break;
                case ConditionOperator.DoesNotContainValues:
                    condition.Operator = ConditionOperator.ContainValues;
                    value = !ConditionIsTrue(entity, condition, context);
                    break;
#endif
                default:
                    throw new NotImplementedException(condition.Operator.ToString());
            }
            return value;
        }

        private static void AssertExpectedNumberOfValues(ConditionExpression condition)
        {
            if (!ExpectedValuesByConditionOperator.TryGetValue(condition.Operator, out var expectedCount))
            {
                return;
            }

            if (condition.Values.Count != expectedCount)
            {
                throw CrmExceptions.GetConditionOperatorRequiresValuesException(condition, expectedCount);
            }

            if (expectedCount == 1 && condition.Values[0] == null)
            {
                condition = new ConditionExpression(condition.AttributeName, condition.Operator);
                throw CrmExceptions.GetConditionOperatorRequiresValuesException(condition, expectedCount);
            }
        }

        /// <summary>
        /// Determines whether the condition specified entity is between.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="context">The context for the current query.</param>
        /// <param name="inclusiveStart">if set to <c>true</c> [inclusive start].</param>
        /// <param name="inclusiveEnd">if set to <c>true</c> [inclusive end].</param>
        /// <returns></returns>
        private static bool IsBetween<T>(T entity, ConditionExpression condition, DateTime start, DateTime end, QueryContext context, bool inclusiveStart = true, bool inclusiveEnd = false) where T : Entity
        {
            var isGreaterThan = inclusiveStart ? ConditionOperator.GreaterEqual : ConditionOperator.GreaterThan;
            var isLessThan = inclusiveEnd ? ConditionOperator.LessEqual : ConditionOperator.LessThan;
            return ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isGreaterThan, start), context)
                && ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isLessThan, end), context);
        }

        private static IEnumerable<FilterExpression> HandleFilterExpressionsWithAliases(QueryExpression qe, FilterExpression fe)
        {
            var condFilter = new FilterExpression(fe.FilterOperator);
            condFilter.Conditions.AddRange(HandleConditionsWithAliases(qe, fe));
            if (condFilter.Conditions.Any())
            {
                yield return condFilter;
            }

            // Handle Adding filter for Conditions where the Entity Name is referencing a LinkEntity.
            // This is used primarily for Outer Joins, where the attempt is to see if the join entity does not exist.
            foreach (var child in fe.Filters.SelectMany(filter => HandleFilterExpressionsWithAliases(qe, filter)))
            {
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
    }
}
