using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

// ReSharper disable InconsistentNaming

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Sandbox.Serialization
#else
namespace Source.DLaB.Xrm.Sandbox.Serialization
#endif
	
{
    /// <summary>
    /// Sandbox Serialization Safe KeyValuePairOfstringanyType
    /// </summary>
    [DataContract (Namespace = "http://schemas.datacontract.org/2004/07/System.Collections.Generic")]
    public struct KeyValuePairOfstringanyType
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [DataMember]
        public string key { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public object value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairOfstringanyType"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public KeyValuePairOfstringanyType(string key, object value)
        {
            this.key = key;
            var reference = value as EntityReference;
            if (reference != null)
            {
                value = new SerializableEntityReference(reference); 
            }
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairOfstringanyType"/> class.
        /// </summary>
        /// <param name="kvp">The KVP.</param>
        public KeyValuePairOfstringanyType(KeyValuePair<string, object> kvp) : this(kvp.Key, kvp.Value)
        {
            
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="KeyValuePairOfstringanyType"/> to KeyValuePair.
        /// </summary>
        /// <param name="pair">The pair.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator KeyValuePair<string, object>(KeyValuePairOfstringanyType pair)
        {
            var reference = pair.value as SerializableEntityReference;
            if (reference != null)
            {
                pair.value = (EntityReference)reference;
            }

            return new KeyValuePair<string, object>(pair.key, pair.value);
        }
    }
}
