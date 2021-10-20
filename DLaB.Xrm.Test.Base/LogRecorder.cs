using System.Collections.Generic;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    internal class LogRecorder : ITestLogger
    {
        private FakeTraceService Trace { get;}

        public IEnumerable<TraceParams> Logs => Trace.Traces;

        public LogRecorder(ITestLogger logger)
        {
            Trace = new FakeTraceService(logger);
        }

        public void WriteLine(string message) { Trace.Trace(message); }
        public void WriteLine(string format, params object[] args) { Trace.Trace(format, args); }
    }
}
