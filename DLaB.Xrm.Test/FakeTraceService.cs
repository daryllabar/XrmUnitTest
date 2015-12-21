using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Diagnostics;

namespace DLaB.Xrm.Test
{
    public class FakeTraceService : ITracingService
    {
        public List<TraceParams> Traces { get; set; }
        public Action<string, object[]> TraceAction { get; set; }


        public FakeTraceService()
        {
            Traces = new List<TraceParams>();
            TraceAction = (s, o) => { };
        }

        public FakeTraceService(Action<string, object []> traceAction ) : this()
        {
            TraceAction = traceAction;
        }

        public void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
            Traces.Add(new TraceParams(format,args));
            TraceAction(format, args);
        }

        public class TraceParams
        {
            public string Format { get; set; }
            public object[] Args { get; set; }
            public string Trace { get; set; }

            public TraceParams(string format, object[] args)
            {
                Format = format;
                Args = args;
                Trace = string.Format(format, args);
            }
        }
    }
}
