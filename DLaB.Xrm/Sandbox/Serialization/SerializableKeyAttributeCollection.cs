using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [CollectionDataContract(Name = "KeyAttributeCollection", Namespace = "http://schemas.microsoft.com/xrm/7.1/Contracts")]
    [Serializable]
    public class SerializableKeyAttributeCollection : List<KeyValuePairOfstringanyType>
    {
        public SerializableKeyAttributeCollection() {}

        public SerializableKeyAttributeCollection(KeyAttributeCollection keys)
        {
            foreach (var key in keys)
            {
                Add(new KeyValuePairOfstringanyType(key));
            }
        }
    }
}
