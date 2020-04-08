using System;
using System.Reflection;
using DLaB.Xrm.Client;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Assumes the Early bound entity has a primary name property to determine the name to retrieve
    /// </summary>
    public class PrimaryNameViaFieldProvider: PrimaryNameFieldProviderBase
    {
        /// <summary>
        /// Assembly of early bound entities
        /// </summary>
        public Assembly EarlyBoundEntityAssembly { get; set; }
        /// <summary>
        /// Namespace of early bound entities
        /// </summary>
        public string EarlyBoundNamespace { get; set; }
        /// <summary>
        /// Field name containing the primary attribute.  Defaults to the System.
        /// </summary>
        public string PrimaryNameFieldName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="earlyBoundEntityAssembly">Assembly of early bound entities</param>
        /// <param name="earlyBoundNamespace">Namespace of early bound entities</param>
        public PrimaryNameViaFieldProvider(Assembly earlyBoundEntityAssembly, string earlyBoundNamespace)
        {
            EarlyBoundEntityAssembly = earlyBoundEntityAssembly;
            EarlyBoundNamespace = earlyBoundNamespace;
            PrimaryNameFieldName = AppConfig.CrmEntities.PrimaryNameAttributeName;
        }

        /// <summary>
        /// Returns the Primary Name for the given entity logical name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        public override string GetPrimaryName(string logicalName)
        {
            if (NamelessEntities.Contains(logicalName))
            {
                return string.Empty;
            }
            var type = EntityHelper.GetType(EarlyBoundEntityAssembly, EarlyBoundNamespace, logicalName);
            var field = type.GetField(PrimaryNameFieldName);
            if (field == null)
            {
                throw new Exception($"Type \"{type.FullName}\" does not contain a field with the name \"{PrimaryNameFieldName}\"!  Consider using the Early Bound Generator to generate this value or using the PrimaryNameViaNonStandardNamesProvider and providing a list of non-standard names via the config.");
            }

            return (string)field.GetValue(null);
        }

        /// <summary>
        /// Returns the Primary Name for the given entity logical name
        /// </summary>
        /// <returns></returns>
        public override string GetPrimaryName<T>()
        {
            var field = typeof(T).GetField(PrimaryNameFieldName);
            if (field == null)
            {
                if (NamelessEntities.Contains(EntityHelper.GetEntityLogicalName<T>()))
                {
                    return string.Empty;
                }
                throw new Exception($"Type \"{typeof(T).FullName}\" does not contain a field with the name \"{PrimaryNameFieldName}\"!  Consider using the Early Bound Generator to generate this value or using the PrimaryNameViaNonStandardNamesProvider and providing a list of non-standard names via the config.");
            }

            return (string)field.GetValue(null);
        }
    }
}
