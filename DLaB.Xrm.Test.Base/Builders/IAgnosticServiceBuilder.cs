using System;
using Microsoft.Xrm.Sdk;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else
namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Interface for OrganizationServiceBuilder messages that get called by the TestMethodClassBase
    /// </summary>
    public interface IAgnosticServiceBuilder
    {
        /// <summary>
        /// Defaults the Parent BusinessUnit Id of all business units to the root BU if not already populated
        /// </summary>
        IAgnosticServiceBuilder WithDefaultParentBu();
        /// <summary>Defaults the entity name of all created entities.</summary>
        /// <param name="getName">function to call to get the name for the given Entity and it's Primary Field Info</param>
        IAgnosticServiceBuilder WithEntityNameDefaulted(Func<Entity, PrimaryFieldInfo, string> getName);
        /// <summary>
        /// Asserts that any create of an entity has the id populated.  Useful to ensure that all entities can be deleted after they have been created since the id is known.
        /// </summary>
        IAgnosticServiceBuilder AssertIdNonEmptyOnCreate();
        /// <summary>Builds this IOrganizationService.</summary>
        IOrganizationService Build();
    }
}
