using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Extension Methods for Local CRM
    /// </summary>
    public static class Extensions
    {
        #region Assembly

        /// <summary>
        /// Gets the entity type from the assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        internal static Type GetEntityType(this Assembly assembly, string logicalName)
        {
            return assembly.GetTypes().FirstOrDefault(t =>
                t.GetCustomAttribute<EntityLogicalNameAttribute>(true)?.LogicalName == logicalName);
        }

        #endregion Assembly
        #region ConditionExpression

        /// <summary>
        /// Gets the Date value from Date or string.
        /// </summary>
        /// <param name="condition">The condition expression.</param>
        /// <param name="index">The index of the value in the condition Values collection.  Defaults to 0.</param>
        /// <returns></returns>
        public static DateTime GetDateTimeValueFromDateOrString(this ConditionExpression condition, int index = 0)
        {
            var value = condition.Values[index];
            if (value is string stringValue)
            {
                return DateTime.Parse(stringValue);
            }

            if (value is DateTime dateValue)
            {
                return dateValue;
            }

            throw CrmExceptions.GetDateShouldBeStringOrDateException(condition.GetQualifiedAttributeName());
        }

        /// <summary>
        /// Gets the int value from int or string.
        /// </summary>
        /// <param name="condition">The condition expression.</param>
        /// <param name="index">The index of the value in the condition Values collection.  Defaults to 0.</param>
        /// <returns></returns>
        public static int GetIntValueFromIntOrString(this ConditionExpression condition, int index = 0)
        {
            var value = condition.Values[index];
            if (value is string stringValue)
            {
                return int.Parse(stringValue);
            }

            if (value is int intValue)
            {
                return intValue;
            }

            throw CrmExceptions.GetIntShouldBeStringOrIntException(condition.GetQualifiedAttributeName());
        }

        /// <summary>
        /// Gets the name of the qualified attribute.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public static string GetQualifiedAttributeName(this ConditionExpression condition)
        {
            return string.IsNullOrWhiteSpace(condition.EntityName) ? condition.AttributeName : condition.EntityName + "." + condition.AttributeName;
        }

        #endregion ConditionExpression

        #region DateTime

        /// <summary>
        /// Removes the milliseconds from the date.  CRM doesn't store milliseconds, so this is used frequently when working with dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime RemoveMilliseconds(this DateTime date)
        {
            return date.AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond));
        }

        /// <summary>
        /// Removes the milliseconds from the date.  CRM doesn't store milliseconds, so this is used frequently when working with dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime? RemoveMilliseconds(this DateTime? date)
        {
            return date?.AddTicks(-(date.Value.Ticks % TimeSpan.TicksPerSecond));
        }

        #endregion DateTime

        #region Entity

        /// <summary>
        /// Checks if the entity contains the given attribute and it is null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attName"></param>
        /// <returns></returns>
        public static bool ContainsNullValue(this Entity entity, string attName) { return entity.Contains(attName) && entity[attName] == null; }

        #endregion Entity

        #region EntityReference

        /// <summary>
        /// Clones the entity reference
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static EntityReference Clone(this EntityReference entity)
        {
            if(entity == null)
            {
                return null;
            }

            var clone = new EntityReference
            {
                LogicalName = entity.LogicalName,
                Id = entity.Id,
                Name = entity.Name
            };

#if !PRE_KEYATTRIBUTE
            clone.KeyAttributes.AddRange(entity.KeyAttributes);
            clone.RowVersion = entity.RowVersion;
#endif
            return clone;

        }

        #endregion EntityReference

        #region IEnumerable<FetchAttributeInfo>

        /// <summary>
        /// Returns all Attributes in the FetchType tree
        /// </summary>
        /// <param name="fe">The fe.</param>
        /// <returns></returns>
        public static IEnumerable<FetchAttributeInfo> GetAllAttributes(this FetchType fe)
        {
            var entity = (FetchEntityType)fe.Items[0];
            return (from item in entity.Items
                    select (IEnumerable<FetchAttributeInfo>)GetAllAttributes(entity.name, (dynamic)item)).SelectMany(v => v as FetchAttributeInfo[] ?? v.ToArray());

        }

        private static IEnumerable<FetchAttributeInfo> GetAllAttributes(string entityLogicalName, FetchAttributeType attribute)
        {
            yield return new FetchAttributeInfo(entityLogicalName, attribute);
        }

        // ReSharper disable once UnusedParameter.Local
        private static IEnumerable<FetchAttributeInfo> GetAllAttributes(string entityLogicalName, FetchLinkEntityType link)
        {
            var items = link.Items;
            if (items == null)
            {
                return new List<FetchAttributeInfo>();
            }

            return (from item in link.Items
                    select (IEnumerable<FetchAttributeInfo>)GetAllAttributes(link.name, (dynamic)item)).SelectMany(v => v as FetchAttributeInfo[] ?? v.ToArray());
        }

        // ReSharper disable UnusedParameter.Local
        private static IEnumerable<FetchAttributeInfo> GetAllAttributes(string entityLogicalName, object obj)
        {
            // ignore
            return Enumerable.Empty<FetchAttributeInfo>();
        }
        // ReSharper restore UnusedParameter.Local

        #endregion IEnumerable<FetchAttributeInfo>

        #region IOrderedEnumerable<T> where T : Entity

        /// <summary>
        /// Orders the entiities by the specified OrderExpressoin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="order">The order expression.</param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> Order<T>(this IOrderedEnumerable<T> entities, OrderExpression order) where T : Entity
        {
            return order.OrderType == OrderType.Ascending ?
                entities.ThenBy(e => ConvertStringObjectToLower(e[order.AttributeName])) :
                entities.ThenByDescending(e => ConvertStringObjectToLower(e[order.AttributeName]));
        }

        #endregion IOrderedEnumerable<T> where T : Entity 

        #region LinkEntity

        /// <summary>
        /// Walks the whole LinkEntity Tree, returning the LinkedEntity with the specific alias, or null if not found.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static LinkEntity GetLinkEntity(this LinkEntity link, string alias)
        {
            return link.EntityAlias == alias ? link : link.LinkEntities.FirstOrDefault(l => l.GetLinkEntity(alias) != null);
        }

        #endregion LinkEntity

        #region List<T> where T : Entity

        /// <summary>
        /// Orders the entiities by the specified OrderExpressoin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="order">The order expression.</param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> Order<T>(this List<T> entities, OrderExpression order) where T : Entity
        {
            return order.OrderType == OrderType.Ascending ?
                entities.OrderBy(e => ConvertStringObjectToLower(e[order.AttributeName])) :
                entities.OrderByDescending(e => ConvertStringObjectToLower(e[order.AttributeName]));
        }

        #endregion List<T> where T : Entity

        #region QueryExpression

        /// <summary>
        /// Walks the whole QueryExpresion Tree, returning the LinkedEntity with the specific alias, or null if not found.
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static LinkEntity GetLinkEntity(this QueryExpression qe, string alias)
        {
            return qe.LinkEntities.FirstOrDefault(l => l.GetLinkEntity(alias) != null);
        }

        #endregion QueryExpression

        #region Helper

        private static object ConvertStringObjectToLower(object value)
        {
            if (value == null) { return null; }
            if (value is string str)
            {
                return str.ToLower();
            }

            return value;
        }

        #endregion Helper
    }
}
