using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm
{
    public partial class Extensions
    {
        #region ConditionExpression

        private static bool ValuesInConditionIn(this ConditionExpression c1, string attributeName, IEnumerable<object> values)
        {
            var list = values.ToList();
            return (c1 != null && attributeName != null && list.Any() &&
                c1.AttributeName == attributeName &&
                c1.Operator == ConditionOperator.In &&
                !list.Except(c1.Values).Any()); // http://stackoverflow.com/questions/332973/linq-check-whether-an-array-is-a-subset-of-another
        }

        #endregion // ConditionExpression

        #region FilterExpression

        public static bool HasConditionInWithValues(this FilterExpression filter, params object[] columnNameAndValuePairs)
        {
            var attributeName = columnNameAndValuePairs[0] as string;
            if (attributeName == null)
            {
                throw new ArgumentException("HasConditionInWithValues requires the first value in the columnNameAndValuePairs attribute to be the attribute name.", "columnNameAndValuePairs");
            }

            return filter.Conditions.Any(c => c.ValuesInConditionIn(attributeName, columnNameAndValuePairs.Skip(1))) ||
                filter.Filters.Any(f => f.HasConditionInWithValues(columnNameAndValuePairs));
        }

        #endregion // FilterExpression

        #region LinkEntity

        public static bool HasConditionInWithValues(this LinkEntity link, params object[] columnNameAndValuePairs)
        {
            return link.LinkCriteria.HasConditionInWithValues(columnNameAndValuePairs) || link.LinkEntities.Any(l => l.HasConditionInWithValues(columnNameAndValuePairs));
        }

        #endregion // LinkEntity

        #region QueryExpression

        public static bool HasConditionInWithValues(this QueryExpression qe, params object[] columnNameAndValuePairs)
        {
            return qe.Criteria.HasConditionInWithValues(columnNameAndValuePairs) ||
                  qe.LinkEntities.Any(l => l.HasConditionInWithValues(columnNameAndValuePairs));
        }

        #endregion // QueryExpression
    }
}
