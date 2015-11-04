using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [CollectionDataContract(Name = "FormattedValueCollection", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableFormattedValueCollection: List<KeyValuePairOfstringstring>
    {
        public SerializableFormattedValueCollection() { }

        public SerializableFormattedValueCollection(FormattedValueCollection values)
        {
            foreach (var value in values)
            {
                Add(new KeyValuePairOfstringstring(value));
            }
        }
    }
}
