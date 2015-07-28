using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class OrderExpressionComparer : IEqualityComparer<OrderExpression>
    {
        private static IEqualityComparer<OrderExpression> Comparer { get; set; }

        static OrderExpressionComparer()
        {
            Comparer = ProjectionEqualityComparer<OrderExpression>.Create(o => new { o.AttributeName, o.OrderType });
        }
        public bool Equals(OrderExpression order1, OrderExpression order2)
        {
            return Comparer.Equals(order1, order2);
        }

        public int GetHashCode(OrderExpression order)
        {
            order.ThrowIfNull("order");
            return Comparer.GetHashCode(order);
        }
    }
}
