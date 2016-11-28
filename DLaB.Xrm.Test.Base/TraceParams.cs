using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Class for being able to access the Trace Params before being traced
    /// </summary>
    [DebuggerDisplay("{Trace}")]
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
            Trace = args.Length == 0 ? format : string.Format(format, args);
        }
    }
}
