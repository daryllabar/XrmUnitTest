using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm
{
    public class LateBoundQuerySettings : QuerySettings<Entity>
    {
        public LateBoundQuerySettings(string logicalName):base(logicalName) { }
    }
}
