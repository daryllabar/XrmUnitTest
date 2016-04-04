using System.Diagnostics;
using DLaB.Xrm.Test;
namespace Example.MsTestBase
{
    public abstract class TestMethodClassBase : TestMethodClassBaseDLaB
    {

        protected override void LoadConfigurationSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

        [DebuggerHidden]
        public void Test()
        {
            Test(new DebugLogger());
        }
    }
}
