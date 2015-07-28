using DLaB.Xrm.Comparers;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm
{
    public static partial class Extensions
    {
        #region ColumnSet

        public static bool IsEqual(this ColumnSet cs, ColumnSet columnSet)
        {
            return new ColumnSetComparer().Equals(cs, columnSet);
        }

        #endregion // ColumnSet

        #region FilterExpression

        public static bool IsEqual(this FilterExpression fe, FilterExpression filter)
        {
            return new FilterExpressionComparer().Equals(fe, filter);
        }

        #endregion // FilterExpression

        #region PagingInfo

        public static bool IsEqual(this PagingInfo infoThis, PagingInfo info)
        {
            return new PagingInfoComparer().Equals(infoThis, info);
        }

        #endregion // PagingInfo

        #region QueryExpression

        public static bool IsEqual(this QueryExpression qeThis, QueryExpression qe)
        {
            return new QueryExpressionComparer().Equals(qeThis, qe);
        }

        #endregion // QueryExpression
    }
}
