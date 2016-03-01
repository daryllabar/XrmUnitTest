using System;
using DLaB.Common;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox
{
    /// <summary>
    /// Exception Handler For Exceptions when executing in Sandbox Isolation Mode
    /// </summary>
    public class ExceptionHandler
    {
        /// <summary>
        /// Determines whether the given exception can be thrown in sandbox mode.
        /// Throws a Safe if it can't
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        /// <exception cref="Exception"></exception>
        public static bool CanThrow(Exception ex)
        {
            var currentException = ex;
            var canThrow = true;

            // While the Exception Types are still valid to be thrown, loop through all inner exceptions, checking for validity
            while (canThrow && currentException != null)
            {
                if (IsValidToBeThrown(currentException))
                {
                    currentException = currentException.InnerException;
                }
                else
                {
                    canThrow = false;
                }
            }

            if (canThrow)
            {
                return true;
            }

            var exceptionMessage = ex.ToStringWithCallStack();

            // ReSharper disable once InvertIf - I like it better this way
            if (IsValidToBeThrown(ex))
            {
                // Attempt to throw the exact Exception Type, with the 
                var ctor = ex.GetType().GetConstructor(new[] { typeof(string) });
                if (ctor != null)
                {
                    throw (Exception) ctor.Invoke(new object[] { exceptionMessage });
                }
            }

            throw new Exception(exceptionMessage);
        }

        /// <summary>
        /// Determines whether the specified ex is valid to be thrown.
        /// Current best guess is that it is not 
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private static bool IsValidToBeThrown(Exception ex)
        {
            var assembly = ex.GetType().Assembly.FullName.ToLower();
            return assembly.StartsWith("mscorlib,") || assembly.StartsWith("microsoft.xrm.sdk,");
        }
    }
}
