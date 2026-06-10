using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        private static IQueryable<T> CallJoin<T>(LocalCrmDatabaseInfo info, IQueryable<T> query, LinkEntity link) where T : Entity
        {
            try
            {
                var tFrom = typeof(T);
                var tTo = GetType(info, link.LinkToEntityName);
                return (IQueryable<T>)GenericMethodCaller.InvokeLocalCrmDatabaseStaticMultiGenericMethod(
                    info,
                    nameof(Join),
                    BindingFlags.NonPublic,
                    new object[] { tFrom, tTo },
                    info, query, link);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null)
                {
                    throw;
                }

                throw ex.InnerException;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static IQueryable<TFrom> Join<TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TFrom> query, LinkEntity link)
            where TFrom : Entity
            where TTo : Entity
        {
            // NotAny is an anti-semi-join (NOT EXISTS): return TFrom records that have NO matching TTo record.
            if (link.JoinOperator == JoinOperator.NotAny)
            {
                return ApplyNotAnyFilter<TFrom, TTo>(info, query, link);
            }

            IQueryable<LinkEntityTypes<TFrom, TTo>> result;
            if (link.JoinOperator == JoinOperator.Inner)
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on 
                         new
                         {
                             Id = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName),
                             FilterConditions = true
                         } equals
                         new
                         {
                             Id = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName),
                             FilterConditions = EvaluateFilter(t, link.LinkCriteria, new QueryContext(info))
                         }
                         select new LinkEntityTypes<TFrom, TTo>(AddAliasedColumns(f, t, link), t);

                // Apply any Conditions on the Link Entity
                //result = ApplyLinkFilter(info, result, link.LinkCriteria);
            }
            else
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName),
                                 FilterConditions = true
                             }
                             equals
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName),
                                 FilterConditions = EvaluateFilter(t, link.LinkCriteria, new QueryContext(info))
                             }
                             into joinResult
                         from t in joinResult.DefaultIfEmpty()
                         select new LinkEntityTypes<TFrom, TTo>(AddAliasedColumns(f, t, link), t);
            }

            var root = result.Select(r => r.Root);
            foreach (var entity in link.LinkEntities)
            {
                root = CallChildJoin<TFrom, TTo>(info, root, link, entity);
            }

            return root;
        }

        /// <summary>
        /// Applies a NotAny (NOT EXISTS / anti-semi-join) filter.
        /// Returns TFrom records that have NO matching TTo record (considering link criteria and nested links).
        /// </summary>
        private static IQueryable<TFrom> ApplyNotAnyFilter<TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TFrom> query, LinkEntity link)
            where TFrom : Entity
            where TTo : Entity
        {
            // Start with all TTo entities that satisfy the link criteria.
            IQueryable<TTo> matchingTo = SchemaGetOrCreate<TTo>(info).AsQueryable()
                .Where(t => EvaluateFilter(t, link.LinkCriteria, new QueryContext(info)));

            // For each nested link, further restrict matchingTo to only those with a matching related record.
            foreach (var nestedLink in link.LinkEntities)
            {
                matchingTo = FilterByNestedLinkExistence<TTo>(info, matchingTo, nestedLink);
            }

            // Collect the set of "to" attribute values that constitute a match.
            var matchSet = new HashSet<object>();
            foreach (var t in matchingTo)
            {
                var toValue = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName);
                if (toValue != null)
                {
                    matchSet.Add(toValue);
                }
            }

            // Return only TFrom entities whose link attribute has NO entry in the match set.
            return query.AsEnumerable().Where(f =>
            {
                var fromValue = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName);
                return fromValue == null || !matchSet.Contains(fromValue);
            }).AsQueryable();
        }

        /// <summary>
        /// Filters <paramref name="query"/> to only those entities that have (or do not have,
        /// depending on the nested link's join operator) a matching record in the related entity.
        /// Used when building the existence check for a NotAny join.
        /// </summary>
        private static IQueryable<TFrom> FilterByNestedLinkExistence<TFrom>(LocalCrmDatabaseInfo info, IQueryable<TFrom> query, LinkEntity link)
            where TFrom : Entity
        {
            var tFrom = typeof(TFrom);
            var tTo = GetType(info, link.LinkToEntityName);
            return (IQueryable<TFrom>)GenericMethodCaller.InvokeLocalCrmDatabaseStaticMultiGenericMethod(
                info,
                nameof(FilterByNestedLinkExistenceGeneric),
                BindingFlags.NonPublic,
                new object[] { tFrom, tTo },
                info, query, link);
        }

        // ReSharper disable once UnusedMember.Local
        private static IQueryable<TFrom> FilterByNestedLinkExistenceGeneric<TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TFrom> query, LinkEntity link)
            where TFrom : Entity
            where TTo : Entity
        {
            // Get TTo entities matching this nested link's criteria.
            IQueryable<TTo> matchingTo = SchemaGetOrCreate<TTo>(info).AsQueryable()
                .Where(t => EvaluateFilter(t, link.LinkCriteria, new QueryContext(info)));

            // Recursively apply any deeper nested links.
            foreach (var nestedLink in link.LinkEntities)
            {
                matchingTo = FilterByNestedLinkExistence<TTo>(info, matchingTo, nestedLink);
            }

            // Collect the set of "to" attribute values.
            var matchSet = new HashSet<object>();
            foreach (var t in matchingTo)
            {
                var toValue = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName);
                if (toValue != null)
                {
                    matchSet.Add(toValue);
                }
            }

            // Filter TFrom based on the nested link's join operator.
            if (link.JoinOperator == JoinOperator.NotAny)
            {
                // Nested NotAny: keep TFrom records with NO matching TTo.
                return query.AsEnumerable().Where(f =>
                {
                    var fromValue = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName);
                    return fromValue == null || !matchSet.Contains(fromValue);
                }).AsQueryable();
            }

            if (link.JoinOperator == JoinOperator.LeftOuter)
            {
                // LeftOuter within an existence check: does not restrict the outer set.
                return query;
            }

            // Inner / Natural: keep TFrom records that DO have a matching TTo.
            return query.AsEnumerable().Where(f =>
            {
                var fromValue = ConvertCrmTypeToBasicComparable(f, link.LinkFromAttributeName);
                return fromValue != null && matchSet.Contains(fromValue);
            }).AsQueryable();
        }

        private static IQueryable<LinkEntityTypes<TFrom, TTo>> ApplyLinkFilter<TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<LinkEntityTypes<TFrom, TTo>> query, FilterExpression filter)
            where TFrom : Entity
            where TTo : Entity
        {
            return query.Where(l => EvaluateFilter(l.Current, filter, new QueryContext(info)));
        }

        private static IQueryable<TRoot> CallChildJoin<TRoot, TFrom>(LocalCrmDatabaseInfo info, IQueryable<TRoot> query, LinkEntity fromEntity, LinkEntity link)
            where TRoot : Entity
            where TFrom : Entity
        {
            var tRoot = typeof(TRoot);
            var tTo = GetType(info, link.LinkToEntityName);
            return (IQueryable<TRoot>)GenericMethodCaller.InvokeLocalCrmDatabaseStaticMultiGenericMethod(
                info,
                nameof(ChildJoin),
                BindingFlags.NonPublic,
                new object[] { tRoot, typeof(TFrom), tTo },
                info, query, fromEntity, link);
        }


        // ReSharper disable once UnusedMember.Local
        private static IQueryable<TRoot> ChildJoin<TRoot, TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TRoot> query, LinkEntity fromEntity, LinkEntity link)
            where TRoot : Entity
            where TFrom : Entity
            where TTo : Entity
        {
            var fromName = JoinAliasEntityPreFix + fromEntity.EntityAlias;

            // NotAny is an anti-semi-join (NOT EXISTS): filter TRoot records where the embedded TFrom
            // has NO matching TTo record.
            if (link.JoinOperator == JoinOperator.NotAny)
            {
                // Start with TTo entities matching the link criteria.
                IQueryable<TTo> matchingTo = SchemaGetOrCreate<TTo>(info).AsQueryable()
                    .Where(t => EvaluateFilter(t, link.LinkCriteria, new QueryContext(info)));

                // For each nested link, further restrict matchingTo (existence check).
                foreach (var nestedLink in link.LinkEntities)
                {
                    matchingTo = FilterByNestedLinkExistence<TTo>(info, matchingTo, nestedLink);
                }

                // Collect the set of "to" attribute values that constitute a match.
                var matchSet = new HashSet<object>();
                foreach (var t in matchingTo)
                {
                    var toValue = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName);
                    if (toValue != null)
                    {
                        matchSet.Add(toValue);
                    }
                }

                // Return TRoot records whose embedded TFrom has NO entry in the match set.
                return query.AsEnumerable().Where(f =>
                {
                    var fromEntity2 = f[fromName] as TFrom;
                    if (fromEntity2 == null)
                    {
                        return true; // No embedded TFrom means no match is possible.
                    }

                    var fromValue = ConvertCrmTypeToBasicComparable(fromEntity2, link.LinkFromAttributeName);
                    return fromValue == null || !matchSet.Contains(fromValue);
                }).AsQueryable();
            }

            IQueryable<LinkEntityTypes<TRoot, TTo>> result;
            if (link.JoinOperator == JoinOperator.Inner)
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on 
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable((TFrom)f[fromName], link.LinkFromAttributeName),
                                 FilterConditions = true
                             } equals
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName),
                                 FilterConditions = EvaluateFilter(t, link.LinkCriteria, new QueryContext(info))
                             }
                         select new LinkEntityTypes<TRoot, TTo>(AddAliasedColumns(f, t, link), t);
            }
            else
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable((TFrom)f[fromName], link.LinkFromAttributeName),
                                 FilterConditions = true
                             } equals
                             new
                             {
                                 Id = ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName),
                                 FilterConditions = EvaluateFilter(t, link.LinkCriteria, new QueryContext(info))
                             }
                             into joinResult
                         from t in joinResult.DefaultIfEmpty()
                         select new LinkEntityTypes<TRoot, TTo>(AddAliasedColumns(f, t, link), t);
            }

            var root = result.Select(r => r.Root);
            foreach (var entity in link.LinkEntities)
            {
                root = CallChildJoin<TRoot, TTo>(info, root, link, entity);
            }

            return root;

            //return link.LinkEntities.Aggregate(result.Select(e => AddAliasedColumns(e.Root, e.Current, e.Alias, link.Columns)),
            //                                   (current, childLink) => current.Intersect(CallChildJoin(info, result, childLink)));
        }
        //private static IQueryable<TRoot> ChildJoin<TRoot, TFrom, TTo>(LocalCrmDatabaseInfo info, IQueryable<TRoot> query, LinkEntity link)
        //    where TRoot : Entity
        //    where TFrom : Entity
        //    where TTo : Entity
        //{
        //    IQueryable<LinkEntityTypes<TRoot, TTo>> result;
        //    if (link.JoinOperator == JoinOperator.Inner)
        //    {
        //      result = from f in query.Select(q => new LinkEntityTypes<TRoot,TFrom>(q, (TFrom)q["ALIAS_FROM_ENTITY"], ""))
        //               join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable(f.Current, link.LinkFromAttributeName) equals
        //                   ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName)
        //               select new LinkEntityTypes<TRoot, TTo>(AddAliasedColumns(f.Root, t, link.EntityAlias, link.Columns), t, link.EntityAlias);
        //    }
        //    else
        //    {
        //        result = from f in query
        //                 join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable(f.Current, link.LinkFromAttributeName) equals
        //                     ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName) into joinResult
        //                 from t in joinResult.DefaultIfEmpty()
        //                 select new LinkEntityTypes<TRoot, TTo>(f.Root, t, link.EntityAlias);
        //    }

        //    // Apply any Conditions on the Link Entity
        //    result = ApplyLinkFilter(result, link.LinkCriteria);

        //    foreach (var entity in link.LinkEntities)
        //    {
        //        //CallChildJoin(info, result, entity);
        //    }
        //    return result.Select(r => r.Root);
        //}

        internal class LinkEntityTypes<TRoot, TCurrent>
            where TRoot : Entity
            where TCurrent : Entity
        {
            public TRoot Root { get; }
            public TCurrent Current { get; }

            public LinkEntityTypes(TRoot root, TCurrent current)
            {
                Root = root;
                Current = current;
            }
        }

        internal class LinkEntityTypes<TRoot, TFrom, TTo>
            where TRoot : Entity
            where TFrom : Entity
            where TTo : Entity
        {
            public TRoot Root { get; }
            public TFrom From { get; }
            public TTo To { get; }

            public LinkEntityTypes(TRoot root, TFrom from, TTo to)
            {
                Root = root;
                From = from;
                To = to;
            }
        }

        internal const string JoinAliasEntityPreFix = "ALIAS_FROM_ENTITY_";

        private static TFrom AddAliasedColumns<TFrom, TTo>(TFrom fromEntity, TTo toEntity, LinkEntity link)
            where TFrom : Entity
            where TTo : Entity
        {
            fromEntity[JoinAliasEntityPreFix + link.EntityAlias] = toEntity;
            if (toEntity == null)
            {
                return fromEntity;
            }

            // Since the Projection is modifying the underlying objects, a HasAliasedAttribute Call is required.  
            if (link.Columns.AllColumns)
            {
                foreach (var attribute in toEntity.Attributes.Where(a => !fromEntity.HasAliasedAttribute(link.EntityAlias + "." + a.Key)))
                {
                    fromEntity.AddAliasedValue(link.EntityAlias, toEntity.LogicalName, attribute.Key, attribute.Value);
                }
            }
            else
            {
                foreach (var c in link.Columns.Columns.Where(v => toEntity.Attributes.Keys.Contains(v) && !fromEntity.HasAliasedAttribute(link.EntityAlias + "." + v)))
                {
                    fromEntity.AddAliasedValue(link.EntityAlias, toEntity.LogicalName, c, toEntity[c]);
                }
            }

            foreach(var column in link.Columns.AttributeExpressions.Where(e => e.HasGroupBy == false && e.AggregateType == XrmAggregateType.None))
            {
                fromEntity.Attributes.Add(column.Alias, new AliasedValue(link.LinkToEntityName, column.AttributeName, toEntity[column.AttributeName]));
            }

            return fromEntity;
        }
    }
}
