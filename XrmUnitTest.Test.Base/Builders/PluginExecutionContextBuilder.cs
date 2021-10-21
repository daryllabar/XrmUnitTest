#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace XrmUnitTest.Test.Builders
{
    public class PluginExecutionContextBuilder : PluginExecutionContextBuilderBase<PluginExecutionContextBuilder>
    {
        protected override PluginExecutionContextBuilder This => this;
    }
}
