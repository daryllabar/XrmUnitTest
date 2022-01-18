using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

#if NET
using DataverseUnitTest.Assumptions;
#else
using DLaB.Xrm.Test.Assumptions;
#endif

namespace XrmUnitTest.Test.Assumptions
{
    [PrerequisiteAssumptions(typeof(ConnectionRole1), typeof(ConnectionRole2))]
    public class ConnectionRoleAssociated : AssociationOnlyAssumptionBaseAttribute, IAssumptionEntityType<ConnectionRoleAssociated, ConnectionRoleAssociation>
    {
        public override EntityReference Entity => Assumptions.GetId<ConnectionRole1>();
        public override string Relationship => "connectionroleassociation_association";
        public override IEnumerable<EntityReference> Entities => new EntityReference[] {Assumptions.GetId<ConnectionRole2>()};

        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            var entity = service.GetFirstOrDefault<ConnectionRoleAssociation>(cr => new
                {
                    cr.Id,
                    cr.ConnectionRoleId,
                    cr.AssociatedConnectionRoleId,
                },
                ConnectionRoleAssociation.Fields.ConnectionRoleId, Entity.Id,
                ConnectionRoleAssociation.Fields.AssociatedConnectionRoleId, Entities.First().Id);
            return entity;
        }


    }
}