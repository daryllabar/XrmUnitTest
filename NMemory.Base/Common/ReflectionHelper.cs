﻿// ----------------------------------------------------------------------------------
// <copyright file="ReflectionHelper.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ReflectionHelper
    {
        public static Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        public static object GetMemberValue(MemberInfo member, object source)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(source);
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetValue(source, null);
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo", "member");
            }
        }

        public static MethodInfo GetMethodInfo<TClass>(Expression<Action<TClass>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method;
        }

        public static MethodInfo GetStaticMethodInfo<TResult>(Expression<Func<TResult>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method;
        }

        public static PropertyInfo GetPropertyInfo<TClass, TResult>(Expression<Func<TClass, TResult>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static string GetMemberName<TClass, TResult>(Expression<Func<TClass, TResult>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member.Name;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static string GetMethodName<T>(Expression<Func<T, object>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method.Name;
        }

        public static Type GetUnderlyingIfNullable(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static bool IsAnonymousType(Type type)
        {
            return CheckIfAnonymousType(type)
                ||
                (!typeof(IComparable).IsAssignableFrom(type) &&
                type.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false)
                    .Cast<DebuggerDisplayAttribute>()
                    .Any(m => m.Type == "<Anonymous Type>"));
        }

        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static bool IsNullable(Type type)
        {
            return
                !type.IsValueType ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsTuple(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type t = type.GetGenericTypeDefinition();

            return
                t == typeof(Tuple<>) ||
                t == typeof(Tuple<,>) ||
                t == typeof(Tuple<,,>) ||
                t == typeof(Tuple<,,,>) ||
                t == typeof(Tuple<,,,,>) ||
                t == typeof(Tuple<,,,,,>) ||
                t == typeof(Tuple<,,,,,,>) ||
                t == typeof(Tuple<,,,,,,,>);
        }

        public static Type GetTupleType(int count)
        {
            switch (count)
            {
                case 1:
                    return typeof(Tuple<>);
                case 2:
                    return typeof(Tuple<,>);
                case 3:
                    return typeof(Tuple<,,>);
                case 4:
                    return typeof(Tuple<,,,>);
                case 5:
                    return typeof(Tuple<,,,,>);
                case 6:
                    return typeof(Tuple<,,,,,>);
                case 7:
                    return typeof(Tuple<,,,,,,>);
                default:
                    throw new ArgumentException("", "count");
            }
        }

        public static Type GetTupleType(params Type[] elementTypes)
        {
            Type tuple = GetTupleType(elementTypes.Length);

            return tuple.MakeGenericType(elementTypes);
        }

        public static bool IsGenericEnumerable(Type type)
        {
            return GetEnumerableInterface(type) != null;
        }

        public static bool IsCastableTo(Type from, Type to)
        {
            if (to.IsAssignableFrom(@from))
            {
                return true;
            }

            var methods = @from
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m =>
                    m.ReturnType == to &&
                    (m.Name == "op_Implicit" || m.Name == "op_Explicit"));

            return methods.Count() > 0;
        }

        public static Type GetEnumerableInterface(Type type)
        {
            if (type.IsInterface)
            {
                if (type.IsInterface && 
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return type;
                }
            }

            Type[] interfaces = type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                Type subInterface = interfaces[i];

                if (subInterface.IsGenericType &&
                    subInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return subInterface;
                }
            }

            return null;
        }

        public static Type GetElementType(Type type)
        {
            Type enumerableInterface = GetEnumerableInterface(type);

            if (enumerableInterface == null)
            {
                return null;
            }

            return enumerableInterface.GetGenericArguments()[0];
        }
    }
}
