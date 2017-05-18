using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Plugin
{
    /// <summary>
    /// Isolation Mode of the Plugin Assembly
    /// </summary>
#if DLAB_PUBLIC
    public enum IsolationMode
#else
    internal enum IsolationMode
#endif
    {
        /// <summary>
        /// No Isolation Mode
        /// </summary>
        None = 1,
        /// <summary>
        /// Sandboxed Isoloation Mode
        /// </summary>
        Sandbox = 2
    }
}
