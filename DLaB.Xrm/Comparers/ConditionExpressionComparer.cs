using System;
using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class ConditionExpressionComparer : IEqualityComparer<ConditionExpression>
    {
        public bool Equals(ConditionExpression condition1, ConditionExpression condition2)
        {
            if (condition1 == condition2) { return true; }
            if (condition1 == null || condition2 == null) { return false; }

            return condition1.AttributeName == condition2.AttributeName &&
                   condition1.Operator == condition2.Operator &&
                   new EnumerableComparer<Object>().Equals(condition1.Values, condition2.Values);
        }

        public int GetHashCode(ConditionExpression condition)
        {
            condition.ThrowIfNull("condition");

            // Skip the more expensive checks for the hash code.  They are more likely to mutate as well...
            return new HashCode().
                Hash(condition.AttributeName).
                Hash(condition.Operator);
            // Hash(condition.Values, new EnumerableComparer<Object>());
        }
    }
}
