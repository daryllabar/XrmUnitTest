using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Tracing Service that allows for Faking and implment the ITracingService
    /// </summary>
    public class FakeTraceService : ITracingService, ICloneable
    {
        /// <summary>
        /// Gets or sets the traces.
        /// </summary>
        /// <value>
        /// The traces.
        /// </value>
        public List<TraceParams> Traces { get; set; }
        /// <summary>
        /// Gets or sets the trace action.
        /// </summary>
        /// <value>
        /// The trace action.
        /// </value>
        public Action<string, object[]> TraceAction { get; set; }

        private ITestLogger Logger { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTraceService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FakeTraceService(ITestLogger logger)
        {
            Traces = new List<TraceParams>();
            TraceAction = (s, o) => { };
            Logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTraceService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="traceAction">The trace action.</param>
        public FakeTraceService(ITestLogger logger, Action<string, object []> traceAction ) : this(logger)
        {
            TraceAction = traceAction;
        }

        /// <summary>
        /// Traces the specified format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void Trace(string format, params object[] args)
        {
            if (Logger != null)
            {
                if (args.Length == 0)
                {
                    Logger.WriteLine(format);
                }
                else
                {
                    Logger.WriteLine(format, args);
                }
            }
            Traces.Add(new TraceParams(format, args));
            TraceAction(format, args);
        }

        /// <summary>
        /// Class for being able to access the Trace Params before being traced
        /// </summary>
        public class TraceParams
        {
            /// <summary>
            /// Gets or sets the format.
            /// </summary>
            /// <value>
            /// The format.
            /// </value>
            public string Format { get; set; }
            /// <summary>
            /// Gets or sets the arguments.
            /// </summary>
            /// <value>
            /// The arguments.
            /// </value>
            public object[] Args { get; set; }
            /// <summary>
            /// Gets or sets the trace.
            /// </summary>
            /// <value>
            /// The trace.
            /// </value>
            public string Trace { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TraceParams"/> class.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="args">The arguments.</param>
            public TraceParams(string format, object[] args)
            {
                Format = format;
                Args = args;
                // If there are no args, then this has probably already been formatted, don't reformat
                Trace = args.Length == 0 ? format : String.Format(format, args);

            }
        }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public FakeTraceService Clone()
        {
            var clone = (FakeTraceService)MemberwiseClone();
            CloneReferenceValues(clone);
            return clone;
        }

        /// <summary>
        /// Clones the reference values.
        /// </summary>
        /// <param name="clone">The clone.</param>
        protected void CloneReferenceValues(FakeTraceService clone)
        {
            clone.Logger = Logger;
            clone.Traces = new List<TraceParams>();
            clone.Traces.AddRange(Traces);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion Clone
    }
}
