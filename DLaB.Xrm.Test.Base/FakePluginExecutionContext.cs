using System;
using System.Diagnostics;
using System.Linq;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("{DebugInfo}")]
    public class FakePluginExecutionContext : FakeExecutionContext, IPluginExecutionContext, IServiceFaked<IPluginExecutionContext>, IFakeService, ICloneable
    {
        /// <summary>
        /// Gets or sets the stage.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public int Stage { get; set; }
        /// <summary>
        /// Gets or sets the parent context.
        /// </summary>
        /// <value>
        /// The parent context.
        /// </value>
        public IPluginExecutionContext ParentContext { get; set; }

        private string DebugInfo => $"Message: {MessageName}, Entity: {PrimaryEntityName}, Stage: {Stage}, Depth: {Depth}, ParentContexts {this.GetParentContexts().Count()}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FakePluginExecutionContext"/> class.
        /// </summary>
        public FakePluginExecutionContext()
        {
            // Default to most Common, which I'm guessing is Pre Operation Update
            Stage = (int) PipelineStage.PreOperation;
            MessageName = "Update";
            IsolationMode = (int)DLaB.Xrm.Plugin.IsolationMode.Sandbox;
        }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public new FakePluginExecutionContext Clone()
        {
            var clone = (FakePluginExecutionContext) MemberwiseClone();
            CloneReferenceValues(clone);
            if (ParentContext is ICloneable parent)
            {
                ParentContext = (IPluginExecutionContext)parent.Clone();
            }
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }        

        #endregion Clone
    }
}
