using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    public class FakePluginExecutionContext : FakeExecutionContext, IPluginExecutionContext
    {
        public int Stage { get; set; }
        public IPluginExecutionContext ParentContext { get; set; }

        public FakePluginExecutionContext()
        {
            // Default to most Common, which I'm guessing is Pre Operation Update
            Stage = (int) PipelineStage.PreOperation;
            MessageName = MessageType.Update;
        }
    }
}
