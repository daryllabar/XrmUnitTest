using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Extensions for the DLaB.Xrm.Sandbox.Serialization namespace
    /// </summary>
#if DLAB_PUBLIC
    public static class Extensions
#else
    internal static class Extensions
#endif
    {
        #region string

        /// <summary>
        /// Deserializes the entity from a string xml value to a specific entity type.
        /// </summary>
        /// <param name="xml">The xml to deserialize.</param>
        /// <returns></returns>
        public static SerializableEntity DeserializeSerializedEntity(this string xml)
        {
            return xml.DeserializeDataObject<SerializableEntity>();
        }

        /// <summary>
        /// Deserializes the entity from a string xml value to an IExtensibleDataObject
        /// </summary>
        /// <param name="xml">The xml to deserialize.</param>
        /// <returns></returns>
        public static T DeserializeSerializedEntity<T>(this string xml) where T : Entity
        {
            var entity = DeserializeSerializedEntity(xml);
            return ((Entity) entity).ToEntity<T>();
        }

        #endregion string
    }
}
