using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace DLaB.Xrm.Sandbox.Serialization
{
    public class KeyValuePairOfstringanyType
    {
        public string key { get; set; }
        public object value { get; set; }

        public KeyValuePairOfstringanyType() {}

        public KeyValuePairOfstringanyType(string key, object value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValuePairOfstringanyType(KeyValuePair<string, object> kvp) : this(kvp.Key, kvp.Value)
        {
            
        }
    }
}
