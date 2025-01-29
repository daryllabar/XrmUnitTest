using System;
using System.Collections.Generic;

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
        /// If only one Test Attribute Type will be used, gets the type of the attribute used to define a test method.  If more than one is to be used, this must be null and the IMultiTestMethodAttributeTestFrameworkProvider should be used with the TestMethodAttributeTypes populated.
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

    /// <summary>
    /// Defines an Interface for a test framework provider that has multiple Test Method Attribute types.  This is needed to be able to interact with different Test Frameworks
    /// </summary>
    public interface IMultiTestMethodAttributeTestFrameworkProvider : ITestFrameworkProvider
    {
        /// <summary>
        /// Allows for defining more than one attribute type.  If only one is needed, the TestMethodAttributeType should be used.
        /// </summary>
        /// <value>
        /// The types of the attributes used to define a test method.
        /// </value>
        HashSet<Type> TestMethodAttributeTypes { get; }
    }
}
