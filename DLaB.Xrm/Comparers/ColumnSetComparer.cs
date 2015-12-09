using System;
using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    /// <summary>
    /// Comparer for ColumnSets
    /// </summary>
    public class ColumnSetComparer : IEqualityComparer<ColumnSet>
    {
        /// <summary>
        /// Compaes the two Column Sets
        /// </summary>
        /// <param name="cs1">The CS1.</param>
        /// <param name="cs2">The CS2.</param>
        /// <returns></returns>
        public bool Equals(ColumnSet cs1, ColumnSet cs2)
        {
            if (cs1 == cs2) { return true; }
            if (cs1 == null || cs2 == null) { return false; }

            return cs1.AllColumns == cs2.AllColumns
                && new EnumerableComparer<String>().Equals(cs1.Columns, cs2.Columns);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="cs">The cs.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(ColumnSet cs)
        {
            cs.ThrowIfNull("cs");
            return new EnumerableComparer<String>().GetHashCode(cs.Columns);
        }
    }
}
