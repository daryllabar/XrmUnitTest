using System;
using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class ColumnSetComparer : IEqualityComparer<ColumnSet>
    {
        public bool Equals(ColumnSet cs1, ColumnSet cs2)
        {
            if (cs1 == cs2) { return true; }
            if (cs1 == null || cs2 == null) { return false; }

            return cs1.AllColumns == cs2.AllColumns
                && new EnumerableComparer<String>().Equals(cs1.Columns, cs2.Columns);
        }

        public int GetHashCode(ColumnSet cs)
        {
            cs.ThrowIfNull("cs");
            return new EnumerableComparer<String>().GetHashCode(cs.Columns);
        }
    }
}
