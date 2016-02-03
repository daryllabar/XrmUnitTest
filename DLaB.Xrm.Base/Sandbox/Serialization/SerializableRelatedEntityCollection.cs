using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    /// <summary>
    /// Sandbox safe Serialization for Related Entity Collection
    /// </summary>
    [CollectionDataContract(Name = "RelatedEntityCollection", Namespace = "http://schemas.datacontract.org/2004/07/System.Collections.Generic")]
    public class SerializableRelatedEntityCollection: List<KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableRelatedEntityCollection"/> class.
        /// </summary>
        public SerializableRelatedEntityCollection()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableRelatedEntityCollection"/> class.
        /// </summary>
        /// <param name="col">The col.</param>
        public SerializableRelatedEntityCollection(RelatedEntityCollection col)
        {
            foreach (var related in col)
            {
                Add(new KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN(related.Key, related.Value));
            }
        }
    }
}
