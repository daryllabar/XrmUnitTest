using System;
using System.Linq;
using System.Reflection;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

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
        /// Used for defining OptionMetadata
        /// </summary>
        public int LanguageCode = AppConfig.DefaultLanguageCode;
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
        /// The organization identifier.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public Guid OrganizationId { get; private set; }
        /// <summary>
        /// Used to populate Created/Modifed By and Owner Attributes
        /// </summary>
        public EntityReference User { get; private set; }
        /// <summary>
        /// Used to populate Created/Modifed On Behalf Of Attributes
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
        public static LocalCrmDatabaseInfo Create<T>(string databaseName = null, Guid? userId = null, Guid? userOnBehalfOf = null, Guid? userBusinessUnit = null) where T : OrganizationServiceContext
        {
            var contextType = typeof(T);
            if (contextType.Name == "OrganizationServiceContext")
            {
                throw new Exception("Must pass in a derived type from Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }

            databaseName = databaseName ?? String.Empty;

            return Create(contextType.Assembly, contextType.Namespace, databaseName, userId, userOnBehalfOf, userBusinessUnit);
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
        public static LocalCrmDatabaseInfo Create(Assembly earlyBoundAssembly, string earlyBoundNamespace, string databaseName = null, Guid? userId = null, Guid? userOnBehalfOf = null, Guid? userBusinessUnit = null)
        {
            databaseName = databaseName ?? String.Empty;

            return new LocalCrmDatabaseInfo
            {
                BusinessUnit = new EntityReference("businessunit", userBusinessUnit.GetValueOrDefault()),
                DatabaseName = databaseName,
                EarlyBoundEntityAssembly = earlyBoundAssembly,
                EarlyBoundNamespace = earlyBoundNamespace,
                User = new EntityReference("systemuser", userId.GetValueOrDefault(Guid.NewGuid())),
                UserOnBehalfOf = new EntityReference("systemuser", userOnBehalfOf.GetValueOrDefault()),
                OrganizationId = ConvertToGuid(databaseName),
            };
        }

        /// <summary>
        /// Determines whether given entity name is defiend in the early bound assembly
        /// </summary>
        /// <param name="entityLogicalName">Name of the entity logical.</param>
        /// <returns></returns>
        public bool IsTypeDefined(string entityLogicalName)
        {
            return EntityHelper.IsTypeDefined(EarlyBoundEntityAssembly, EarlyBoundNamespace, entityLogicalName);
        }

        /// <summary>
        /// A hacky method to convert a string into a Guid.  Splits the string up into 4 parts, gets the hashcode for each,
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
