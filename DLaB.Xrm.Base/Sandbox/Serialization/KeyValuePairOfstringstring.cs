using System.Collections.Generic;
using System.Runtime.Serialization;

// ReSharper disable InconsistentNaming
namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Sandbox Serialization Safe KeyValuePairOfstringstring
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/System.Collections.Generic")]
    public struct KeyValuePairOfstringstring
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string key { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairOfstringstring"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public KeyValuePairOfstringstring(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairOfstringstring"/> class.
        /// </summary>
        /// <param name="kvp">The KVP.</param>
        public KeyValuePairOfstringstring(KeyValuePair<string, string> kvp) : this(kvp.Key, kvp.Value)
        {

        }
    }
}
