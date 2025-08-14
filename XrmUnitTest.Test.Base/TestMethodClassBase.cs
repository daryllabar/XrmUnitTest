
#if NET
using DataverseUnitTest;
using DataverseUnitTest.MSTest;
#else
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.MSTest;
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
            Test(new TestLogger());
        }
    }
}
