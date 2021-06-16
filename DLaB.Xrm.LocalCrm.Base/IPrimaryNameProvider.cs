using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Defines an Interface for a Primary Name provider.  This is needed to be able to default primary names of entity references.
    /// </summary>
    public interface IPrimaryNameProvider
    {
        /// <summary>
        /// Returns the Primary Name for the given entity logical name
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        string GetPrimaryName(string logicalName);

        /// <summary>
        /// Returns the Primary Name for the given entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string GetPrimaryName<T>() where T: Entity;
    }
}
