using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal static class GenericMethodCaller
    {

        [DebuggerHidden]
        internal static object InvokeLocalCrmDatabaseStaticGenericMethod(LocalCrmDatabaseInfo info, string logicalName, string methodName, params object[] parameters)
        {
            return InvokeLocalCrmDatabaseStaticGenericMethod(info, logicalName, methodName, BindingFlags.Public, parameters);
        }

        [DebuggerHidden]
        internal static object InvokeLocalCrmDatabaseStaticGenericMethod(LocalCrmDatabaseInfo info, string logicalName, string methodName, BindingFlags bindingFlags, params object[] parameters)
        {
            return InvokeLocalCrmDatabaseStaticMultiGenericMethod(info, methodName, bindingFlags, new object[] {logicalName}, parameters);
        }

        [DebuggerHidden]
        internal static object InvokeLocalCrmDatabaseStaticMultiGenericMethod(LocalCrmDatabaseInfo info, string methodName, BindingFlags bindingFlags, object[] typesOrLogicalNames, params object[] parameters)
        {
            var types = new Type[typesOrLogicalNames.Length];
            for (var i=0; i < typesOrLogicalNames.Length; i++)
            {
                types[i] = typesOrLogicalNames[i] is string
                    ? LocalCrmDatabase.GetType(info, (string) typesOrLogicalNames[i])
                    : (Type) typesOrLogicalNames[i];
            }

            try
            {
                return typeof(LocalCrmDatabase).GetMethods(bindingFlags | BindingFlags.Static)
                                               .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethod)
                                               ?.MakeGenericMethod(types)
                                               .Invoke(null, parameters)!;
            }
            catch (TargetInvocationException ex)
            {
                ThrowInnerException(ex);
                throw new Exception("Throw InnerException didn't throw exception");
            }
        }

        [DebuggerHidden]
        internal static Entity InvokeToEntity(Entity entity, LocalCrmDatabaseInfo info)
        {
            return InvokeToEntity(entity, LocalCrmDatabase.GetType(info, entity.LogicalName));
        }

        [DebuggerHidden]
        internal static Entity InvokeToEntity(Entity entity, Type toEntityType)
        {
            try
            {
                return (Entity)typeof(Entity).GetMethod("ToEntity")!.MakeGenericMethod(toEntityType).Invoke(entity, null)!;
            }
            catch (TargetInvocationException ex)
            {
                ThrowInnerException(ex);
                throw new Exception("Throw InnerException didn't throw exception");
            }
        }

        /// <summary>
        /// Attempts to throw the inner exception of the TargetInvocationException
        /// </summary>
        /// <param name="ex"></param>
        [DebuggerHidden]
        private static void ThrowInnerException(TargetInvocationException ex)
        {
            if (ex.InnerException == null) { throw new NullReferenceException("TargetInvocationException did not contain an InnerException", ex); }

            Exception? exception = null;
            try
            {
                //Assume typed Exception has "new (String message, Exception innerException)" signature
                exception = (Exception)(Activator.CreateInstance(ex.InnerException.GetType(), ex.InnerException.Message, ex.InnerException) ?? throw new Exception("Unable to create Exception of type " + ex.InnerException.GetType().FullName));
            }
            catch
            {
                //Constructor doesn't have the right constructor, eat the error and throw the inner exception below
            }

            if (exception?.InnerException == null || ex.InnerException.Message != exception.Message)
            {
                // Wasn't able to correctly create the new Exception.  Fall back to just throwing the inner exception
                throw ex.InnerException;
            }
            throw exception;
        }
    }
}
