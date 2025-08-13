using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;
using System.Reflection;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{

    /// <summary>
    /// Provides functionality to set non-public properties or fields of a target object using reflection.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    public class NonPublicSetter<TTarget>
        where TTarget : class
    {
        /// <summary>
        /// Gets or sets the target object whose members will be set.
        /// </summary>
        public TTarget Value { get; set; }

        private readonly Type _targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonPublicSetter{TTarget}"/> class.
        /// </summary>
        /// <param name="target">The target object to operate on.</param>
        public NonPublicSetter(TTarget target)
        {
            Value = target;
            _targetType = target.GetType();
        }

        /// <summary>
        /// Sets the value of a non-public property or field specified by the selector expression.
        /// </summary>
        /// <typeparam name="TProp">The type of the property or field to set.</typeparam>
        /// <param name="selector">An expression selecting the property or field to set.</param>
        /// <param name="value">The value to assign to the property or field.</param>
        /// <returns>The current <see cref="NonPublicSetter{TTarget}"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the member cannot be set.</exception>
        public NonPublicSetter<TTarget> Set<TProp>(Expression<Func<TTarget, TProp>> selector, TProp value)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var member = GetMember(selector);
            switch (member)
            {
                case PropertyInfo prop:
                    // 1) Non-public setter
                    var setMethod = prop.GetSetMethod(nonPublic: true);
                    if (setMethod != null)
                    {
                        setMethod.Invoke(Value, new object[] { value });
                        return this;
                    }

                    // 2) Backing field <PropertyName>k__BackingField
                    var backing = FindField(_targetType, $"<{prop.Name}>k__BackingField");
                    if (backing != null)
                    {
                        backing.SetValue(Value, value);
                        return this;
                    }

                    break;

                case FieldInfo field:
                    // Direct field (if any)
                    field.SetValue(Value, value);
                    return this;
            }

            if (Value is OrganizationRequest orgRequest)
            {
                orgRequest.Parameters[member.Name] = value;
                return this;
            }

            if (Value is OrganizationResponse orgResponse)
            {
                orgResponse.Results[member.Name] = value;
                return this;
            }

            throw new InvalidOperationException(
                $"Could not set member '{member.Name}' on type '{_targetType.FullName}'.");
        }

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> from a lambda expression selecting a member.
        /// </summary>
        /// <param name="expr">The lambda expression selecting the member.</param>
        /// <returns>The <see cref="MemberInfo"/> of the selected member.</returns>
        /// <exception cref="ArgumentException">Thrown if the selector is not a member access.</exception>
        private static MemberInfo GetMember(LambdaExpression expr)
        {
            var body = expr.Body is UnaryExpression u && u.NodeType == ExpressionType.Convert
                ? u.Operand
                : expr.Body;

            if (body is MemberExpression m) return m.Member;
            throw new ArgumentException("Selector must be a member access, e.g., x => x.Property.");
        }

        /// <summary>
        /// Finds a field by name in the specified type or its base types.
        /// </summary>
        /// <param name="type">The type to search for the field.</param>
        /// <param name="name">The name of the field to find.</param>
        /// <returns>The <see cref="FieldInfo"/> if found; otherwise, <c>null</c>.</returns>
        private static FieldInfo FindField(Type type, string name)
        {
            // Walk base types to find non-public instance fields
            while (type != null)
            {
                var f = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (f != null) return f;
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Implicitly converts a <see cref="NonPublicSetter{TTarget}"/> to its underlying target object.
        /// </summary>
        /// <param name="value">The <see cref="NonPublicSetter{TTarget}"/> instance.</param>
        /// <returns>The underlying target object.</returns>
        public static implicit operator TTarget(NonPublicSetter<TTarget> value)
        {
            return value.Value;
        }
    }
}