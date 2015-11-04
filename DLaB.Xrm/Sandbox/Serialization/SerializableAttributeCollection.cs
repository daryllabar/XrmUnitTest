using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [CollectionDataContract(Name = "AttributeCollection", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableAttributeCollection : List<KeyValuePairOfstringanyType>
    {

        public SerializableAttributeCollection() { }

        public SerializableAttributeCollection(AttributeCollection attributes)
        {
            foreach (var att in attributes)
            {
                Add(new KeyValuePairOfstringanyType(att));
            }
        }
    }
}
