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
                        var likeCondition = (string)condition.Values[0];
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
                    value = IsBetween(entity, condition, DateTime.UtcNow.Date.AddDays(-1d * days), DateTime.UtcNow);
                    break;
                case ConditionOperator.NextXDays:
                    days = condition.GetIntValueFromIntOrString();
                    if (days <= 0)
                    {
                        throw CrmExceptions.GetConditionValueGreaterThan0Exception();
                    }
                    value = IsBetween(entity, condition, DateTime.UtcNow, DateTime.UtcNow.Date.AddDays(days + 1));
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
                    value = !ConditionIsTrue(entity, condition);
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
                    value = !ConditionIsTrue(entity, condition);
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
                default:
                    throw new NotImplementedException(condition.Operator.ToString());
            }
            return value;
        }

        /// <summary>
        /// Determines whether the condition specified entity is between.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="inclusiveStart">if set to <c>true</c> [inclusive start].</param>
        /// <param name="inclusiveEnd">if set to <c>true</c> [inclusive end].</param>
        /// <returns></returns>
        private static bool IsBetween<T>(T entity, ConditionExpression condition, DateTime start, DateTime end, bool inclusiveStart = true, bool inclusiveEnd = false) where T : Entity
        {
            var isGreaterThan = inclusiveStart ? ConditionOperator.GreaterThan : ConditionOperator.GreaterEqual;
            var isLessThan = inclusiveEnd ? ConditionOperator.LessThan : ConditionOperator.LessEqual;
            return ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isGreaterThan, start))
                && ConditionIsTrue(entity, new ConditionExpression(condition.AttributeName, isLessThan, end));
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
