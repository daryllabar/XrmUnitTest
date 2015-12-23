using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm
{
    /// <summary>
    /// Query Settings that are not bound to a specific Entity Type
    /// </summary>
    public class LateBoundQuerySettings : QuerySettings<Entity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LateBoundQuerySettings"/> class.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        public LateBoundQuerySettings(string logicalName):base(logicalName) { }
    }
}
