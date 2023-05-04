using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DLaB.Common;

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
        public EntityMetadata Generate(Type entityType, string logicalName, string primaryName, int languageCode)

        {
            var name = new LocalizedLabel(logicalName, languageCode);
            var metadata = new EntityMetadata
            {
                DisplayCollectionName = new Label(name,
                    new[]
                    {
                        name
                    })
            };
            var type = typeof(EntityMetadata);
            var attributeMetadata = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => new AttributeInfo(p))
                .Where(a => a.LogicalName != null)
                .Distinct(new LogicalAttributeNameComparer())
                .Select(att => CreateAttributeMetadata(att.Name, att.PropertyType, att.LogicalName, languageCode, primaryName));
            type.GetProperty(nameof(metadata.PrimaryNameAttribute))?.SetValue(metadata, primaryName);
            type.GetProperty(nameof(metadata.Attributes))?.SetValue(metadata, attributeMetadata.ToArray());

            return metadata;
        }

        private class AttributeInfo
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
            public string LogicalName { get; set; }

            public AttributeInfo(PropertyInfo property)
            {
                Name = property.Name;
                PropertyType = property.PropertyType;
                LogicalName = property.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName;
            }
        }


        private class LogicalAttributeNameComparer : IEqualityComparer<AttributeInfo>
        {
            public bool Equals(AttributeInfo x, AttributeInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
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

        private AttributeMetadata CreateAttributeMetadata(string name, Type propertyType, string logicalName, int languageCode, string primaryName)
        {
            AttributeMetadata attribute;
            if (propertyType == typeof(string))
            {
                attribute = name == primaryName
                    ? (AttributeMetadata)new EntityNameAttributeMetadata(logicalName)
                    : new StringAttributeMetadata(logicalName);
            }
#if !PRE_KEYATTRIBUTE && !XRM_2015 && !XRM_2016
            else if (propertyType.IsGenericType
                     && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                     && propertyType.GetGenericArguments().Length == 1
                     && (propertyType.GetGenericArguments()[0].IsEnum || propertyType.GetGenericArguments()[0] == typeof(OptionSetValue)) )
            {
                var entity = new Entity();
                entity.KeyAttributes = null;

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
            else if (propertyType.IsEnum || Nullable.GetUnderlyingType(propertyType)?.IsEnum == true || propertyType == typeof(OptionSetValue))
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
                    attribute = new PicklistAttributeMetadata(logicalName);
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
            attribute.DisplayName = new Label(label, new[] { label });

            return attribute;
        }
    }
}
