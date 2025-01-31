using System;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        /// <summary>
        /// Creates the mirrored if the Entity is a Connection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        private static void CreateMirroredConnection<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (EntityHelper.GetEntityLogicalName<T>() != Connection.EntityLogicalName)
            {
                return;
            }

            if (service.MirroredEntityRequestTriggered)
            {
                service.MirroredEntityRequestTriggered = false;
                return;
            }

            service.MirroredEntityRequestTriggered = true;

            // Copy over matching values and create
            service.Create(GetMirroredConnection(entity));
        }

        private static Entity GetMirroredConnection<T>(T entity) where T : Entity
        {
            var clone = entity.Clone(true);
            clone.Id = Guid.Empty;
            Swap(clone, Connection.Fields.Record1Id, entity, Connection.Fields.Record2Id);
            Swap(clone, Connection.Fields.Record1ObjectTypeCode, entity, Connection.Fields.Record2ObjectTypeCode);
            Swap(clone, Connection.Fields.Record1RoleId, entity, Connection.Fields.Record2RoleId);
            Swap(clone, Connection.Fields.Record2Id, entity, Connection.Fields.Record1Id);
            Swap(clone, Connection.Fields.Record2ObjectTypeCode, entity, Connection.Fields.Record1ObjectTypeCode);
            Swap(clone, Connection.Fields.Record2RoleId, entity, Connection.Fields.Record1RoleId);
            clone[Connection.Fields.IsMaster] = false;
            return clone;
        }

        private static void Swap(Entity entityTo, string attributeTo, Entity entityFrom, string attributeFrom)
        {
            if (entityFrom.Attributes.TryGetValue(attributeFrom, out var value))
            {
                entityTo[attributeTo] = value;
            }
        }

        private static void AutoPopulateConnectionFields<T>(T entity, bool isCreate) where T : Entity
        {
            if (!isCreate
                || EntityHelper.GetEntityLogicalName<T>() != Connection.EntityLogicalName
                || entity.GetAttributeValue<bool?>(Connection.Fields.IsMaster) == false)
            {
                return;
            }

            entity[Connection.Fields.IsMaster] = true;
        }
    }
}