using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Sandbox.Serialization
{
    [DataContract(Name = "EntityCollection", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class SerializableEntityCollection
    {
        [DataMember]
        public List<SerializableEntity> Entities { get; set; }

        [DataMember]
        public string EntityName { get; set; }

        [DataMember]
        public string MinActiveRowVersion { get; set; }

        [DataMember]
        public bool MoreRecords { get; set; }

        [DataMember]
        public string PagingCookie { get; set; }

        [DataMember]
        public int TotalRecordCount { get; set; }

        [DataMember]
        public bool TotalRecordCountLimitExceeded { get; set; }

        public SerializableEntityCollection()
        {
            Entities = new List<SerializableEntity>();
        }

        public SerializableEntityCollection(EntityCollection entities) : this()
        {
            foreach (var entity in entities.Entities)
            {
                Entities.Add(new SerializableEntity(entity));
            }
            EntityName = entities.EntityName;
            MinActiveRowVersion = entities.MinActiveRowVersion;
            MoreRecords = entities.MoreRecords;
            PagingCookie = entities.PagingCookie;
            TotalRecordCount = entities.TotalRecordCount;
            TotalRecordCountLimitExceeded = entities.TotalRecordCountLimitExceeded;

        }
    }
}
