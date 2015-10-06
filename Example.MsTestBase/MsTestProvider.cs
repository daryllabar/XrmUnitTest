using System;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Example.MsTestBase
{
    class MsTestProvider : ITestFrameworkProvider
    {
        public Type TestMethodAttributeType { get { return typeof (TestMethodAttribute); } }
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
