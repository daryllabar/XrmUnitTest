using System;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Defines an Interface for a test framework provider.  This is needed to be able to interact with different Test Frameworks
    /// </summary>
    public interface ITestFrameworkProvider
    {
        /// <summary>
        /// Gets the type of the attribute used to define a test method.
        /// </summary>
        /// <value>
        /// the type of the attribute used to define a test method.
        /// </value>
        Type TestMethodAttributeType { get; }

        /// <summary>
        /// Exception to throw when a custom Assertion has failed.  MsTest: AssertFailedException
        /// </summary>
        /// <returns></returns>
        Exception GetFailedException(string message);

        /// <summary>
        /// Exception to throw when a custom Assertion is inconclusive.  MsTest: AssertInconclusiveException.  
        /// </summary>
        /// <returns></returns>
        Exception GetInconclusiveException(string message);
    }
}
