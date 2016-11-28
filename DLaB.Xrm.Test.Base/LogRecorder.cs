using System.Collections.Generic;

namespace DLaB.Xrm.Test
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
