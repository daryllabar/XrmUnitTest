#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace XrmUnitTest.Test
{
    public abstract class TestMethodClassBase : TestMethodClassBaseDLaB
    {

        protected override void LoadConfigurationSettings()
        {
            TestInitializer.InitializeTestSettings();
        }

        public void Test()
        {
            Test(new DebugLogger());
        }
    }
}
