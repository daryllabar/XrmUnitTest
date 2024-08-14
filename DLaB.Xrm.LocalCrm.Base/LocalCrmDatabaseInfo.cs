using System;
using System.Linq;
using System.Reflection;
using DLaB.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
#if !XRM_2013
using Microsoft.Xrm.Sdk.Organization;
#endif
using AppConfig = DLaB.Xrm.Client.AppConfig;
#if NET
using System.Web;
#else
using System.Net;
using static System.Net.WebRequestMethods;
#endif

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Info Object for setting a Local Crm Database 
    /// </summary>
    public class LocalCrmDatabaseInfo
    {
        /// <summary>
        /// Used to populate Owning Business Unit Attributes
        /// </summary>
        public EntityReference BusinessUnit { get; private set; }
        /// <summary>
        /// Defines the instance of the database.  Allows for sharing of the database from different call sites, if given the same name.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName { get; private set; }
        /// <summary>
        /// The early bound entity assembly.
        /// </summary>
        /// <value>
        /// The early bound entity assembly.
        /// </value>
        public Assembly EarlyBoundEntityAssembly { get; private set; }
        /// <summary>
        /// The early bound namespace.
        /// </summary>
        /// <value>
        /// The early bound namespace.
        /// </value>
        public string EarlyBoundNamespace { get; private set; }

        /// <summary>
        /// Gets or sets the Data Center Id.
        /// </summary>
        public Guid DataCenterId { get; set; }

#if !XRM_2013
        /// <summary>
        /// Gets or sets the collection of endpoints.
        /// </summary>
        public EndpointCollection Endpoints { get; set; }
#endif

        /// <summary>
        /// Gets or sets the Environment Id.
        /// </summary>
        public Guid? EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the geo.
        /// </summary>
        public string Geo { get; set; }

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
        public string FullNameFormat { get; set; }
        /// <summary>
        /// Used for defining OptionMetadata
        /// </summary>
        public int LanguageCode { get; private set; }
        /// <summary>
        /// The ManyToManyAssociationProvider
        /// </summary>
        public IMany2ManyAssociationProvider ManyToManyAssociationProvider { get; set; }
        /// <summary>
        /// The PrimaryNameProvider
        /// </summary>
        public IPrimaryNameProvider PrimaryNameProvider { get; set; }
        /// <summary>
        /// The organization identifier.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public Guid OrganizationId { get; private set; }

#if !PRE_MULTISELECT
        /// <summary>
        /// Gets or sets the organization type.
        /// </summary>
        public OrganizationType OrganizationType { get; set; }
#endif

        /// <summary>
        /// Gets or sets the organization version.
        /// </summary>
        public string OrganizationVersion { get; set; }

        /// <summary>
        /// Gets or sets the schema type.
        /// </summary>
        public string SchemaType { get; set; }

#if !XRM_2013
        /// <summary>
        /// Gets or sets the organization state.
        /// </summary>
        public OrganizationState State { get; set; }
#endif

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the URL name.
        /// </summary>
        public string UrlName { get; set; }

        /// <summary>
        /// Used to populate Created/Modified By and Owner Attributes.
        /// </summary>
        public EntityReference User { get; private set; }
        /// <summary>
        /// Used to populate Created/Modified On Behalf Of Attributes.
        /// </summary>
        public EntityReference UserOnBehalfOf { get; private set; }

        private LocalCrmDatabaseInfo() { }

        /// <summary>
        /// Creates the specified database info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userOnBehalfOf">The user on behalf of.</param>
        /// <param name="userBusinessUnit">The user business unit.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext</exception>
        public static LocalCrmDatabaseInfo Create<T>(string databaseName = null, 
            Guid? userId = null, 
            Guid? userOnBehalfOf = null, 
            Guid? userBusinessUnit = null) where T : OrganizationServiceContext
        {
            return Create<T>(new LocalCrmDatabaseOptionalSettings
            {
                BusinessUnitId = userBusinessUnit,
                DatabaseName = databaseName,
                UserId = userId,
                UserOnBehalfOfId = userOnBehalfOf
            });
        }

        /// <summary>
        /// Creates the specified database info.
        /// </summary>
        /// <param name="earlyBoundAssembly">The early bound assembly.</param>
        /// <param name="earlyBoundNamespace">The early bound namespace.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userOnBehalfOf">The user on behalf of.</param>
        /// <param name="userBusinessUnit">The user business unit.</param>
        /// <returns></returns>
        public static LocalCrmDatabaseInfo Create(Assembly earlyBoundAssembly, 
            string earlyBoundNamespace, 
            string databaseName = null, 
            Guid? userId = null, 
            Guid? userOnBehalfOf = null, 
            Guid? userBusinessUnit = null)
        {
            return Create(earlyBoundAssembly, earlyBoundNamespace, new LocalCrmDatabaseOptionalSettings
            {
                BusinessUnitId = userBusinessUnit,
                DatabaseName = databaseName,
                UserId = userId,
                UserOnBehalfOfId = userOnBehalfOf
            });
        }

        /// <summary>
        /// Creates the specified database info.
        /// </summary>
        /// <param name="optionalSettings">The settings to be used.</param>
        /// <returns></returns>
        public static LocalCrmDatabaseInfo Create<T>(LocalCrmDatabaseOptionalSettings optionalSettings)
        {
            var contextType = typeof(T);
            if (contextType.Name == "OrganizationServiceContext")
            {
                throw new Exception("Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }

            return Create(contextType.Assembly, contextType.Namespace, optionalSettings);
        }

        private const int UrlPreAndPostFixLength = 26; // "https://.crm.dynamics.com/".Length
        /// <summary>
        /// Creates the specified database info.
        /// </summary>
        /// <param name="earlyBoundAssembly">The early bound assembly.</param>
        /// <param name="earlyBoundNamespace">The early bound namespace.</param>
        /// <param name="optionalSettings">The settings to be used.</param>
        /// <returns></returns>
        public static LocalCrmDatabaseInfo Create(Assembly earlyBoundAssembly,
            string earlyBoundNamespace,
            LocalCrmDatabaseOptionalSettings optionalSettings)
        {
            optionalSettings = optionalSettings ?? new LocalCrmDatabaseOptionalSettings();
#if NET
            var dbName = optionalSettings.DatabaseName ?? "DataverseUnitTest";
            var urlName = HttpUtility.UrlEncode(dbName).Limit(200);
#else
            var dbName = optionalSettings.DatabaseName ?? "XrmUnitTest";
            var urlName = WebUtility.UrlEncode(dbName).Limit(264 - UrlPreAndPostFixLength);
#endif
            return new LocalCrmDatabaseInfo
            {
                BusinessUnit = GetRef(optionalSettings.BusinessUnitId, Entities.BusinessUnit.EntityLogicalName, AppConfig.CrmSystemSettings.BusinessUnitId),
                DatabaseName = dbName,
                DataCenterId = optionalSettings.DataCenterId ?? Guid.NewGuid(),
                EarlyBoundEntityAssembly = earlyBoundAssembly,
                EarlyBoundNamespace = earlyBoundNamespace,
#if !XRM_2013
                Endpoints = optionalSettings.Endpoints ?? new EndpointCollection
                {
                    { EndpointType.WebApplication, $"https://{urlName}.crm.dynamics.com/" },
                    { EndpointType.OrganizationService,$"https://{urlName}.api.crm.dynamics.com/XRMServices/2011/Organization.svc" },
                    { EndpointType.OrganizationDataService, $"https://{urlName}.api.crm.dynamics.com/XRMServices/2011/OrganizationData.svc" }
                },
#endif
                EnvironmentId = optionalSettings.EnvironmentId ?? Guid.NewGuid(),
                FriendlyName = optionalSettings.FriendlyName ?? dbName,
                FullNameFormat = optionalSettings.FullNameFormat ?? AppConfig.CrmSystemSettings.FullNameFormat,
                Geo = optionalSettings.Geo ?? "NA",
                LanguageCode = optionalSettings.LanguageCode ?? AppConfig.DefaultLanguageCode,
                ManyToManyAssociationProvider = optionalSettings.ManyToManyAssociationProvider ?? new Many2ManyAssociationProvider(AppConfig.CrmEntities.Many2ManyAssociationDefinitions),
                PrimaryNameProvider = optionalSettings.PrimaryNameProvider ?? PrimaryNameFieldProviderBase.GetConfiguredProvider(earlyBoundAssembly, earlyBoundNamespace),
                OrganizationId = optionalSettings.OrganizationId ?? ConvertToGuid(dbName),
#if !PRE_MULTISELECT
                OrganizationType = optionalSettings.OrganizationType ?? OrganizationType.Customer,
#endif
                OrganizationVersion = optionalSettings.OrganizationVersion ?? "9.2.24064.210",
                SchemaType = optionalSettings.SchemaType ?? "Full",
#if !XRM_2013
                State = optionalSettings.State ?? OrganizationState.Enabled,
#endif
                TenantId = optionalSettings.TenantId ?? Guid.NewGuid(),
                UrlName = optionalSettings.UrlName ?? urlName,
                User = GetRef(optionalSettings.UserId, Entities.SystemUser.EntityLogicalName, AppConfig.CrmSystemSettings.UserId),
                UserOnBehalfOf = GetRef(optionalSettings.UserOnBehalfOfId, Entities.SystemUser.EntityLogicalName, AppConfig.CrmSystemSettings.OnBehalfOfId),
            };
        }

        private static EntityReference GetRef(Guid? id, string logicalName, Guid defaultId)
        {
            return new EntityReference(logicalName, id.GetValueOrDefault(defaultId));
        }

        /// <summary>
        /// Determines whether given entity name is defined in the early bound assembly
        /// </summary>
        /// <param name="entityLogicalName">Name of the entity logical.</param>
        /// <returns></returns>
        public bool IsTypeDefined(string entityLogicalName)
        {
            return EntityHelper.IsTypeDefined(EarlyBoundEntityAssembly, EarlyBoundNamespace, entityLogicalName);
        }

        /// <summary>
        /// A hack-y method to convert a string into a Guid.  Splits the string up into 4 parts, gets the hashcode for each,
        /// then converts that directly into a Guid.  This at least ensures that the same OrganizationId is returned
        /// for the same database Name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Guid ConvertToGuid(string value)
        {
            value = value ?? string.Empty; // Handle Null
            value = value.PadLeft(4);// keeps the string from being too short
            var length = (int)Math.Floor(value.Length * .25);
            return new Guid(Enumerable.Range(0, 4)
               .SelectMany(i => BitConverter.GetBytes(value.Substring(i * length, i == 3 ? value.Length - length * i : length).GetHashCode())).ToArray());
        }
    }
}
