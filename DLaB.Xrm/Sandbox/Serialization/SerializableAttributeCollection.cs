using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Sandbox safe Serializable Attribute Collection
    /// </summary>
    [CollectionDataContract(Name = "AttributeCollection", Namespace = "http://schemas.datacontract.org/2004/07/System.Collections.Generic")]
    public class SerializableAttributeCollection : List<KeyValuePairOfstringanyType>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableAttributeCollection"/> class.
        /// </summary>
        public SerializableAttributeCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableAttributeCollection"/> class.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public SerializableAttributeCollection(AttributeCollection attributes)
        {
            foreach (var att in attributes)
            {
                Add(new KeyValuePairOfstringanyType(att));
            }
        }
    }
}
