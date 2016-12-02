using System;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public interface IAgnosticServiceBuilder
    {
        IAgnosticServiceBuilder WithDefaultParentBu();
        IAgnosticServiceBuilder WithEntityNameDefaulted(Func<Entity, PrimaryFieldInfo, string> getName);
        IAgnosticServiceBuilder AssertIdNonEmptyOnCreate();
        IAgnosticServiceBuilder WithBusinessUnitDeleteAsDeactivate();
        IOrganizationService Build();
    }
}
