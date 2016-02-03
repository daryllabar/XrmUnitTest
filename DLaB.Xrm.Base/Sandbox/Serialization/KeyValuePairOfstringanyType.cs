using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

// ReSharper disable InconsistentNaming

namespace DLaB.Xrm.Sandbox.Serialization
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
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairOfstringanyType"/> class.
        /// </summary>
        /// <param name="kvp">The KVP.</param>
        public KeyValuePairOfstringanyType(KeyValuePair<string, object> kvp) : this(kvp.Key, kvp.Value)
        {
            
        }
    }
}
