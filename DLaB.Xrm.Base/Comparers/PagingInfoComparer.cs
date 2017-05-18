using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    /// <summary>
    /// Compares Paging Infos
    /// </summary>
#if DLAB_PUBLIC
    public class PagingInfoComparer : IEqualityComparer<PagingInfo>
#else
    internal class PagingInfoComparer : IEqualityComparer<PagingInfo>
#endif
    {
        private static IEqualityComparer<PagingInfo> Comparer { get; set; }

        static PagingInfoComparer()
        {
            Comparer = ProjectionEqualityComparer<PagingInfo>.Create(i => new { i.Count, i.PageNumber, i.PagingCookie, i.ReturnTotalRecordCount});
        }
        /// <summary>
        /// Compares the two page infos
        /// </summary>
        /// <param name="page1">The page1.</param>
        /// <param name="page2">The page2.</param>
        /// <returns></returns>
        public bool Equals(PagingInfo page1, PagingInfo page2)
        {
            return Comparer.Equals(page1, page2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(PagingInfo info)
        {
            info.ThrowIfNull("info");
            return Comparer.GetHashCode(info);
        }
    }
}
