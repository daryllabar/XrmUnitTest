using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using Microsoft.Crm.Sdk.Messages;
using DLaB.Common.Exceptions;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using DLaB.Common;
using Microsoft.Xrm.Sdk.Messages;

namespace DLaB.Xrm
{
    [DebuggerStepThrough]
    public static partial class Extensions
    {
        #region <T>

        /// <summary>
        /// Serializes the value to a json string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The Value to serialize to JSON</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">text</exception>
        public static string SerializeToJson<T>(this T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(memoryStream, value);
                return Encoding.Default.GetString(memoryStream.ToArray());
            }
        }

        #endregion <T>

        #region ColumnSet

        /// <summary>
        /// Allows for adding column names in an early bound manner:
        /// columnSet.AddColumns&lt;Opportunity&gt;(i => new { i.OpportunityId, i.SalesStage, i.SalesStageCode });
        /// </summary>
        /// <typeparam name="T">An Entity Type</typeparam>
        /// <param name="columnSet">The ColumnSet</param>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add</param>
        /// <returns></returns>
        public static ColumnSet AddColumns<T>(this ColumnSet columnSet, Expression<Func<T, object>> anonymousTypeInitializer) where T : Entity
        {
            columnSet.AddColumns(GetAttributeNamesArray(anonymousTypeInitializer));
            return columnSet;
        }

        #endregion // ColumnSet

        #region Entity

        #region AssertContainsAllNonNull

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null.  Any missing/null attributes will result in an exception, listing the missing attributes.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static void AssertContainsAllNonNull(this Entity entity, params string[] attributeNames)
        {
            List<string> missingAttributes;
            if (!entity.ContainsAllNonNull(out missingAttributes, attributeNames))
            {
                if (missingAttributes.Count == 1)
                {
                    throw new Exception("Missing Required Field: " + missingAttributes[0]);
                }

                throw new Exception("Missing Required Fields: " + String.Join(", ", missingAttributes));
            }
        }

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null.  Any missing/null attributes will result in an exception, listing the missing attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="anonymousTypeInitializer"></param>
        /// <returns></returns>
        public static void AssertContainsAllNonNull<T>(this T entity, Expression<Func<T, object>> anonymousTypeInitializer)
            where T : Entity
        {
            AssertContainsAllNonNull(entity, GetAttributeNamesArray(anonymousTypeInitializer));
        }

        #endregion AssertContainsAllNonNull

        /// <summary>
        /// Clears the id of an entity so it can be saved as a new entity
        /// </summary>
        /// <param name="entity"></param>
        public static void ClearId(this Entity entity)
        {
            entity.Id = Guid.Empty;
            entity.Attributes.Remove(EntityHelper.GetIdAttributeName(entity.LogicalName));
        }

        #region ContainsAllNonNull

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static bool ContainsAllNonNull(this Entity entity, params string[] attributeNames)
        {
            return attributeNames.All(name => entity.Contains(name) && entity[name] != null);
        }

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="anonymousTypeInitializer"></param>
        /// <returns></returns>
        public static bool ContainsAllNonNull<T>(this T entity, Expression<Func<T, object>> anonymousTypeInitializer)
            where T : Entity
        {
            return ContainsAllNonNull(entity, GetAttributeNamesArray(anonymousTypeInitializer));
        }

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="nullAttributeNames"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static bool ContainsAllNonNull(this Entity entity, out List<string> nullAttributeNames, params string[] attributeNames)
        {
            nullAttributeNames = attributeNames.Where(name => !entity.Contains(name) || entity[name] == null).ToList();

            return !nullAttributeNames.Any();
        }

        /// <summary>
        /// Checks to see if the Entity Contains the attribute names, and the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="nullAttributeNames"></param>
        /// <param name="anonymousTypeInitializer"></param>
        /// <returns></returns>
        public static bool ContainsAllNonNull<T>(this T entity, out List<string> nullAttributeNames, Expression<Func<T, object>> anonymousTypeInitializer)
            where T : Entity
        {
            return ContainsAllNonNull(entity, out nullAttributeNames, GetAttributeNamesArray(anonymousTypeInitializer));
        }

        #endregion ContainsAllNonNull

        #region ContainsAnyNonNull

        /// <summary>
        /// Checks to see if the Entity Contains any of the attribute names, and the value is not null
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static bool ContainsAnyNonNull(this Entity entity, params string[] attributeNames)
        {
            return attributeNames.Any(name => entity.Contains(name) && entity[name] != null);
        }

        /// <summary>
        /// Checks to see if the Entity Contains any of the attribute names, and the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="anonymousTypeInitializer"></param>
        /// <returns></returns>
        public static bool ContainsAnyNonNull<T>(this T entity, Expression<Func<T, object>> anonymousTypeInitializer)
            where T : Entity
        {
            return ContainsAnyNonNull(entity, GetAttributeNamesArray(anonymousTypeInitializer));
        }

        #endregion ContainsAnyNonNull

        public static String GetFormUrl(this Entity entity, Uri uri)
        {
            string parameters = String.Format("etn={0}&pagetype=entityrecord&id={1}", entity.LogicalName, entity.Id.ToString("D"));
            string url;
            if (uri == null)
            {
                url = @"http://localhost/main.aspx?" + parameters;
            }
            else
            {
                url = String.Format(@"http://{0}/{1}main.aspx?{2}", uri.Host, uri.Segments[1], parameters);
            }
            return url;
        }

        public static String GetFormUrl(this Entity entity, IOrganizationService service)
        {
            return entity.GetFormUrl(service.GetServiceUri());
        }

        public static string GetFormattedAttributeValueOrNull(this Entity entity, string attributeLogicalName)
        {
            if (string.IsNullOrWhiteSpace(attributeLogicalName))
            {
                throw new ArgumentNullException("attributeLogicalName");
            }
            // Check for unaliased value first
            if (entity.FormattedValues.Contains(attributeLogicalName))
            {
                return entity.FormattedValues[attributeLogicalName];
            }
            // Check for Aliased values second
            foreach (var attribute in entity.FormattedValues.Keys)
            {
                var attributeName = attribute;
                var aliasedEntityName = SplitAliasedAttributeEntityName(ref attributeName);
                if (!String.IsNullOrWhiteSpace(aliasedEntityName) && attributeName == attributeLogicalName)
                {
                    return entity.FormattedValues[attribute];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns Local Option Set text Only.  Not sure what else it does...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeLogicalName"></param>
        /// <returns></returns>
        public static string GetFormattedAttributeValue(this Entity entity, string attributeLogicalName)
        {
            var value = entity.GetFormattedAttributeValueOrNull(attributeLogicalName);

            if (value == null)
            {
                throw new Exception("Formatted attribute value with attribute " + attributeLogicalName +
                " was not found!  Only these attributes were found: " + String.Join(", ", entity.Attributes.Keys));
            }

            return value;
        }

        public static string GetIdAttributeName(this Entity entity)
        {
            return EntityHelper.GetIdAttributeName(entity.LogicalName);
        }

        /// <summary>
        /// Returns the Id of the entity or Guid.Empty if it is null"
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Guid GetIdOrDefault(this Entity entity)
        {
            return entity == null ? Guid.Empty : entity.Id;
        }

        /// <summary>
        /// Returns the name of the Attribute that contains the default name of the entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static String GetNameAttribute(this Entity entity)
        {
            var value = String.Empty;
            var index = entity.LogicalName.IndexOf("_", StringComparison.Ordinal);
            var prefix = index > 0 ? entity.LogicalName.Substring(0, index) : String.Empty;
            if (entity.Contains(prefix + "_name"))
            {
                value = prefix + "_name";
            }
            else if (entity.Contains("fullname"))
            {
                value = "fullname";
            }
            else if (entity.Contains("name"))
            {
                value = "name";
            }
            else if (entity.LogicalName.Contains('_'))
            {
                // Get prefix of entity
                prefix = entity.LogicalName.Substring(0, entity.LogicalName.IndexOf('_'));
                if (entity.Contains(prefix + "_name"))
                {
                    value = prefix + "_name";
                }
            }

            return value;
        }

        /// <summary>
        /// Returns the Name and Id of the Current Entity in this format "Name (id)"
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static String GetNameId(this Entity entity)
        {
            var value = String.Empty;
            var nameAttribute = entity.GetNameAttribute();
            if (nameAttribute != String.Empty && entity.Contains(nameAttribute))
            {
                value = entity.Attributes[nameAttribute] as String ?? "NULL";
            }

            return value + String.Format(" ({0})", entity.Id);
        }

        /// <summary>
        /// Returns the value for the attribute, or the default value for the type if it does not exist in the entity.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        public static T GetOrDefault<T>(this Entity entity, string attributeName)
        {
            return entity.GetOrDefault(attributeName, default(T));
        }

        /// <summary>
        /// Returns the value for the attribute, or the default value for the type if it does not exist in the entity.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">The value to use if the entity does not contain the attribute.</param>
        /// <returns></returns>
        public static T GetOrDefault<T>(this Entity entity, string attributeName, T defaultValue)
        {
            T value;
            if (entity.Contains(attributeName))
            {
                value = (T)entity[attributeName];
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Creates the EntityReference from Entity, settings it's name
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityReference ToEntityReference(this Entity entity, string name)
        {
            var reference = entity.ToEntityReference();
            reference.Name = name;
            return reference;
        }

        /// <summary>
        /// Creates an array of attribute names array from an Anonymous Type Initializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousTypeInitializer">The anonymous type initializer.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">lambda must return an object initializer</exception>
        private static string[] GetAttributeNamesArray<T>(Expression<Func<T, object>> anonymousTypeInitializer) where T : Entity
        {
            var initializer = anonymousTypeInitializer.Body as NewExpression;
            if (initializer == null || initializer.Members == null)
                throw new ArgumentException("lambda must return an object initializer");

            // Search for and replace any occurence of Id with the actual Entity's Id
            return initializer.Members.Select(GetLogicalAttributeName<T>).ToArray();
        }

        /// <summary>
        /// Normally just returns the name of the property, in lowercase.  But Id must be looked up via reflection.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string GetLogicalAttributeName<T>(MemberInfo property) where T : Entity
        {
            var name = property.Name.ToLower();
            if (name == "id")
            {
                var attribute = typeof(T).GetProperty("Id").GetCustomAttributes<AttributeLogicalNameAttribute>().FirstOrDefault();
                if (attribute == null)
                {
                    throw new ArgumentException(property.Name + " does not contain an AttributeLogicalNameAttribute.  Unable to determine id");
                }

                name = attribute.LogicalName;
            }

            return name;
        }

        /// <summary>
        /// Converts an Earlybound Entity to a Typed Version
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Entity ToSdkEntity(this Entity entity)
        {
            var sdkEntity = new Entity(entity.LogicalName);
            sdkEntity.Attributes.AddRange(entity.Attributes);
            sdkEntity.EntityState = entity.EntityState;
            sdkEntity.FormattedValues.AddRange(entity.FormattedValues);
            sdkEntity.KeyAttributes.AddRange(entity.KeyAttributes);
            if (entity.Id != Guid.Empty)
            {
                sdkEntity.Id = entity.Id;
            }
            sdkEntity.RelatedEntities.AddRange(entity.RelatedEntities);
            sdkEntity.RowVersion = entity.RowVersion;

            return sdkEntity;
        }

        /// <summary>
        /// Iterates and displays the attributes listed in the entity's Attributes collection
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeFormat">The attribute format.</param>
        /// <returns></returns>
        public static String ToStringAttributes(this Entity entity, string attributeFormat = "[{0}]: {1}")
        {
            return String.Join(Environment.NewLine, entity.Attributes.Select(att =>
                String.Format(attributeFormat, att.Key, GetAttributeValue(att.Value))));
        }

        private static string GetAttributeValue(object value)
        {
            if (value == null)
            {
                return "Null";
            }

            var osv = value as OptionSetValue;
            if (osv != null)
            {
                return osv.Value.ToString(CultureInfo.InvariantCulture);
            }

            var entity = value as EntityReference;
            if (entity != null)
            {
                return entity.GetNameId();
            }

            return value.ToString();
        }

        /// <summary>
        /// Clone Entity (deep copy)
        /// </summary>
        /// <param name="source">source entity.</param>
        /// <returns>new cloned entity</returns>
        public static T Clone<T>(this T source) where T : Entity
        {
            return source == null ? null : source.Serialize().DeserializeEntity<T>();
        }

        #endregion // Entity

        #region EntityCollection

        /// <summary>
        /// Converts the entity collection into a list, casting each entity.
        /// </summary>
        /// <typeparam name="T">The type of Entity</typeparam>
        /// <param name="col">The collection to convert</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(this EntityCollection col) where T : Entity
        {
            if (typeof(T) == typeof(Entity))
            {
                // T is Entity.  No need to cast, just convert.
                return (List<T>)(object)col.Entities.ToList();
            }

            return col.Entities.Select(e => e.ToEntity<T>()).ToList();
        }

        #endregion // EntityCollection

        #region EntityImageCollection

        public static List<String> ToStringDebug(this EntityImageCollection images, string name)
        {
            var values = new List<String>();
            if (images != null && images.Count > 0)
            {
                values.Add("** " + name + " **");
                foreach (var image in images)
                {
                    if (image.Value == null || image.Value.Attributes.Count == 0)
                    {
                        values.Add("    Image[" + image.Key + "] " + image.Value + ": Empty");
                    }
                    else
                    {
                        values.Add("*   Image[" + image.Key + "] " + image.Value.ToEntityReference().GetNameId() + "   *");
                        values.Add(image.Value.ToStringAttributes("        Entity[{0}]: {1}"));
                    }
                }
            }
            else
            {
                values.Add(name + ": Empty");
            }

            return values;
        }

        #endregion // EntityImageCollection

        #region EntityReference

        /// <summary>
        /// Returns the Name and Id of an entity reference in this format "Name (id)"
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static String GetNameId(this EntityReference entity)
        {
            return String.Format("{0} ({1})", entity.Name, entity.Id);
        }

        /// <summary>
        /// Returns the Id of the entity reference or Guid.Empty if it is null"
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Guid GetIdOrDefault(this EntityReference entity)
        {
            return entity == null ? Guid.Empty : entity.Id;
        }

        /// <summary>
        /// Returns the Name of the entity reference or null if it is null"
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetNameOrDefault(this EntityReference entity)
        {
            return entity == null ? null : entity.Name;
        }

        public static String ToStringDebug(this EntityReference entity)
        {
            if (entity == null)
            {
                return "Null";
            }
            return String.Format("EntityReference {{ LogicalName: {0}, Name: {1}, Id: {2}}}", entity.LogicalName, entity.GetNameOrDefault(), entity.Id);
        }

        #endregion // EntityReference

        #region FilterExpression

        /// <summary>
        /// Depending on the Type of T, adds the correct is active criteria Statment
        /// Note: Use AddLink&lt;T&gt; for Linked Entities
        /// </summary>
        /// <typeparam name="T">The Entity type.</typeparam>
        /// <param name="fe"></param>
        public static FilterExpression ActiveOnly<T>(this FilterExpression fe) where T : Entity
        {
            return ActiveOnlyCore(fe, new ActivePropertyInfo<T>());
        }

        /// <summary>
        /// Depending on the Type of T, adds the correct is active criteria Statment
        /// Note: Use AddLink&lt;T&gt; for Linked Entities
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="logicalName">The logical name of the entity to have the active only enforced.</param>
        public static FilterExpression ActiveOnly(this FilterExpression fe, string logicalName)
        {
            return ActiveOnlyCore(fe, new LateBoundActivePropertyInfo(logicalName));
        }

        private static FilterExpression ActiveOnlyCore<T>(FilterExpression fe, ActivePropertyInfo<T> activeInfo) where T : Entity
        {
            switch (activeInfo.ActiveAttribute)
            {
                case ActiveAttributeType.IsDisabled:
                    fe.AddConditionEnforceAndFilterOperator(new ConditionExpression(
                            activeInfo.AttributeName, ConditionOperator.Equal, false));
                    break;
                case ActiveAttributeType.StateCode:
                    if (activeInfo.ActiveState.HasValue)
                    {
                        fe.StateIs(activeInfo.ActiveState.Value);
                    }
                    else
                    {
                        fe.AddConditionEnforceAndFilterOperator(new ConditionExpression(
                                activeInfo.AttributeName, ConditionOperator.NotEqual, activeInfo.NotActiveState.GetValueOrDefault(int.MinValue)));
                    }
                    break;
                case ActiveAttributeType.None:
                    break;
                default:
                    throw new EnumCaseUndefinedException<ActiveAttributeType>(activeInfo.ActiveAttribute);
            }
            return fe;
        }

        /// <summary>
        /// Adds the condition to the Filter Expression, and if the current filter expression's logical operator
        /// is an Or, it will move all of the current conditions into a child filter and create a new
        /// top level and filter
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="ce"></param>
        public static void AddConditionEnforceAndFilterOperator(this FilterExpression criteria, ConditionExpression ce)
        {
            if (criteria.FilterOperator == LogicalOperator.Or)
            {
                // Move the current filter criteria down and add the is active as an and filter join
                var fe = new FilterExpression(LogicalOperator.Or);

                // Move Conditions
                foreach (var condition in criteria.Conditions.ToList())
                {
                    fe.AddCondition(condition);
                    criteria.Conditions.Remove(condition);
                }

                // Move Filters
                foreach (var filter in criteria.Filters.ToList())
                {
                    fe.AddFilter(filter);
                    criteria.Filters.Remove(filter);
                }

                criteria.FilterOperator = LogicalOperator.And;
                criteria.AddCondition(ce);
                criteria.Filters.Add(fe);
            }
            else
            {
                criteria.AddCondition(ce);
            }
        }

        /// <summary>
        /// Adds the conditions to the FilterExpression as normal if the FilterOperator is an And.
        /// If it is an Or, adds the conditions to a child FilterExpression, with a FilterOperator of And.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="conditions"></param>
        public static void AddConditionsAnded(this FilterExpression criteria, params ConditionExpression[] conditions)
        {
            var fe = criteria;
            if (fe.FilterOperator == LogicalOperator.Or)
            {
                fe = new FilterExpression(LogicalOperator.And);
                criteria.AddFilter(fe);
            }

            foreach (var condition in conditions)
            {
                fe.AddCondition(condition);
            }
        }

        /// <summary>
        /// Adds a Condition expression to the filter expression to force the statecode to be a specfic value.
        /// </summary>
        /// <param name="fe">The Filter Expression.</param>
        /// <param name="entityStateEnum">The entity state enum value.</param>
        /// <returns>The Filter expression with the conditionexpression added</returns>
        public static FilterExpression StateIs(this FilterExpression fe, object entityStateEnum)
        {
            fe.AddConditionEnforceAndFilterOperator(new ConditionExpression("statecode", ConditionOperator.Equal, (int)entityStateEnum));
            return fe;
        }

        #endregion // FilterExpression

        #region IExecutionContext

        private static List<String> ToStringDebug(this IExecutionContext context)
        {
            var lines = new List<String>
            {
                "BusinessUnitId: " + context.BusinessUnitId,
                "CorrelationId: " + context.CorrelationId,
                "Depth: " + context.Depth,
                "InitiatingUserId: " + context.InitiatingUserId,
                "IsInTransaction: " + context.IsInTransaction,
                "IsolationMode: " + context.IsolationMode,
                "MessageName: " + context.MessageName,
                "Mode: " + context.Mode,
                "OperationCreatedOn: " + context.OperationCreatedOn,
                "OperationId: " + context.OperationId,
                "Organization: " + context.OrganizationName + "(" + context.OrganizationId + ")",
                "OwningExtension: " + (context.OwningExtension == null ? "Null" : context.OwningExtension.GetNameId()),
                "PrimaryEntityId: " + context.PrimaryEntityId,
                "PrimaryEntityName: " + context.PrimaryEntityName,
                "SecondaryEntityName: " + context.SecondaryEntityName,
                "UserId: " + context.UserId,
            };
            lines.AddRange(context.InputParameters.ToStringDebug("Input Parameters"));
            lines.AddRange(context.OutputParameters.ToStringDebug("Output Parameters"));
            lines.AddRange(context.PostEntityImages.ToStringDebug("PostEntityImages"));
            lines.AddRange(context.PreEntityImages.ToStringDebug("PreEntityImages"));
            lines.AddRange(context.SharedVariables.ToStringDebug("Shared Variables"));

            if (ConfigurationManager.AppSettings.AllKeys.Any())
            {
                lines.Add("* App Config Values *");
                lines.AddRange(ConfigurationManager.AppSettings.AllKeys.Select(key => "    [" + key + "]: " + ConfigurationManager.AppSettings[key]));
            }

            return lines;
        }

        #endregion IExecutionContext

        #region IPluginExecutionContext

        public static String ToStringDebug(this IPluginExecutionContext context)
        {
            var lines = ((IExecutionContext)context).ToStringDebug();
            lines.AddRange(new[]
            {
                "Has Parent Context: " + (context.ParentContext != null),
                "Stage: " + context.Stage
            });

            return String.Join(Environment.NewLine, lines);
        }

        #endregion IPluginExecutionContext

        #region IOrganizationService

        /// <summary>
        /// Assigns the supplied entity to the supplied user
        /// </summary>
        /// <param name="service"></param>
        /// <param name="target"></param>
        /// <param name="systemUser"></param>
        /// <returns>AssignResponse</returns>
        public static AssignResponse Assign(this IOrganizationService service, EntityReference target, EntityReference systemUser)
        {
            return (AssignResponse)service.Execute(new AssignRequest
            {
                Assignee = systemUser,
                Target = target
            });
        }

        /// <summary>
        /// Reassigns the owner of the entity to the new owner
        /// </summary>
        /// <param name="service"></param>
        /// <param name="itemToChangeOwnershipOf">Must have Logical Name and Id Populated</param>
        /// <param name="newOwnerId"></param>
        /// <returns></returns>
        public static AssignResponse Assign(this IOrganizationService service, Entity itemToChangeOwnershipOf, Guid newOwnerId)
        {
            return Assign(service, itemToChangeOwnershipOf.ToEntityReference(), newOwnerId);
        }

        /// <summary>
        /// Reassigns the owner of the entity to the new owner
        /// </summary>
        /// <param name="service"></param>
        /// <param name="itemToChangeOwnershipOf">Must have Logical Name and Id Populated</param>
        /// <param name="newOwnerId"></param>
        /// <returns></returns>
        public static AssignResponse Assign(this IOrganizationService service, EntityReference itemToChangeOwnershipOf, Guid newOwnerId)
        {
            return (AssignResponse)service.Execute(new AssignRequest
            {
                Target = itemToChangeOwnershipOf,
                Assignee = new EntityReference("systemuser", newOwnerId),
            });
        }

        /// <summary>
        /// Associates one or more entities to an entity.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        /// <param name="relationshipLogicalName"></param>
        /// <param name="entities"></param>
        public static void Associate(this IOrganizationService service, Entity entity, string relationshipLogicalName, params Entity[] entities)
        {
            var relationship = new Relationship(relationshipLogicalName);
            if (entity.LogicalName == entities.First().LogicalName)
            {
                relationship.PrimaryEntityRole = EntityRole.Referenced;
            }

            service.Associate(entity.LogicalName, entity.Id,
                relationship,
                new EntityReferenceCollection(entities.Select(e => e.ToEntityReference()).ToList()));
        }

        /// <summary>
        /// Associates one or more entities to an entity.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        /// <param name="relationshipLogicalName"></param>
        /// <param name="entities"></param>
        public static void Associate(this IOrganizationService service, EntityReference entity, string relationshipLogicalName, params EntityReference[] entities)
        {
            var relationship = new Relationship(relationshipLogicalName);
            if (entity.LogicalName == entities.First().LogicalName)
            {
                relationship.PrimaryEntityRole = EntityRole.Referenced;
            }

            service.Associate(entity.LogicalName, entity.Id,
                relationship,
                new EntityReferenceCollection(entities.ToList()));
        }

        /// <summary>
        /// Deletes the specified entity
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity to be deleted.</param>
        public static void Delete(this IOrganizationService service, Entity entity)
        {
            service.Delete(entity.LogicalName, entity.Id);
        }

        /// <summary>
        /// Deletes the specified entity  
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static void Delete(this IOrganizationService service, EntityReference entity)
        {
            service.Delete(entity.LogicalName, entity.Id);
        }

        /// <summary>
        /// Attempts to delete the entity with the given id. If it doesn't exist, false is returned
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity to delete if it exists.</param>
        public static void DeleteIfExists(this IOrganizationService service, Entity entity)
        {
            service.DeleteIfExists(entity.LogicalName, entity.Id);
        }

        /// <summary>
        /// Delete all active entities in the entity specified by the LogicalName and the Filter Expression
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="logicalName">The logical name of the entity that will be deleted.</param>
        /// <param name="fe">The filter expression to use to determine what records to delete.</param>
        /// <returns></returns>
        public static bool DeleteIfExists(this IOrganizationService service, string logicalName, FilterExpression fe)
        {
            var qe = new QueryExpression(logicalName) { Criteria = fe };
            return service.DeleteIfExists(qe);
        }

        /// <summary>
        /// Attempts to delete the entity with the given id. If it doesn't exist, false is returned
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The id of the entity to search and potentially delete.</param>
        /// <returns></returns>
        public static bool DeleteIfExists(this IOrganizationService service, string entityName, Guid id)
        {
            return DeleteIfExistsWithRetry(service, entityName, id, 0);
        }

        /// <summary>
        /// Delete all entities that are returned by the Query Expression.
        /// </summary>
        /// <param name="service">The Service</param>
        /// <param name="qe">The query expression used to define the set of entities to delete</param>
        /// <returns></returns>
        public static bool DeleteIfExists(this IOrganizationService service, QueryExpression qe)
        {
            var exists = false;
            var idName = EntityHelper.GetIdAttributeName(qe.EntityName);
            qe.ColumnSet = new ColumnSet(idName);
            qe.NoLock = true;
            var entities = service.RetrieveMultiple(qe);
            if (entities.Entities.Count > 0)
            {
                exists = true;
                entities.Entities.ToList().ForEach(e => service.Delete(qe.EntityName, e.Id));
            }
            return exists;
        }

        private static bool DeleteIfExistsInternal(IOrganizationService service, string logicalName, Guid id)
        {
            var exists = false;
            var idName = EntityHelper.GetIdAttributeName(logicalName);
            var qe = new QueryExpression(logicalName) { ColumnSet = new ColumnSet(idName) };

            qe.WhereEqual(idName, id);
            qe.First();
            qe.NoLock = true;
            if (service.RetrieveMultiple(qe).Entities.Count > 0)
            {
                service.Delete(logicalName, id);
                exists = true;
            }
            return exists;
        }

        /// <summary>
        /// There have been Generic SQL errors casued with calling this while using multi-threading.  This hopefully
        /// will fix that
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The id.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns></returns>
        private static bool DeleteIfExistsWithRetry(IOrganizationService service, string entityName, Guid id,
                                                    int retryCount)
        {
            bool exists;
            try
            {
                exists = DeleteIfExistsInternal(service, entityName, id);
            }
            catch (System.ServiceModel.FaultException<OrganizationServiceFault> ex)
            {
                if (retryCount < 10 && ex.Message.Equals("Generic SQL error.", StringComparison.CurrentCultureIgnoreCase))
                { // This is normally caused by database deadlock issue.  
                    // Attempt to reprocess once after sleeping a random number of milliseconds
                    System.Threading.Thread.Sleep(new Random(System.Threading.Thread.CurrentThread.ManagedThreadId).
                        Next(1000, 5000));
                    exists = DeleteIfExistsWithRetry(service, entityName, id, retryCount + 1);
                }
                else if (ex.Message.EndsWith(id + " Does Not Exist"))
                {
                    exists = false;
                }
                else if (ex.Message == "The object you tried to delete is associated with another object and cannot be deleted.")
                {
                    throw new Exception("Entity " + entityName + " (" + id + ") is associated with another object and cannot be deleted.");
                }
                else
                {
                    throw;
                }
            }

            return exists;
        }
        /// <summary>
        /// Executes a batch of requests against the CRM Web Service using the ExecuteMultipleRequest command.
        /// </summary>
        /// <remarks>
        /// ExecuteMultipleRequest allows for a maximum of 1000 messages to be processed in a single batch job.
        /// </remarks>
        /// <param name="service">Organization Service proxy for connecting to the relevant CRM instance.</param>
        /// <param name="requestCollection">Collection of organization requests to execute against the CRM Web Services.</param>
        /// <param name="returnResponses">Indicates if responses should be returned for the action taken on each entity in the bulk operation.</param>
        /// <param name="continueOnError">Indicates if the batch job should continue if an error occurs for any of the entities being processed. Default is true.</param>
        /// <returns>Returns the <see cref="ExecuteMultipleResponse"/> containing responses and faults from the operation if the returnResponses parameter is set to true; otherwise returns null. Default is true.</returns>
        public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService service, OrganizationRequestCollection requestCollection, bool returnResponses = true, bool continueOnError = true)
        {
            // Validate required parameters.
            if (service == null)
                throw new ArgumentNullException("service", "A valid Organization Service Proxy must be specified.");
            // Validate the request collection.
            if (requestCollection == null)
                throw new ArgumentNullException("requestCollection", "The collection of requests to batch process cannot be null.");
            // Ensure the user is not attempting to pass in more than 1000 requests for the batch job, as this is the maximum number CRM allows within a single batch.
            if (requestCollection.Count > 1000)
                throw new ArgumentOutOfRangeException("requestCollection", "The Entity Collection cannot contain more than 1000 items, as that is the maximum number of messages that can be processed by the CRM web services in a single batch.");

            try
            {
                // Instantiate a new ExecuteMultipleRequest.
                var multipleRequest = new ExecuteMultipleRequest
                {
                    Settings = new ExecuteMultipleSettings { ContinueOnError = continueOnError, ReturnResponses = returnResponses },
                    Requests = requestCollection
                };

                return service.Execute(multipleRequest) as ExecuteMultipleResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while executing an ExecuteMultipleRequest. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Gets all entities using the Query Expression
        /// </summary>
        /// <typeparam name="T">Type of Entity List to return</typeparam>
        /// <param name="service">The service.</param>
        /// <param name="qe">Query Expression to Execute.</param>
        /// <param name="maxCount">The maximum number of entities to retrieve.  Use null for default.</param>
        /// <param name="pageSize">Number of records to return in each fetch.  Use null for default.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAllEntities<T>(this IOrganizationService service, QueryExpression qe, int? maxCount = null, int? pageSize = null)
            where T : Entity
        {
            return RetrieveAllEntities<T>.GetAllEntities(service, qe, maxCount, pageSize);
        }

        /// <summary>
        /// Returns the WhoAmIResponse to determine the current user's UserId, BusinessUnitId, and OrganizationId
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static WhoAmIResponse GetCurrentlyExecutingUserInfo(this IOrganizationService service)
        {
            return (WhoAmIResponse)service.Execute(new WhoAmIRequest());
        }

        /// <summary>
        /// Retrieves the Entity of the given type with the given Id, with all columns
        /// </summary>
        /// <typeparam name="T">An early bound Entity Type</typeparam>
        /// <param name="service">open IOrganizationService</param>
        /// <param name="id">Primary Key of Entity</param>
        /// <returns></returns>
        public static T GetEntity<T>(this IOrganizationService service, Guid id)
            where T : Entity
        {
            return service.GetEntity<T>(id, new ColumnSet(true));
        }

        /// <summary>
        /// Retrieves the Entity of the given type with the given Id, with the given columns
        /// </summary>
        /// <typeparam name="T">An early bound Entity Type</typeparam>
        /// <param name="service">open IOrganizationService</param>
        /// <param name="id">Primary Key of Entity</param>
        /// <param name="anonymousTypeInitializer">An Anonymous Type Initializer where the properties of the anonymous
        /// type are the column names to add</param>
        /// <returns></returns>
        public static T GetEntity<T>(this IOrganizationService service, Guid id, Expression<Func<T, object>> anonymousTypeInitializer)
            where T : Entity
        {
            return service.GetEntity<T>(id, AddColumns(new ColumnSet(), anonymousTypeInitializer));
        }

        /// <summary>
        /// Retrieves the Entity of the given type with the given Id, with the given columns
        /// </summary>
        /// <typeparam name="T">An early bound Entity Type</typeparam>
        /// <param name="service">open IOrganizationService</param>
        /// <param name="id">Primary Key of Entity</param>
        /// <param name="columnSet">Columns to retrieve</param>
        /// <returns></returns>
        public static T GetEntity<T>(this IOrganizationService service, Guid id, ColumnSet columnSet)
            where T : Entity
        {
            return service.Retrieve(EntityHelper.GetEntityLogicalName<T>(), id, columnSet).ToEntity<T>();
        }

        /// <summary>
        /// Returns first 5000 entities using the Query Expression
        /// </summary>
        /// <typeparam name="T">Type of Entity List to return</typeparam>
        /// <param name="service">The service.</param>
        /// <param name="qe">Query Expression to Execute.</param>
        /// <returns></returns>
        public static List<T> GetEntities<T>(this IOrganizationService service, QueryExpression qe) where T : Entity
        {
            return service.RetrieveMultiple(qe).ToEntityList<T>();
        }

        /// <summary>
        /// Gets the first entity that matches the query expression.  An exception is thrown if none are found.
        /// </summary>
        /// <typeparam name="T">The Entity Type.</typeparam>
        /// <param name="service">The service.</param>
        /// <param name="qe">The query expression.</param>
        /// <returns></returns>
        public static T GetFirst<T>(this IOrganizationService service, QueryExpression qe) where T : Entity
        {
            var entity = service.GetFirstOrDefault<T>(qe);
            AssertExists(entity, qe);
            return entity;
        }

        // ReSharper disable once UnusedParameter.Local
        private static void AssertExists<T>(T entity, QueryExpression qe) where T : Entity
        {
            if (entity == null)
            {
                throw new InvalidOperationException("No " + EntityHelper.GetEntityLogicalName<T>() + " found for query " +
                                                    qe.GetSqlStatement());
            }
        }

        /// <summary>
        /// Gets the first entity that matches the query expression.  Null is returned if none are found.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="qe">The query expression.</param>
        /// <returns></returns>
        public static Entity GetFirstOrDefault(this IOrganizationService service, QueryExpression qe)
        {
            qe.First();
            return service.RetrieveMultiple(qe).Entities.FirstOrDefault();
        }

        /// <summary>
        /// Gets the first entity that is returened by the fetch expression.  Null is returned if none are found.
        /// </summary>
        /// <typeparam name="T">The Entity Type.</typeparam>
        /// <param name="service">The service.</param>
        /// <param name="fe">The fetch expression.</param>
        /// <returns></returns>
        public static T GetFirstOrDefault<T>(this IOrganizationService service, FetchExpression fe) where T : Entity
        {
            var entity = service.RetrieveMultiple(fe).Entities.FirstOrDefault();
            return entity == null ? null : entity.ToEntity<T>();
        }

        /// <summary>
        /// Gets the first entity that matches the query expression.  Null is returned if none are found.
        /// </summary>
        /// <typeparam name="T">The Entity Type.</typeparam>
        /// <param name="service">The service.</param>
        /// <param name="qe">The query expression.</param>
        /// <returns></returns>
        public static T GetFirstOrDefault<T>(this IOrganizationService service, QueryExpression qe) where T : Entity
        {
            qe.First();
            return service.GetEntities<T>(qe).FirstOrDefault();
        }

        /// <summary>
        /// Returns a unique string key for the given IOrganizationService
        /// If the service is remote, the uri will be used, including the org Name
        /// If the service is on the server (i.e. plugin), the org id will be used
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetOrganizationKey(this IOrganizationService service)
        {
            var uri = service.GetServiceUri();
            if (uri != null)
            {
                return uri.Host.ToLower() + "|" + uri.Segments[1].Replace(@"/", "|").ToLower();
            }

            // Already on the server, grab organization guid
            // Service should be of type Microsoft.Crm.Extensibility.InProcessServiceProxy which is an internal class.  Use reflection to grab the private field Org Id;
            var field = service.GetType().GetField("_organizationId", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                return String.Empty;
            }

            var orgId = field.GetValue(service).ToString();
            return "localOrgId" + orgId;
        }

        /// <summary>
        /// Retrieves the Organization Name from the URL being used by the IOrganizationService.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <remarks>This method will not support IFD</remarks>
        public static string GetOrganizationName(this IOrganizationService service)
        {
            var uri = service.GetServiceUri();
            if (uri == null)
            {
                throw new Exception("GetOrganizationName does not support In Process implementations of the IOrganizationService.");
            }
            return uri.Segments[1].Replace(@"/", "").ToLower();
        }

        /// <summary>
        /// Assumes that this service is of type ServiceProxy&gt;IOrganizationService&lt; or IIOrganizationServiceWrapper
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public static Uri GetServiceUri(this IOrganizationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var proxy = service as ServiceProxy<IOrganizationService>;
            if (proxy == null)
            {
                var wrapper = service as IIOrganizationServiceWrapper;
                if (wrapper == null)
                {
                    // Check for Xrm.Client.Services.OrganizationService
                    var innerService = service.GetType().GetProperty("InnerService");
                    if (innerService == null)
                    {
                        throw new ArgumentException("Unable to determine the Uri for the IOrganizationService of type " + service.GetType().FullName);
                    }
                    proxy = (ServiceProxy<IOrganizationService>)innerService.GetValue(service);
                }
                else
                {
                    return wrapper.GetServiceUri();
                }
            }

            if (proxy.ServiceConfiguration == null)
            {
                return null;
            }

            return proxy.ServiceConfiguration.CurrentServiceEndpoint.Address.Uri;
        }

        /// <summary>
        /// If the id of the entity is empty, then the entity will be created, if it is not, then the entity will be
        /// updated
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Guid Save(this IOrganizationService service, Entity entity)
        {
            Guid id = entity.Id;
            if (id == Guid.Empty)
            {
                id = service.Create(entity);
                entity.Id = id;
            }
            else
            {
                service.Update(entity);
            }
            return id;
        }

        /// <summary>
        /// Currently only tested against System Users.  Not sure if it will work with other entities
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity to set the state of.</param>
        /// <param name="state">The state to change the entity to.</param>
        /// <param name="status">The status to change the entity to.</param>
        /// <returns></returns>
        public static SetStateResponse SetState(this IOrganizationService service, Entity entity, int state, int? status)
        {
            var setStateReq = new SetStateRequest
            {
                EntityMoniker = entity.ToEntityReference(),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status ?? -1)
            };

            return (SetStateResponse)service.Execute(setStateReq);
        }

        /// <summary>
        /// Currently only tested against System Users.  Not sure if it will work with other entities
        /// May need to rename this to SetSystemUserState
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="logicalName">logical name of the entity.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
        /// <returns></returns>
        public static SetStateResponse SetState(this IOrganizationService service, string logicalName, Guid id, bool active)
        {
            var info = new LateBoundActivePropertyInfo(logicalName);
            var state = active ?
                    info.ActiveState ?? 0 :
                    info.NotActiveState ?? (info.ActiveState == 1 ? 0 : 1);


            var setStateReq = new SetStateRequest
            {
                EntityMoniker = new EntityReference(logicalName, id),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(-1)
            };

            return (SetStateResponse)service.Execute(setStateReq);
        }

        /// <summary>
        /// Attempts to delete the Entity, eating the error if it doesn't exist
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="logicalName">Logical name of the entity.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static bool TryDelete(this IOrganizationService service, string logicalName, Guid id)
        {
            var exists = false;
            try
            {
                service.Delete(logicalName, id);
                exists = true;
            }
            catch (System.ServiceModel.FaultException<OrganizationServiceFault> ex)
            {
                if (!ex.Message.EndsWith(id + " Does Not Exist"))
                {
                    throw;
                }
            }

            return exists;
        }

        #endregion // IOrganizationService

        #region Label

        public static string GetLocalOrDefaultText(this Label label, string defaultIfNull = null)
        {
            var local = label.UserLocalizedLabel ?? label.LocalizedLabels.FirstOrDefault();

            if (local == null)
            {
                return defaultIfNull;
            }

            return local.Label ?? defaultIfNull;
        }

        #endregion // Label

        #region LinkEntity

        public static LinkEntity StateIs(this LinkEntity link, object entityStateEnum)
        {
            link.LinkCriteria.StateIs(entityStateEnum);
            return link;
        }

        #endregion // LinkEntity

        #region OptionSetValue

        /// <summary>
        /// Returns the value of the OptionSetValue, or int.MinValue if it is null
        /// </summary>
        /// <param name="osv"></param>
        /// <returns></returns>
        public static int GetValueOrDefault(this OptionSetValue osv)
        {
            return GetValueOrDefault(osv, int.MinValue);
        }

        /// <summary>
        /// Returns the value of the OptionSetValue, or int.MinValue if it is null
        /// </summary>
        /// <param name="osv">The OptionSetValue.</param>
        /// <param name="defaultValue">The value to default the OptionSetValue's Value to if it is null.</param>
        /// <returns></returns>
        public static int GetValueOrDefault(this OptionSetValue osv, int defaultValue)
        {
            return osv == null ? defaultValue : osv.Value;
        }

        /// <summary>
        /// Allows for Null safe Equals Comparison for more concise code.  i.e.
        /// if(contact.GenderCode.NullSafeEquals(1))
        /// vs.
        /// if(contact.GenderCode != null &amp;&amp; contact.gendercode.Value == 1)
        /// </summary>
        /// <param name="osv"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NullSafeEquals(this OptionSetValue osv, int value)
        {
            return osv != null && osv.Value == value;
        }

        /// <summary>
        /// Allows for Null safe Equals Comparison for more concise code.  i.e.
        /// if(contact.GenderCode.NullSafeEquals(new OptionSet(1)))
        /// vs.
        /// if(contact.GenderCode != null && contact.gendercode.Value == new OptionSet(1))
        /// </summary>
        /// <param name="osv"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NullSafeEquals(this OptionSetValue osv, OptionSetValue value)
        {
            return osv != null && osv.Equals(value);
        }

        #endregion // OptionSetValue

        #region OrganizationRequestCollection

        public static void AddCreate<T>(this OrganizationRequestCollection requests, T entity) where T : Entity
        {
            requests.Add(new CreateRequest { Target = entity });
        }

        public static void AddDelete(this OrganizationRequestCollection requests, EntityReference entity)
        {
            requests.Add(new DeleteRequest { Target = entity });
        }

        public static void AddDelete<T>(this OrganizationRequestCollection requests, T entity) where T : Entity
        {
            requests.Add(new DeleteRequest { Target = entity.ToEntityReference() });
        }

        public static void AddRetrieve<T>(this OrganizationRequestCollection requests, Guid id) where T : Entity
        {
            requests.AddRetrieve<T>(id, new ColumnSet(true));
        }

        public static void AddRetrieve<T>(this OrganizationRequestCollection requests, Guid id, ColumnSet cs) where T : Entity
        {
            requests.Add(new RetrieveRequest { Target = new EntityReference(EntityHelper.GetEntityLogicalName<T>(), id), ColumnSet = cs });
        }

        public static void AddRetrieve<T>(this OrganizationRequestCollection requests, Guid id, Expression<Func<T, object>> anonymousTypeInitializer) where T : Entity
        {
            var cs = AddColumns(new ColumnSet(), anonymousTypeInitializer);
            requests.Add(new RetrieveRequest { Target = new EntityReference(EntityHelper.GetEntityLogicalName<T>(), id), ColumnSet = cs });
        }

        public static void AddRetrieveMultiple(this OrganizationRequestCollection requests, QueryExpression qe)
        {
            requests.Add(new RetrieveMultipleRequest { Query = qe });
        }

        public static void AddUpdate<T>(this OrganizationRequestCollection requests, T entity) where T : Entity
        {
            requests.Add(new UpdateRequest { Target = entity });
        }

        #endregion // OrganizationRequestCollection

        #region ParameterCollection

        /// <summary>
        /// Checks to see if the ParameterCollection Contains the attribute names, and the value is not null
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static bool ContainsAllNonNull(this ParameterCollection parameters, params string[] attributeNames)
        {
            return attributeNames.All(name => parameters.Contains(name) && parameters[name] != null);
        }

        /// <summary>
        /// Gets the parameter value from the collection, cast to type 'T', or default(T) if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The ParameterCollection.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public static T GetParameterValue<T>(this ParameterCollection parameters, string parameterName)
        {
            var attributeValue = parameters.GetParameterValue(parameterName);
            return attributeValue == null ? default(T) : (T)attributeValue;
        }

        /// <summary>
        /// Gets the parameter value from the collection, or null if the collection doesn't contain a parameter with the given name.
        /// </summary>
        /// <param name="parameters">The ParameterCollection.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">parameterName</exception>
        public static object GetParameterValue(this ParameterCollection parameters, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }
            return parameters.Contains(parameterName) ? parameters[parameterName] : null;
        }

        public static List<String> ToStringDebug(this ParameterCollection parameters, string name)
        {
            var values = new List<String>();
            if (parameters != null && parameters.Count > 0)
            {
                values.Add("* " + name + " *");
                foreach (var param in parameters)
                {
                    var entity = param.Value as Entity;
                    var entityRef = param.Value as EntityReference;
                    var entityRefCollection = param.Value as EntityReferenceCollection;
                    var enumerable = param.Value as IEnumerable;
                    var entities = param.Value as EntityCollection;
                    var optionSet = param.Value as OptionSetValue;
                    if (entity != null)
                    {
                        values.Add(entity.ToStringAttributes(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "][{0}]: {1}"));
                    }
                    if (entityRef != null)
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "]: " + entityRef.ToStringDebug());
                    }
                    else if (entities != null)
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "] Entity Collection " + entities.EntityName + ":");
                        values.AddRange(entities.Entities.Select(i => GenerateNonBreakingSpace(8) + i.ToStringAttributes()));
                    }
                    else if (entityRefCollection != null)
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "] Entity Reference Collection:");
                        values.AddRange(entityRefCollection.Select(i => GenerateNonBreakingSpace(8) + i.ToStringDebug()));
                    }
                    else if (enumerable != null && !(param.Value is String))
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "]: " + param.Value.GetType().FullName + ":");
                        values.AddRange(from object item in enumerable select GenerateNonBreakingSpace(8) + item);
                    }
                    else if (optionSet != null)
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "]: " + optionSet.Value);
                    }
                    else
                    {
                        values.Add(GenerateNonBreakingSpace(4) + "Param[" + param.Key + "]: " + param.Value);
                    }
                }
            }
            else
            {
                values.Add(name + ": Empty");
            }
            return values;
        }

        private static string GenerateNonBreakingSpace(int spaces)
        {
            const string space = " "; // This is not a space, it is a Non-Breaking Space (alt+255).  In the log things get trimmed, and this will prevent that from happening;
            return new string(space[0], spaces);
        }

        #endregion // ParameterCollection

        #region QueryExpression

        /// <summary>
        /// Depending on the Type of T, adds the correct is active criteria Statement
        /// Note: Does not work for Linked Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qe"></param>
        public static QueryExpression ActiveOnly<T>(this QueryExpression qe) where T : Entity
        {
            qe.Criteria.ActiveOnly<T>();
            return qe;
        }

        /// <summary>
        /// Depending on the Type of T, adds the correct is active criteria Statement
        /// Note: Does not work for Linked Entities
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="logicalName">The logical name of the entity to have the active only enforced.</param>
        public static QueryExpression ActiveOnly(this QueryExpression qe, string logicalName)
        {
            qe.Criteria.ActiveOnly(logicalName);
            return qe;
        }

        public static void First(this QueryExpression query)
        {
            query.PageInfo.Count = 1;
            query.PageInfo.PageNumber = 1;
        }

        public static QueryExpression StateIs(this QueryExpression qe, object entityStateEnum)
        {
            qe.Criteria.StateIs(entityStateEnum);
            return qe;
        }

        /// <summary>
        /// Updates the Query Expression to only return only the first entity that matches the query expression expression criteria.
        /// Shortcut for setting the Query's PageInfo.Count and PageInfo.PageNumber to 1.
        /// </summary>
        /// <param name="qe">The query.</param>
        /// <param name="count">The count of entities to restrict the result of the query to.</param>
        public static QueryExpression Take(this QueryExpression qe, int count)
        {
            if (count > 5000)
            {
                throw new ArgumentException("Count must be 5000 or less", "count");
            }
            qe.PageInfo.Count = count;
            qe.PageInfo.PageNumber = 1;

            return qe;
        }

        #endregion // QueryExpression

        #region String

        /// <summary>
        /// Deserializes the entity from a string xml value to a specific entity type.
        /// </summary>
        /// <typeparam name="T">The type of entity to deserialize the xml to.</typeparam>
        /// <param name="xml">The xml to deserialize.</param>
        /// <returns></returns>
        public static T DeserializeEntity<T>(this string xml) where T : Entity
        {
            var entity = xml.DeserializeEntity();
            return entity == null ? null : entity.ToEntity<T>();
        }

        /// <summary>
        /// Deserializes the entity from a string xml value to an IExtensibleDataObject
        /// </summary>
        /// <param name="xml">The xml to deserialize.</param>
        /// <returns></returns>
        public static Entity DeserializeEntity(this string xml)
        {
            var serializer = new NetDataContractSerializer();
            var entity = ((Entity)(serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(xml)))));
            return entity;
        }

        /// <summary>
        /// Deserializes the json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">text</exception>
        public static T DeserializeJson<T>(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            using (var reader = new MemoryStream(Encoding.Default.GetBytes(text)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(reader);
            }
        }

        #endregion // String
    }
}
