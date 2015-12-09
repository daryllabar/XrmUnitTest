using DLaB.Xrm.Test;
using Microsoft.Xrm.Sdk;

namespace Example.MsTestBase.Builders
{
    public class ServiceProviderBuilder : DLaB.Xrm.Test.Builders.ServiceProviderBuilderBase<ServiceProviderBuilder>
    {
        public ServiceProviderBuilder()
        {
            
        }

        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITestLogger logger) : base(service, context, logger)
        {
            
        }

        public ServiceProviderBuilder(IOrganizationService service, IPluginExecutionContext context, ITracingService trace) : base(service, context, trace)
        {

        }

        protected override ServiceProviderBuilder This => this;
    }
}
