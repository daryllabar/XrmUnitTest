using DLaB.Xrm.Test;
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
