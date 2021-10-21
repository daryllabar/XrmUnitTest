#if NET
using DataverseUnitTest;
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Builders;
#endif
using Microsoft.Xrm.Sdk;

namespace XrmUnitTest.Test.Builders
{
    public class ServiceProviderBuilder : ServiceProviderBuilderBase<ServiceProviderBuilder>
    {
        public ServiceProviderBuilder() {}
        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITestLogger logger) : base(service, context, logger) {}
        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITracingService trace) : base(service, context, trace) { }

        protected override ServiceProviderBuilder This => this;
    }
}
