using Microsoft.Xrm.Sdk.Organization;
using System;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Defines the optional settings for the local crm database
    /// </summary>
    public class LocalCrmDatabaseOptionalSettings
    {
        /// <summary>
        /// Used to populate Owning Business Unit Attributes
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <summary>
        /// Defines the instance of the database. Allows for sharing of the database from different call sites, if given the same name.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the Data Center Id.
        /// </summary>
        public Guid? DataCenterId { get; set; }

        /// <summary>
        /// Gets or sets the collection of endpoints.
        /// </summary>
        public EndpointCollection? Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the Environment Id.
        /// </summary>
        public Guid? EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        public string? FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the geo.
        /// </summary>
        public string? Geo { get; set; }

        /// <summary>
        /// Defines the full name format. Defaults to F I L.
        /// Format of FullName:
        ///   F = First Name
        ///   M = Middle Name
        ///   I = Middle Initial
        ///   L = Last Name
        /// </summary>
        /// <value>
        /// The full name format (always upper case).
        /// </value>
        public string? FullNameFormat { get; set; }

        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        public int? LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the ManyToManyAssociationProvider.
        /// </summary>
        public IMany2ManyAssociationProvider? ManyToManyAssociationProvider { get; set; }

        /// <summary>
        /// Gets or sets the PrimaryNameProvider.
        /// </summary>
        public IPrimaryNameProvider? PrimaryNameProvider { get; set; }

        /// <summary>
        /// Gets or sets the Many2ManyAssociationProvider.
        /// </summary>
        public IMany2ManyAssociationProvider? Many2ManyAssociationProvider { get; set; }

        /// <summary>
        /// Gets or sets the organization identifier.
        /// </summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the organization type.
        /// </summary>
        public OrganizationType? OrganizationType { get; set; }

        /// <summary>
        /// Gets or sets the organization version.
        /// </summary>
        public string? OrganizationVersion { get; set; }

        /// <summary>
        /// Gets or sets the schema type.
        /// </summary>
        public string? SchemaType { get; set; }

        /// <summary>
        /// Gets or sets the organization state.
        /// </summary>
        public OrganizationState? State { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// The Time Provider
        /// </summary>
        public ITimeProvider? TimeProvider { get; set; }

        /// <summary>
        /// Gets or sets the URL name.
        /// </summary>
        public string? UrlName { get; set; }

        /// <summary>
        /// Used to populate Created/Modified By and Owner Attributes.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Used to populate Created/Modified On Behalf Of Attributes.
        /// </summary>
        public Guid? UserOnBehalfOfId { get; set; }
    }
}
