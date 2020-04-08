using System.Collections.Generic;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Utilizes a Non Standard Names Dictionary to determine the primary name of entities
    /// </summary>
    public class PrimaryNameViaNonStandardNamesProvider: PrimaryNameFieldProviderBase
    {
        /// <summary>
        /// Non standard primary name attribute names by entity logical name.
        /// </summary>
        public Dictionary<string,string> NonStandardNames { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nonStandardNames">Non standard primary name attribute names by entity logical name.</param>
        public PrimaryNameViaNonStandardNamesProvider(Dictionary<string, string> nonStandardNames = null)
        {
            NonStandardNames = nonStandardNames ?? new Dictionary<string, string>();
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
            return NonStandardNames.TryGetValue(logicalName, out var name) ? name : EntityHelper.GetPrimaryFieldInfo(logicalName, new DefaultConfig()).AttributeName;
        }

        /// <summary>
        /// Returns the Primary Name for the given entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override string GetPrimaryName<T>()
        {
            return GetPrimaryName(EntityHelper.GetEntityLogicalName<T>());
        }

        private class DefaultConfig : IEntityHelperConfig
        {
            public string GetIrregularIdAttributeName(string logicalName)
            {
                return null;
            }

            public PrimaryFieldInfo GetIrregularPrimaryFieldInfo(string logicalName, PrimaryFieldInfo defaultInfo = null)
            {
                return null;
            }
        }
    }
}
