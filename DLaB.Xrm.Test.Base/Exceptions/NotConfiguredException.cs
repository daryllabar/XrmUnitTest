using System;

#if NET
namespace DataverseUnitTest.Exceptions
#else
namespace DLaB.Xrm.Test.Exceptions
#endif
{
    /// <summary>
    /// Exception Type for when a unit test action is performed, that is depedent on a configuration that is not configured
    /// </summary>
    public class NotConfiguredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConfiguredException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotConfiguredException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotConfiguredException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
