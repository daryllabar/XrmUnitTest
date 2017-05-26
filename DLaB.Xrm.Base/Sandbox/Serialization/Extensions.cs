using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Extensions for the DLaB.Xrm.Sandbox.Serialization namespace
    /// </summary>
    public static class Extensions
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
            return ((Entity) entity).AsEntity<T>();
        }

        #endregion string
    }
}
