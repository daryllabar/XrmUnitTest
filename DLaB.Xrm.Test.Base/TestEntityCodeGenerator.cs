#nullable enable
using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Attempts to record all data required to make queries work so the required objects can be created as a part of the test.
    /// </summary>
    public class TestEntityCodeGenerator
    {
        private readonly Assembly _earlyBoundAssembly;
        private readonly string _namespace;

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="earlyBoundAssembly">The early bound assembly.</param>
        /// <param name="namespace">The namespace for the generated code.</param>
        public TestEntityCodeGenerator(Assembly earlyBoundAssembly, string @namespace)
        {
            _earlyBoundAssembly = earlyBoundAssembly;
            _namespace = @namespace;
        }

        /// <summary>
        /// Generates code for the recorded entities using the specified type derived from OrganizationServiceContext.
        /// </summary>
        /// <param name="entities">The entities to generate code for.</param>
        /// <typeparam name="TContext">The type derived from OrganizationServiceContext.</typeparam>
        /// <returns>The generated code as a string.</returns>
        /// <exception cref="Exception">Thrown when the type is OrganizationServiceContext.</exception>
        public static string GenerateCode<TContext>(Dictionary<EntityReference, Entity> entities) where TContext : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
        {
            var type = typeof(TContext);
            if (type == typeof(Microsoft.Xrm.Sdk.Client.OrganizationServiceContext))
            {
                throw new Exception("Cannot generate code for OrganizationServiceContext.  Please provide a type derived from OrganizationServiceContext, generated as apart of early bound generation.");
            }

            return new TestEntityCodeGenerator(type.Assembly, type.Namespace ?? string.Empty).GenerateCode(entities);
        }

        /// <summary>
        /// Generates code for the recorded entities.
        /// </summary>
        /// <param name="entities">The entities to generate code for.</param>
        /// <returns>The generated code as a string.</returns>
        public string GenerateCode(Dictionary<EntityReference, Entity> entities)
        {
            var sb = new StringBuilder();
            var countByEntity = new Dictionary<string, int>();
            foreach (var entity in entities.Values)
            {
                var type = GetType(entity.LogicalName);
                if (type == null)
                {
                    sb.AppendLine($"// Unable to find EarlyBound Type for {entity.LogicalName}.  Skipping Generation!");
                    continue;
                }

                if (countByEntity.TryGetValue(entity.LogicalName, out var count))
                {
                    countByEntity[entity.LogicalName] = count + 1;
                }
                else
                {
                    countByEntity[entity.LogicalName] = 1;
                }

                GenerateCodeForEntity(sb, entity, type, countByEntity[entity.LogicalName]);
            }

            return sb.ToString();
        }

        private Type? GetType(string entityLogicalName)
        {
            return EntityHelper.IsTypeDefined(_earlyBoundAssembly, _namespace, entityLogicalName)
                ? EntityHelper.GetType(_earlyBoundAssembly, _namespace, entityLogicalName)
                : null;
        }

        private void GenerateCodeForEntity(StringBuilder sb, Entity entity, Type type, int count)
        {
            var tab = "\t";
            sb.AppendLine($"var {entity.LogicalName}{count} = new {type.Name} {{");
            foreach (var att in entity.Attributes.OrderBy(a => a.Key))
            {
                var attributeName = att.Key;
                var attributeValue = att.Value;

                if (attributeValue is AliasedValue)
                {
                    continue;
                }

                var propertyName = type.GetProperties()
                                       .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == attributeName)
                                       ?.Name;

                if (attributeValue == null)
                {
                    continue;
                }

                if (attributeValue.GetType().IsEnum)
                {
                    var enumType = attributeValue.GetType();
                    sb.Append($"{tab}{propertyName} = {enumType.Name}.{Enum.GetName(enumType, attributeValue)},");
                }
                else if (attributeValue is OptionSetValue)
                {
                    propertyName = type.GetProperties()
                        .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == attributeName
                            && (p.PropertyType.IsEnum || p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum))?.Name ?? propertyName;
                }

                sb.Append($"{tab}{propertyName} = {(dynamic)GetInitValue(attributeName, attributeValue, type)},");
                sb.AppendLine();
            }

            sb.AppendLine("};");
        }

        #region Code Gen Init Value

        private string GetInitValue(string name, dynamic value, Type entityType)
        {
            if (value == null)
            {
                return "null";
            }

            try
            {
                return GetInitString(name, value, entityType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to get Init Value for type {value.GetType()} and value {value}.", ex);
            }
        }

        // ReSharper disable UnusedParameter.Local
        private string GetInitString(string name, bool bol, Type entityType)
        {
            return bol.ToString().ToLower();
        }

        private string GetInitString(string name, byte[] bytes, Type entityType)
        {
            return $"new byte[]{{ {string.Join(", ", bytes)} }}";
        }

        // ReSharper disable once RedundantAssignment
        private string GetInitString(string name, EntityReference reference, Type entityType)
        {
            var type = GetType(reference.LogicalName);
            return type == null
                ? $"null /* Unable to find EarlyBound Type for {reference.LogicalName}.  Skipping Generation! */"
                : $"new EntityReference({type.Name}.EntityLogicalName, new Guid(\"{reference.Id}\"))";
        }

        private string GetInitString(string name, DateTime date, Type entityType)
        {
            return $"new DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second})";
        }

        private string GetInitString(string name, Guid id, Type entityType)
        {
            return $"new Guid(\"{id}\")";
        }

        private string GetInitString(string name, Money money, Type entityType)
        {
            return $"new Money({money.Value})";
        }

        private string GetInitString(string name, OptionSetValue osv, Type entityType)
        {
            var property = entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == name
                && (p.PropertyType.IsEnum || p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum));

            if (property == null)
            {
                return $"new OptionSetValue({osv.Value}";
            }

            var type = property.PropertyType.IsEnum
                ? property.PropertyType
                : property.PropertyType.GenericTypeArguments.First();
            return $"{type.Name}.{Enum.GetName(type, osv.Value)}";
        }

        private string GetInitString(string name, string str, Type entityType)
        {
            return "@\"" + str.Replace("\"", "\"\"") + "\"";

        }

        private string GetInitString(string name, object obj, Type entityType)
        {
            return obj.ToString()!;
        }

        // ReSharper restore UnusedParameter.Local

        #endregion Code Gen Init Value
    }
}
