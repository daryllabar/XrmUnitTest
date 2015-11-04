using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [CollectionDataContract(Name = "RelatedEntityCollection", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableRelatedEntityCollection: List<KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN>
    {
        public SerializableRelatedEntityCollection()
        {
            
        }

        public SerializableRelatedEntityCollection(RelatedEntityCollection col)
        {
            foreach (var related in col)
            {
                Add(new KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN(related.Key, related.Value));
            }
        }
    }
}
