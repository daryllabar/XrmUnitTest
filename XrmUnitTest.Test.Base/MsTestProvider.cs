using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace XrmUnitTest.Test
{
    class MsTestProvider : ITestFrameworkProvider
    {
        public Type TestMethodAttributeType => typeof (TestMethodAttribute);

        public Exception GetFailedException(string message)
        {
            return new AssertFailedException(message);
        }

        public Exception GetInconclusiveException(string message)
        {
            return new AssertInconclusiveException(message);
        }
    }
}
