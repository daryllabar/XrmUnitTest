using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;

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
            IQueryable<LinkEntityTypes<TRoot, TTo>> result;
            var fromName = JoinAliasEntityPreFix + fromEntity.EntityAlias;
            if (link.JoinOperator == JoinOperator.Inner)
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable()
                             on ConvertCrmTypeToBasicComparable((TFrom)f[fromName], link.LinkFromAttributeName) equals
                             ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName)
                         select new LinkEntityTypes<TRoot, TTo>(AddAliasedColumns(f, t, link), t);
            }
            else
            {
                result = from f in query
                         join t in SchemaGetOrCreate<TTo>(info).AsQueryable() on ConvertCrmTypeToBasicComparable((TFrom)f[fromName], link.LinkFromAttributeName) equals
                             ConvertCrmTypeToBasicComparable(t, link.LinkToAttributeName) into joinResult
                         from t in joinResult.DefaultIfEmpty()
                         select new LinkEntityTypes<TRoot, TTo>(AddAliasedColumns(f, t, link), t);
            }

            // Apply any Conditions on the Link Entity
            result = ApplyLinkFilter(info, result, link.LinkCriteria);

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

#if !PRE_MULTISELECT
            foreach(var column in link.Columns.AttributeExpressions.Where(e => e.HasGroupBy == false && e.AggregateType == XrmAggregateType.None))
            {
                fromEntity.Attributes.Add(column.Alias, new AliasedValue(link.LinkToEntityName, column.AttributeName, toEntity[column.AttributeName]));
            }
#endif

            return fromEntity;
        }
    }
}
