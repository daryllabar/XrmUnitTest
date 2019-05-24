using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DLaB.Xrm.Test
{
    internal class StubOrganizationService : IOrganizationService
    {
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
        public Guid Create(Entity entity) { return Guid.NewGuid(); }
        public void Delete(string entityName, Guid id) { }
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
        public OrganizationResponse Execute(OrganizationRequest request) { return new OrganizationResponse(); }
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) { return new Entity("Not Defined"); }
        public EntityCollection RetrieveMultiple(QueryBase query) { return new EntityCollection(); }
        public void Update(Entity entity) { }
    }
}
