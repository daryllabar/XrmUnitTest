using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace DLaB.Xrm.Sandbox.Serialization
{
    public class KeyValuePairOfstringstring
    {
        public string key { get; set; }
        public object value { get; set; }

        public KeyValuePairOfstringstring() { }

        public KeyValuePairOfstringstring(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValuePairOfstringstring(KeyValuePair<string, string> kvp) : this(kvp.Key, kvp.Value)
        {

        }
    }
}
