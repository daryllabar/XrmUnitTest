using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Sandbox safe Serialization of Key Attribute Collection
    /// </summary>
    [CollectionDataContract(Name = "KeyAttributeCollection", Namespace = "http://schemas.microsoft.com/xrm/7.1/Contracts")]
    public class SerializableKeyAttributeCollection : List<KeyValuePairOfstringanyType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableKeyAttributeCollection"/> class.
        /// </summary>
        public SerializableKeyAttributeCollection() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableKeyAttributeCollection"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public SerializableKeyAttributeCollection(KeyAttributeCollection keys)
        {
            foreach (var key in keys)
            {
                Add(new KeyValuePairOfstringanyType(key));
            }
        }
    }
}
