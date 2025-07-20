#nullable enable
#if NET
namespace DataverseUnitTest.Builders
#else
namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Represents the configuration for building an organization service builder.
    /// </summary>
    public interface IOrganizationServiceBuilderBuildConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use the primary builder for new entity default Ids.
        /// </summary>
        bool UsePrimaryBuilderForNewEntityDefaultIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the primary builder for entity filter.
        /// </summary>
        bool UsePrimaryBuilderForEntityFilter { get; set; }
    }

    /// <summary>
    /// Represents the default implementation of <see cref="IOrganizationServiceBuilderBuildConfig"/>.
    /// </summary>
    public class OrganizationServiceBuilderBuildConfig : IOrganizationServiceBuilderBuildConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use the primary builder for new entity default Ids.
        /// </summary>
        public bool UsePrimaryBuilderForNewEntityDefaultIds { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use the primary builder for entity filter.
        /// </summary>
        public bool UsePrimaryBuilderForEntityFilter { get; set; } = true;
    }
}
