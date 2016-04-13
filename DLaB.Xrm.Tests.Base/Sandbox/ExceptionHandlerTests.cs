using System;
using DLaB.Xrm.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.Tests.Sandbox
{
    [TestClass]
    public class ExceptionHandlerTests
    {
        [TestMethod]
        public void ExceptionHandler_CustomExceptionInSandbox_Should_ThrowGenericException()
        {
            try
            {
                throw new MissingAttributeException();
            }
            catch (Exception ex)
            {
                try
                {
                    Assert.IsFalse(Xrm.Sandbox.ExceptionHandler.AssertCanThrow(ex));
                }
                catch (Exception inner)
                {
                    AssertNoCustomExceptions(inner);
                }
            }
        }

        private void AssertNoCustomExceptions(Exception ex)
        {
            while (ex != null)
            {
                if (ex.GetType() != typeof (Exception))
                {
                    Assert.Fail("Exception Contained a Custom Exception Type");
                }

                ex = ex.InnerException;
            } 
        }
    }
}
