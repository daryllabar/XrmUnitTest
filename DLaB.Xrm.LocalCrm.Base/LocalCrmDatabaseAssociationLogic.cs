using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.LocalCrm
{
    partial class LocalCrmDatabase
    {
        private static readonly ConcurrentDictionary<string, AssociationInfo> _n2NAssociations = new ConcurrentDictionary<string, AssociationInfo>();

        private static Guid AssociateN2N<TOne, TTwo>(LocalCrmDatabaseOrganizationService service,
            EntityReference one, EntityReference two, Relationship relationship, DelayedException exception)
            where TOne : Entity
            where TTwo : Entity
        {
            var associationEntity = Activator.CreateInstance<N2NAssociation<TOne, TTwo>>();
            N2NAssociation<TOne, TTwo>.EntityLogicalName = relationship.SchemaName;
            associationEntity.LogicalName = N2NAssociation<TOne, TTwo>.EntityLogicalName;
            associationEntity.One = one;
            associationEntity.Two = two;

            _n2NAssociations.TryAdd(relationship.SchemaName, AssociationInfo.Create(typeof(N2NAssociation<TOne, TTwo>), associationEntity));
            return Create(service, associationEntity, exception);
        }

        private static void DisassociateN2N<TOne, TTwo>(LocalCrmDatabaseOrganizationService service,
            EntityReference one, EntityReference two, Relationship relationship, DelayedException exception)
            where TOne : Entity
            where TTwo : Entity
        {
            var table = SchemaGetOrCreate<N2NAssociation<TOne, TTwo>>(service.Info);
            var entity = table.First(e =>
                (e.One.Equals(two) && e.Two.Equals(one)
                    || e.One.Equals(one) && e.Two.Equals(two)));

            table.Delete(entity);
        }

        internal static Type GetType(LocalCrmDatabaseInfo info, string logicalName)
        {
            return _n2NAssociations.TryGetValue(logicalName, out var value)
                ? value.AssociationType
                : EntityHelper.GetType(info.EarlyBoundEntityAssembly, info.EarlyBoundNamespace, logicalName);
        }

        private static bool ContainsLinkedEntityWithAssociation(LinkEntity link)
        {
            return _n2NAssociations.ContainsKey(link.LinkToEntityName)
                || _n2NAssociations.ContainsKey(link.LinkFromEntityName)
                || link.LinkEntities.Any(ContainsLinkedEntityWithAssociation);
        }

        private static IEnumerable<LinkEntity> GetLinkedEntitiesWithMappedAssociations(DataCollection<LinkEntity> links)
        {
            var list = new List<LinkEntity>();
            foreach (var link in links)
            {
                list.Add(GetLinkedEntitiesWithMappedAssociations(link));
            }

            return list;
        }

        private static LinkEntity GetLinkedEntitiesWithMappedAssociations(LinkEntity link)
        {
            if (!ContainsLinkedEntityWithAssociation(link))
            {
                return link;
            }
            var shallowClone = new LinkEntity
            {
                Columns = link.Columns,
                LinkToAttributeName = link.LinkToAttributeName,
                LinkCriteria = link.LinkCriteria,
                EntityAlias = link.EntityAlias,
                ExtensionData = link.ExtensionData,
                JoinOperator = link.JoinOperator,
                LinkFromAttributeName = link.LinkFromAttributeName,
                LinkFromEntityName = link.LinkFromEntityName,
                LinkToEntityName = link.LinkToEntityName,
            };

#if !PRE_KEYATTRIBUTE && !XRM_2015
            shallowClone.Orders.AddRange(link.Orders);
#endif
            if (_n2NAssociations.TryGetValue(shallowClone.LinkFromEntityName, out var fromAssociation))
            {
                shallowClone.LinkFromAttributeName = shallowClone.LinkToEntityName == fromAssociation.OneLogicalName
                    ? N2NAssociations.Fields.One
                    : N2NAssociations.Fields.Two;
            }

            if(_n2NAssociations.TryGetValue(shallowClone.LinkToEntityName, out var toAssociation))
            {
                shallowClone.LinkToAttributeName = shallowClone.LinkFromEntityName == toAssociation.OneLogicalName
                    ? N2NAssociations.Fields.One
                    : N2NAssociations.Fields.Two;
            }

            foreach (var child in link.LinkEntities)
            {
                shallowClone.LinkEntities.Add(GetLinkedEntitiesWithMappedAssociations(child));
            }

            link = shallowClone;
                
            return link;
        }
    }
}
