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

        private Guid AssociateN2N(EntityReference one, EntityReference two, Relationship relationship)
        {
            return (Guid)InvokeStaticMultiGenericMethod(
                one.LogicalName, 
                two.LogicalName, 
                nameof(AssociateN2N), 
                this, one, two, relationship);
        }

        private void DisassociateN2N(EntityReference one, EntityReference two, Relationship relationship)
        {
            InvokeStaticMultiGenericMethod(
                one.LogicalName,
                two.LogicalName,
                nameof(DisassociateN2N),
                this, one, two, relationship);
        }

        private static void DisassociateN2N<TOne, TTwo>(LocalCrmDatabaseOrganizationService service,
            EntityReference one, EntityReference two, Relationship relationship, DelayedException exception)
            where TOne : Entity
            where TTwo : Entity
        {
            if(string.IsNullOrEmpty(N2NAssociation<TOne, TTwo>.EntityLogicalName))
            {
                N2NAssociation<TOne, TTwo>.EntityLogicalName = relationship.SchemaName;
            }
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

                shallowClone.LinkCriteria = GetCriteriaWithMappedAssociations(shallowClone.LinkCriteria, fromAssociation);
            }

            if(_n2NAssociations.TryGetValue(shallowClone.LinkToEntityName, out var toAssociation))
            {
                shallowClone.LinkToAttributeName = shallowClone.LinkFromEntityName == toAssociation.OneLogicalName
                    ? N2NAssociations.Fields.One
                    : N2NAssociations.Fields.Two;

                shallowClone.LinkCriteria = GetCriteriaWithMappedAssociations(shallowClone.LinkCriteria, toAssociation);
            }

            foreach (var child in link.LinkEntities)
            {
                shallowClone.LinkEntities.Add(GetLinkedEntitiesWithMappedAssociations(child));
            }

            link = shallowClone;
                
            return link;
        }

        private static FilterExpression GetCriteriaWithMappedAssociations(FilterExpression fe, AssociationInfo info)
        {
            if (!fe.Conditions.Any(c => c.AttributeName == info.OneAttributeIdName || c.AttributeName == info.TwoAttributeIdName))
            {
                return fe;
            }

            var newFilter = new FilterExpression(fe.FilterOperator)
            {
                ExtensionData = fe.ExtensionData,
#if !XRM_2013 && !XRM_2015 && !XRM_2016
                FilterHint = fe.FilterHint,
#endif
                IsQuickFindFilter = fe.IsQuickFindFilter
            };
            newFilter.Filters.AddRange(fe.Filters);

            foreach (var initialCondition in fe.Conditions)
            {
                var condition = initialCondition;
                if (condition.AttributeName == info.OneAttributeIdName)
                {
                    condition = new ConditionExpression(N2NAssociations.Fields.One, condition.Operator, condition.Values);
                }
                else if (condition.AttributeName == info.TwoAttributeIdName)
                {
                    condition = new ConditionExpression(N2NAssociations.Fields.Two, condition.Operator, condition.Values);
                }
                newFilter.AddCondition(condition);
            }

            return newFilter;
        }
    }
}
