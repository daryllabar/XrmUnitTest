using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Test
{
    public interface ITestFrameworkProvider
    {
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
