using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class PagingInfoComparer : IEqualityComparer<PagingInfo>
    {
        private static IEqualityComparer<PagingInfo> Comparer { get; set; }

        static PagingInfoComparer()
        {
            Comparer = ProjectionEqualityComparer<PagingInfo>.Create(i => new { i.Count, i.PageNumber, i.PagingCookie, i.ReturnTotalRecordCount});
        }
        public bool Equals(PagingInfo page1, PagingInfo page2)
        {
            return Comparer.Equals(page1, page2);
        }

        public int GetHashCode(PagingInfo info)
        {
            info.ThrowIfNull("info");
            return Comparer.GetHashCode(info);
        }
    }
}
