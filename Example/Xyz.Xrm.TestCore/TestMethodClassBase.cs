using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
using Microsoft.Xrm.Sdk;

namespace Xyz.Xrm.Test
{
    public abstract class TestMethodClassBase : TestMethodClassBaseDLaB
    {
        protected override IAgnosticServiceBuilder GetOrganizationServiceBuilder(IOrganizationService service) { return new Builders.OrganizationServiceBuilder(service); }

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
