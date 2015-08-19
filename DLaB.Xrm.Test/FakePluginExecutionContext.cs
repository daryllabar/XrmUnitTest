using System;
using System.Diagnostics;
using System.Linq;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    [DebuggerDisplay("{DebugInfo}")]
    public class FakePluginExecutionContext : FakeExecutionContext, IPluginExecutionContext
    {
        public int Stage { get; set; }
        public IPluginExecutionContext ParentContext { get; set; }

        private string DebugInfo
        {
            get { return String.Format("Message: {0}, Entity: {1}, Stage: {2}, Depth: {3}, ParentContexts {4}", MessageName, PrimaryEntityName, Stage, Depth, this.GetParentContexts().Count()); }
        }

        public FakePluginExecutionContext()
        {
            // Default to most Common, which I'm guessing is Pre Operation Update
            Stage = (int) PipelineStage.PreOperation;
            MessageName = MessageType.Update;
        }
    }
}
