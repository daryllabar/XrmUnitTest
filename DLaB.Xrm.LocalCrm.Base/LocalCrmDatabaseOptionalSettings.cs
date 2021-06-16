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
        /// Defines the instance of the database.  Allows for sharing of the database from different call sites, if given the same name.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Defines the full name format.  Defaults to F I L <para/>
        /// Format of FullName <para/>
        ///   F = First Name <para/>
        ///   M = Middle Name <para/>
        ///   I = Middle Initial <para/>
        ///   L = Last Name 
        /// </summary>
        /// <value>
        /// The full name format (always upper case).
        /// </value>
        public string FullNameFormat { get; set; }
        /// <summary>
        /// Used for defining OptionMetadata
        /// </summary>
        public int? LanguageCode { get; set; }
        /// <summary>
        /// The ManyToManyAssociationProvider
        /// </summary>
        public IMany2ManyAssociationProvider ManyToManyAssociationProvider { get; set; }
        /// <summary>
        /// The PrimaryNameProvider
        /// </summary>
        public IPrimaryNameProvider PrimaryNameProvider { get; set; }
        /// <summary>
        /// The Many2ManyAssociationProvider
        /// </summary>
        public IMany2ManyAssociationProvider Many2ManyAssociationProvider { get; set; }
        /// <summary>
        /// The organization identifier.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public Guid? OrganizationId { get; set; }
        /// <summary>
        /// Used to populate Created/Modified By and Owner Attributes
        /// </summary>
        public Guid? UserId { get; set; }
        /// <summary>
        /// Used to populate Created/Modified On Behalf Of Attributes
        /// </summary>
        public Guid? UserOnBehalfOfId { get; set; }
    }
}
