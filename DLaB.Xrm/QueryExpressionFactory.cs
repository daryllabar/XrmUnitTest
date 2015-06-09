using System;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Collections;
using System.Linq.Expressions;

namespace DLaB.Xrm
{
    public class QueryExpressionFactory
    {
        #region Create

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create(string logicalName, params object[] columnNameAndValuePairs)
        {
            return Create(new LateBoundQuerySettings(logicalName), columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create(string logicalName, ColumnSet columnSet, params object[] columnNameAndValuePairs)
        {
            return Create(new LateBoundQuerySettings(logicalName) { Columns = columnSet }, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create(string logicalName, ColumnSet columnSet, bool first,
                params object[] columnNameAndValuePairs)
        {
            return Create(new LateBoundQuerySettings(logicalName) { Columns = columnSet, First = first }, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="logicalName"></param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create(string logicalName, bool activeOnly, ColumnSet columnSet, bool first,
                params object[] columnNameAndValuePairs)
        {
            var qe = Create(new LateBoundQuerySettings(logicalName)
            {
                ActiveOnly = activeOnly,
                Columns = columnSet,
                First = first
            });
            qe.WhereEqual(columnNameAndValuePairs);
            return qe;
        }

        #endregion // Create

        #region Create<T>

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(params object[] columnNameAndValuePairs) where T : Entity
        {
            return Create(new QuerySettings<T>(), columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(ColumnSet columnSet, params object[] columnNameAndValuePairs) where T : Entity
        {
            return Create(new QuerySettings<T> { Columns = columnSet }, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(Expression<Func<T, object>> anonymousTypeInitializer, 
                params object[] columnNameAndValuePairs) 
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return Create<T>(columnSet, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(ColumnSet columnSet, bool first,
                params object[] columnNameAndValuePairs) where T : Entity
        {
            return Create(new QuerySettings<T> { Columns = columnSet, First = first }, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(Expression<Func<T, object>> anonymousTypeInitializer, bool first,
                params object[] columnNameAndValuePairs)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return Create<T>(columnSet, first, columnNameAndValuePairs);
        }

        public static QueryExpression Create<T>(QuerySettings<T> settings,
            params object[] columnNameAndValuePairs) where T : Entity
        {
            var qe = Create(settings);
            qe.WhereEqual(columnNameAndValuePairs);
            return qe;
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(bool activeOnly, Expression<Func<T, object>> anonymousTypeInitializer,
                bool first, params object[] columnNameAndValuePairs)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return Create<T>(activeOnly, columnSet, first, columnNameAndValuePairs);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnNameAndValuePairs">List of pairs that look like this:
        /// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"</param>
        /// <returns></returns>
        public static QueryExpression Create<T>(bool activeOnly, ColumnSet columnSet, bool first,
                params object[] columnNameAndValuePairs) where T : Entity
        {
            var qe = Create(new QuerySettings<T>
            {
                ActiveOnly = activeOnly,
                Columns = columnSet,
                First = first
            });
            qe.WhereEqual(columnNameAndValuePairs);
            return qe;
        }

        public static QueryExpression Create<T>(QuerySettings<T> settings) where T : Entity
        {
            var qe = new QueryExpression
            {
                EntityName = settings.LogicalName ?? EntityHelper.GetEntityLogicalName<T>(),
                ColumnSet = settings.Columns,
                Criteria = {FilterOperator = settings.CriteriaOperator},
            };

            if (settings.First)
            {
                qe.First();
            }

            if (settings.LogicalName != null)
            {
                // Late Bound Entity
                if (settings.ActiveOnly)
                {
                    qe.ActiveOnly(settings.LogicalName);
                }
            }
            else
            {
                if (settings.ActiveOnly)
                {
                    qe.ActiveOnly<T>();
                }
            }

            return qe;
        }

        #endregion // Create<T>

        #region CreateIn

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, string columnName, IEnumerable values)
        {
            return CreateIn(new LateBoundQuerySettings(logicalName), columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, string columnName, params object[] values)
        {
            return CreateIn(new LateBoundQuerySettings(logicalName), columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, ColumnSet columnSet, string columnName, IEnumerable values)
        {
            return CreateIn(new LateBoundQuerySettings(logicalName) { Columns = columnSet }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, ColumnSet columnSet, string columnName, params object[] values)
        {
            return CreateIn(new LateBoundQuerySettings(logicalName) { Columns = columnSet }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, ColumnSet columnSet, bool first, string columnName, IEnumerable values)
        {
            return CreateIn(new LateBoundQuerySettings(logicalName) { Columns = columnSet, First = first }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, ColumnSet columnSet, bool first, string columnName,
                params object[] values)
           
        {
            return CreateIn(new LateBoundQuerySettings(logicalName) { Columns = columnSet, First = first }, columnName, values);
        }


        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="logicalName"></param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, bool activeOnly, ColumnSet columnSet, bool first, string columnName, IEnumerable values)
        {
            var qe = Create(new LateBoundQuerySettings(logicalName)
            {
                ActiveOnly = activeOnly,
                Columns = columnSet,
                First = first
            });
            qe.WhereIn(columnName, values);
            return qe;
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="logicalName"></param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn(string logicalName, bool activeOnly, ColumnSet columnSet, bool first, string columnName, params object[] values)
        {
            var qe = Create(new LateBoundQuerySettings(logicalName)
            {
                ActiveOnly = activeOnly,
                Columns = columnSet,
                First = first
            });
            qe.WhereIn(columnName, values);
            return qe;
        }

        #endregion // CreateIn

        #region CreateIn<T>

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(string columnName, IEnumerable values) where T : Entity
        {
            return CreateIn(new QuerySettings<T>(), columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(string columnName, params object[] values) where T : Entity
        {
            return CreateIn(new QuerySettings<T>(), columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(Expression<Func<T, object>> anonymousTypeInitializer,
                string columnName, IEnumerable values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(columnSet, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(ColumnSet columnSet, string columnName, IEnumerable values) where T : Entity
        {
            return CreateIn(new QuerySettings<T> { Columns = columnSet }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(Expression<Func<T, object>> anonymousTypeInitializer,
                string columnName, params object[] values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(columnSet, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(ColumnSet columnSet, string columnName, params object[] values) where T : Entity
        {
            return CreateIn(new QuerySettings<T> { Columns = columnSet }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(Expression<Func<T, object>> anonymousTypeInitializer, bool first,
                string columnName, IEnumerable values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(columnSet, first, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(ColumnSet columnSet, bool first, string columnName, IEnumerable values) where T : Entity
        {
            return CreateIn(new QuerySettings<T> { Columns = columnSet, First = first }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(Expression<Func<T, object>> anonymousTypeInitializer, bool first,
                string columnName, params object[] values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(columnSet, first, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(ColumnSet columnSet, bool first, string columnName, 
                params object[] values) 
            where T : Entity
        {
            return CreateIn(new QuerySettings<T> { Columns = columnSet, First = first }, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings">The query settings used to Create the QueryExpression.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(QuerySettings<T> settings, string columnName, params object[] values) where T : Entity
        {
            var qe = Create(settings);
            qe.WhereIn(columnName, values);
            return qe;
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(bool activeOnly, Expression<Func<T, object>> anonymousTypeInitializer,
                bool first, string columnName, IEnumerable values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(activeOnly, columnSet, first, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(bool activeOnly, ColumnSet columnSet, bool first, string columnName, IEnumerable values) where T : Entity
        {
            var qe = Create(new QuerySettings<T>
            {
                ActiveOnly = activeOnly,
                Columns = columnSet,
                First = first
            });
            qe.WhereIn(columnName, values);
            return qe;
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(bool activeOnly, Expression<Func<T, object>> anonymousTypeInitializer,
                bool first, string columnName, params object[] values)
            where T : Entity
        {
            var columnSet = new ColumnSet().AddColumns(anonymousTypeInitializer);
            return CreateIn<T>(activeOnly, columnSet, first, columnName, values);
        }

        /// <summary>
        /// Returns a Query Expression for the given inputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnSet">Columns to retrieve.</param>
        /// <param name="first">Used to specificy that only one entity should be returned.</param>
        /// <param name="activeOnly">Specifies if only Active Records should be returned.</param>
        /// <param name="columnName">The name of the column to perform the in against.</param>
        /// <param name="values">The list of values to search for being in the column name.</param>
        public static QueryExpression CreateIn<T>(bool activeOnly, ColumnSet columnSet, bool first, string columnName, params object[] values) where T : Entity
        {
            var qe = Create(new QuerySettings<T>
            { 
                ActiveOnly = activeOnly, 
                Columns = columnSet, 
                First = first });
            qe.WhereIn(columnName, values);
            return qe;
        }

        #endregion // CreateIn<T>
    }
}
