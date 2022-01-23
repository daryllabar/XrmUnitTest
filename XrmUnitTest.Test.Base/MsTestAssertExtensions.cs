using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter
{
    public static class MsTestAssertExtensions
    {
        public static void AttributesAreEqual(this Assert assert, Entity expected, Entity actual)
        {
            var error = AssertHelper.AttributesAreEqual(expected, actual);
            if(error != null)
            {
                Fail(nameof(AttributesAreEqual), error);
            }
        }

        private static void Fail(string name, string message = null)
        {
            var msg = $"Assert.{name} failed.";
            if (!string.IsNullOrWhiteSpace(message))
            {
                msg += "  " + message;
            }

            throw new AssertFailedException(msg);
        }
    }
}
