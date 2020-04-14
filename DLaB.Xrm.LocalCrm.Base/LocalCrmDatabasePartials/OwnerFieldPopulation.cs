using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        private static void PopulateOwnerFields<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            var owner = entity.GetAttributeValue<EntityReference>(Email.Fields.OwnerId);
            if (owner == null)
            {
                return;
            }

            var properties = PropertiesCache.For<T>();

            PopulateOwnerFieldsForUser(service, entity, owner, properties);
            PopulateOwnerFieldsForTeam(service, entity, owner, properties);
        }

        private static void PopulateOwnerFieldsForUser<T>(LocalCrmDatabaseOrganizationService service, T entity, EntityReference owner, EntityProperties properties) where T : Entity
        {
            if (owner.LogicalName != SystemUser.EntityLogicalName)
            {
                return;
            }

            PopulateOwnerField(entity, owner, properties, Email.Fields.OwningUser);
            PopulateOwningBuField(service, entity, owner, properties);
        }

        private static void PopulateOwnerFieldsForTeam<T>(LocalCrmDatabaseOrganizationService service, T entity, EntityReference owner, EntityProperties properties) where T : Entity
        {
            if (owner.LogicalName != Team.EntityLogicalName)
            {
                return;
            }

            PopulateOwnerField(entity, owner, properties, Email.Fields.OwningTeam);
            PopulateOwningBuField(service, entity, owner, properties);
        }

        private static void PopulateOwnerField<T>(T entity, EntityReference owner, EntityProperties properties, string ownerField) where T : Entity
        {
            if (properties.PropertiesByLogicalName.ContainsKey(ownerField))
            {
                entity[ownerField] = new EntityReference(owner.LogicalName, owner.Id);
            }
        }

        private static void PopulateOwningBuField<T>(LocalCrmDatabaseOrganizationService service, T entity, EntityReference owner, EntityProperties properties) where T : Entity
        {
            if (!properties.PropertiesByLogicalName.ContainsKey(Email.Fields.OwningBusinessUnit))
            {
                return;
            }

            var bu = GetDatabaseEntity(service.Info, owner.LogicalName, owner.Id).GetAttributeValue<EntityReference>(SystemUser.Fields.BusinessUnitId);
            if (bu != null)
            {
                entity[Email.Fields.OwningBusinessUnit] = new EntityReference(bu.LogicalName, bu.Id);
            }
        }
    }
}
