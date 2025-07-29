#if NET
using DataverseUnitTest;
using DataverseUnitTest.MSTest;
#else
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.MSTest;
#endif
using DLaB.Xrm.Entities;
using XrmUnitTest.Test.Builders;


namespace XrmUnitTest.Test
{

    public abstract class LocalTestBase : LocalTestBase<CrmEnvironmentBuilder, CrmContext, BusinessUnit, SystemUser>
    {
        protected override void InitializeTestSettings()
        {
            TestInitializer.InitializeTestSettings();
        }
    }
}
