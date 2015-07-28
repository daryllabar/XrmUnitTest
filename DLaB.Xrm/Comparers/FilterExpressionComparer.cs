using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class FilterExpressionComparer : IEqualityComparer<FilterExpression>
    {
        public bool Equals(FilterExpression filter1, FilterExpression filter2)
        {
            if (filter1 == filter2) { return true; }
            if (filter1 == null || filter2 == null) { return false; }

            return new EnumerableComparer<ConditionExpression>(new ConditionExpressionComparer()).Equals(filter1.Conditions, filter2.Conditions) &&
                   filter1.FilterOperator == filter2.FilterOperator &&
                   new EnumerableComparer<FilterExpression>(new FilterExpressionComparer()).Equals(filter1.Filters, filter2.Filters) &&
                   filter1.IsQuickFindFilter == filter2.IsQuickFindFilter;
        }

        public int GetHashCode(FilterExpression filter)
        {
            filter.ThrowIfNull("filter");
            return new HashCode()
                .Hash(filter.Conditions, new EnumerableComparer<ConditionExpression>(new ConditionExpressionComparer()))
                .Hash(filter.FilterOperator)
                .Hash(filter.Filters, new EnumerableComparer<FilterExpression>(new FilterExpressionComparer()))
                .Hash(filter.IsQuickFindFilter);
        }
    }
}
