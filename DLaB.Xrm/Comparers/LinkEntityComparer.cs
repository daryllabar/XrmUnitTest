using System.Collections.Generic;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Comparers
{
    public class LinkEntityComparer : IEqualityComparer<LinkEntity>
    {
        public bool Equals(LinkEntity link1, LinkEntity link2)
        {
            if (link1 == link2) { return true; }
            if (link1 == null || link2 == null) { return false; }

            return 
                // Cheap checks First
                link1.EntityAlias == link2.EntityAlias &&
                link1.JoinOperator == link2.JoinOperator &&
                link1.LinkFromAttributeName == link2.LinkFromAttributeName &&
                link1.LinkFromEntityName == link2.LinkFromEntityName &&
                link1.LinkToAttributeName == link2.LinkToAttributeName &&
                link1.LinkToEntityName == link2.LinkToEntityName &&
                // More Expensive Second
                link1.Columns.IsEqual(link2.Columns) &&
                link1.LinkCriteria.IsEqual(link2.LinkCriteria) &&
                new EnumerableComparer<LinkEntity>(new LinkEntityComparer()).Equals(link1.LinkEntities, link2.LinkEntities);
        }

        public int GetHashCode(LinkEntity link)
        {
            link.ThrowIfNull("link");
            // Skip the more expensive checks for the hashCode.  They are more likely to mutate as well...
            return new HashCode()
                // .Hash(link.Columns, new ColumnSetComparer())
                .Hash(link.EntityAlias)
                .Hash(link.JoinOperator)
                // .Hash(link.LinkCriteria, new FilterExpressionComparer())
                // .Hash(link.LinkEntities, new EnumerableComparer<LinkEntity>(new LinkEntityComparer()))
                .Hash(link.LinkFromAttributeName)
                .Hash(link.LinkFromEntityName)
                .Hash(link.LinkToAttributeName)
                .Hash(link.LinkToEntityName);
        }
    }
}

