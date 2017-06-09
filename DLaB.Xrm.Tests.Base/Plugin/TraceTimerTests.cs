using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DLaB.Xrm.Plugin;
using DLaB.Xrm.Test;

namespace DLaB.Xrm.Tests.Plugin
{
    [TestClass]
    public class TraceTimerTests
    {

        [TestMethod]
        public void TraceTimer_UsingStatment_Should_LogTime()
        {
            var message = "TEST";
            var trace = new FakeTraceService(new EmptyTrace());
            using (new TraceTimer(trace, message))
            {
                Thread.Sleep(10);
            }
            Assert.AreEqual(2, trace.Traces.Count);
            Assert.IsTrue(trace.Traces[0].Trace.Contains(message));
            var end = trace.Traces[1].Trace;
            Assert.IsTrue(end.Contains(message));
            Assert.IsTrue(end.Contains(" 0.01"), $"Expected '{end}' to contain ' 0.01'");
        }

        private class EmptyTrace : ITestLogger
        {
            public void WriteLine(string message) { }
            public void WriteLine(string format, params object[] args) { }
        }
    }
}
