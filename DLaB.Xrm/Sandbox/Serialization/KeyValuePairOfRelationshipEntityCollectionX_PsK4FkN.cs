using Microsoft.Xrm.Sdk;
// ReSharper disable InconsistentNaming

namespace DLaB.Xrm.Sandbox.Serialization
{
    public class KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN
    {
        public Relationship key { get; set; }
        public SerializableEntityCollection value { get; set; }

        public KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN() { }

        public KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN(Relationship key, SerializableEntityCollection value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValuePairOfRelationshipEntityCollectionX_PsK4FkN(Relationship key, EntityCollection value): this(key, new SerializableEntityCollection(value))
        {
        }
    }
}
