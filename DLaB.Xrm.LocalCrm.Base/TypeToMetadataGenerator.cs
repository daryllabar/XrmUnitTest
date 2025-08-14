using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DLaB.Common;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif


namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Generates Entity Metadata using reflection on an early bound entity type
    /// </summary>
    public class TypeToMetadataGenerator
    {

        /// <summary>
        /// Converts a given Type to EntityMetadata.
        /// </summary>
        /// <param name="entityType">The Type to convert to EntityMetadata.</param>
        /// <param name="logicalName">The logical name of the entity.</param>
        /// <param name="primaryName">The primary name of the entity.</param>
        /// <param name="languageCode">The language code.</param>
        /// <returns>The generated EntityMetadata based on the given Type.</returns>
        public static EntityMetadata Generate(Type entityType, string logicalName, string primaryName, int languageCode)

        {
            var name = new LocalizedLabel(logicalName, languageCode);
            var metadata = new EntityMetadata
            {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0300 // Use collection expression for array
                DisplayCollectionName = new Label(name,
                    new[]
                    {
                        name
                    })
#pragma warning restore IDE0300
#pragma warning restore IDE0079
            };
            var type = typeof(EntityMetadata);
            var attributeMetadata = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => new AttributeInfo(p))
                .Where(a => a.LogicalName != null)
                .OrderByDescending(a => a.PropertyType.IsEnum)
                .Distinct(new LogicalAttributeNameComparer())
                .Select(att => CreateAttributeMetadata(entityType, att.Name, att.PropertyType, att.LogicalName, languageCode, primaryName));
            type.GetProperty(nameof(metadata.PrimaryNameAttribute))?.SetValue(metadata, primaryName);
            type.GetProperty(nameof(metadata.Attributes))?.SetValue(metadata, attributeMetadata.OrderBy(m => m.LogicalName).ToArray());

            return metadata;
        }

        private class AttributeInfo(PropertyInfo property)
        {
            public string Name { get; } = property.Name;
            public Type PropertyType { get; } = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            public string LogicalName { get;  } = property.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName;
        }


        private class LogicalAttributeNameComparer : IEqualityComparer<AttributeInfo>
        {
            public bool Equals(AttributeInfo x, AttributeInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null) return false;
                if (y is null) return false;
                if (x.GetType() != y.GetType()) return false;

                return x.LogicalName == y.LogicalName;
            }

            public int GetHashCode(AttributeInfo obj)
            {
                return obj.LogicalName != null
                    ? obj.LogicalName.GetHashCode()
                    : 0;
            }
        }

        private static AttributeMetadata CreateAttributeMetadata(Type entityType, string name, Type propertyType, string logicalName, int languageCode, string primaryName)
        {
            AttributeMetadata attribute;
            if (propertyType == typeof(string))
            {
                attribute = name == primaryName
                    ? new EntityNameAttributeMetadata(logicalName)
                    : new StringAttributeMetadata(logicalName);
            }
#if !PRE_MULTISELECT
            else if (propertyType.IsGenericType
                     && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                     && propertyType.GetGenericArguments().Length == 1
                     && (propertyType.GetGenericArguments()[0].IsEnum || propertyType.GetGenericArguments()[0] == typeof(OptionSetValue)) )
            {
                attribute = new MultiSelectPicklistAttributeMetadata(logicalName);
            }
            else if (propertyType == typeof(byte[]))
            {
                attribute = new FileAttributeMetadata(logicalName);
                //attribute = new ImageAttributeMetadata(logicalName);
            }
#endif
            else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                attribute = new BooleanAttributeMetadata(logicalName);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                attribute = new DateTimeAttributeMetadata(DateTimeFormat.DateAndTime, logicalName);
            }
            else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            {
                attribute = new DecimalAttributeMetadata(logicalName);
            }
            else if (propertyType == typeof(double) || propertyType == typeof(double))
            {
                attribute = new DoubleAttributeMetadata(logicalName);
            }
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
            {
                attribute = new IntegerAttributeMetadata(logicalName);
                //attribute = new BigIntAttributeMetadata(logicalName);
            }
            else if (propertyType == typeof(EntityReference))
            {
                attribute = new LookupAttributeMetadata();
            }
            //else if (propertyType == typeof(?))
            //{
            //  attributed = new MemoAttributeMetadata(logicalName);
            //}
            else if (propertyType == typeof(Money))
            {
                attribute = new MoneyAttributeMetadata(logicalName);
            }
            else if (propertyType.IsEnum || propertyType == typeof(OptionSetValue))
            {
                if (logicalName == "statecode")
                {
                    attribute = new StateAttributeMetadata();
                }
                else if (logicalName == "statuscode")
                {
                    attribute = new StatusAttributeMetadata();
                }
                else
                {
                    var picklist = new PicklistAttributeMetadata(logicalName)
                    {
                        OptionSet = new OptionSetMetadata
                        {
                            Name = propertyType.Name,
                            DisplayName = new Label(propertyType.Name.SpaceOutCamelCase(), languageCode)
                            {
                                UserLocalizedLabel = new LocalizedLabel(propertyType.Name.SpaceOutCamelCase(), languageCode)
                            }
                        }
                    };
                    attribute = picklist; 
                    if (propertyType.IsEnum)
                    {
                        var options = picklist.OptionSet.Options;
                        foreach (var value in Enum.GetValues(propertyType))
                        {
                            var intValue = (int)value;
                            var nameValue = Enum.GetName(propertyType, value) ?? "Unknown";
                            var option = new OptionMetadata(
                                new Label(nameValue.SpaceOutCamelCase(), languageCode)
                                {
                                    UserLocalizedLabel = new LocalizedLabel(nameValue.SpaceOutCamelCase(), languageCode)
                                },
                                intValue
                            );
                            options.Add(option);
                        }
                    }
                }
            }
            //else if (propertyType == typeof(?))
            //{
            //  attribute = new ManagedPropertyAttributeMetadata(logicalName);
            //}
#if !PRE_KEYATTRIBUTE
            else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
            {
                attribute = new UniqueIdentifierAttributeMetadata(logicalName);
            }
#endif
            else
            {
                // Default to string
                attribute = new StringAttributeMetadata(logicalName);
            }

            attribute.LogicalName = logicalName;
            attribute.SchemaName = name;
            var label = new LocalizedLabel(name.SpaceOutCamelCase(), languageCode);
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0300 // Use collection expression for array
            attribute.DisplayName = new Label(label, new[] { label });
#pragma warning restore IDE0300
#pragma warning restore IDE0079

            attribute.WithPrivate().Set(a => a.EntityLogicalName, EntityHelper.GetEntityLogicalName(entityType));
            return attribute;
        }
    }
}
