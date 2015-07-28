using DLaB.Xrm;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm
{
    public partial class Extensions
    {
        #region ConditionExpression

        private static bool EqualsCondition(this ConditionExpression c1, ConditionExpression c2)
        {
            return (c1 == c2) || (c1 != null && c2 != null &&
                c1.AttributeName == c2.AttributeName &&
                c1.Operator == c2.Operator &&
                c1.Values.SequenceEqual(c2.Values));
        }

        #endregion // ConditionExpression

        #region FilterExpression

        public static bool HasCondition(this FilterExpression filter, string entityName, params object[] columnNameAndValuePairs)
        {
            var tmp = new FilterExpression();

            // match all conditions one at a time.
            foreach (var condition in tmp.WhereEqual(entityName, columnNameAndValuePairs).Conditions)
            {
                if (!filter.HasCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasCondition(this FilterExpression filter, ConditionExpression condition)
        {
            return filter.Conditions.Any(c => c.EqualsCondition(condition)) ||
                filter.Filters.Any(f => f.HasCondition(condition));
        }

        #endregion // FilterExpression

        #region LinkEntity

        /// <summary>
        /// Returns all Filters that are filtering on the given entity logical name
        /// </summary>
        /// <param name="link"></param>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public static IEnumerable<FilterExpression> GetEntityFilters(this LinkEntity link, string logicalName)
        {
            if (link.LinkToEntityName == logicalName)
            {
                yield return link.LinkCriteria;
            }

            foreach (var childLink in link.LinkEntities)
            {
                foreach (var filter in childLink.GetEntityFilters(logicalName))
                {
                    yield return filter;
                }
            }
        }

        public static bool HasCondition(this LinkEntity link, params object[] columnNameAndValuePairs)
        {
            var tmp = new FilterExpression();

            // match all conditions one at a time.
            foreach (var condition in tmp.WhereEqual(link.LinkToEntityName, columnNameAndValuePairs).Conditions)
            {
                if (!link.HasCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasCondition(this LinkEntity link, ConditionExpression condition)
        {
            return link.LinkCriteria.HasCondition(condition) || link.LinkEntities.Any(l => l.HasCondition(condition));
        }

        public static bool HasEntity(this LinkEntity link, string logicalName)
        {
            return link.LinkToEntityName == logicalName || link.LinkEntities.Any(l => l.HasEntity(logicalName));
        }

        #endregion // LinkEntity

        #region QueryExpression

        /// <summary>
        /// Returns all Filters that are filtering on the given entity logical name
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public static IEnumerable<FilterExpression> GetEntityFilters(this QueryExpression qe, string logicalName)
        {
            if(qe.EntityName == logicalName){
                yield return qe.Criteria;
            }

            foreach (var link in qe.LinkEntities)
            {
                foreach (var filter in link.GetEntityFilters(logicalName))
                {
                    yield return filter;
                }
            }
        }

        public static bool HasCondition(this QueryExpression qe, params object[] columnNameAndValuePairs)
        {
            var tmp = new FilterExpression();

            // match all conditions one at a time.
            foreach (var condition in tmp.WhereEqual(qe.EntityName, columnNameAndValuePairs).Conditions)
            {
                // Needs to loop through and find all
                if (!qe.HasCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasCondition(this QueryExpression qe, ConditionExpression condition)
        {
            return qe.Criteria.HasCondition(condition) ||
                  qe.LinkEntities.Any(l => l.HasCondition(condition));
        }

        public static bool HasEntity(this QueryExpression qe, string logicalName)
        {
            return qe.EntityName == logicalName || qe.LinkEntities.Any(l => l.HasEntity(logicalName));
        }

        #endregion // QueryExpression   
    }
}
