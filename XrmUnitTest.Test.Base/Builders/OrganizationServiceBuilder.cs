using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
#endif

namespace XrmUnitTest.Test.Builders
{
    public class OrganizationServiceBuilder : OrganizationServiceBuilderBase<OrganizationServiceBuilder>
    {
        protected override OrganizationServiceBuilder This => this;

        #region Constructors


        public OrganizationServiceBuilder() : this(TestBase.GetOrganizationService()) {}

        public OrganizationServiceBuilder(IOrganizationService service) : base(service) {}

        #endregion Constructors
    }
}