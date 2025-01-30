using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;
using ParticipationMask = DLaB.Xrm.LocalCrm.OptionSets.OptionSet.ActivityParty_ParticipationTypeMask;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        /// <summary>
        /// Creates the activity pointer if the Entity is an Activity Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        private static void CreateActivityPointer<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (EntityHelper.GetEntityLogicalName<T>() == ActivityPointer.EntityLogicalName)
            {
                return; // Type is already an activity pointer, no need to recreated
            }

            if (!PropertiesCache.For<T>().IsActivityType)
            {
                return; // Type is not an activity type
            }

            // Copy over matching values and create
            service.Create(GetActivityPointerForActivityEntity(entity));
        }

        private static Entity GetActivityPointerForActivityEntity<T>(T entity) where T : Entity
        {
            var pointerFields = typeof(ActivityPointer.Fields).GetFields();
            var pointer = new Entity(ActivityPointer.EntityLogicalName)
            {
                Id = entity.Id
            };
            foreach (var att in pointerFields.Where(p => PropertiesCache.For<T>().ContainsProperty(p.Name))
                                             .Select(field => field.GetRawConstantValue().ToString()).Where(entity.Contains))
            {
                pointer[att] = entity[att];
            }

            return pointer;
        }

        private static void CreateActivityParties<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (entity.LogicalName == ActivityParty.EntityLogicalName
                || entity.LogicalName == ActivityPointer.EntityLogicalName
                || !PropertiesCache.For<T>().IsActivityType)
            {
                return;
            }

            CreateActivityPartiesFromPartyLists(service, entity);
            CreateActivityPartiesFromSingleFields(service, entity);
        }

        private static readonly HashSet<string> ActivityPartyFields = new HashSet<string>
        {
            Email.Fields.From,
            Email.Fields.RegardingObjectId,
            Appointment.Fields.Organizer,
            Appointment.Fields.OwningUser,
            Appointment.Fields.OwnerId
        };

        private static void CreateActivityPartiesFromSingleFields<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            foreach (var att in entity.Attributes.Where(a => ActivityPartyFields.Contains(a.Key)))
            {
                service.CreateActivityParty(new Entity
                {
                    LogicalName = ActivityParty.EntityLogicalName,
                    [ActivityParty.Fields.PartyId] = att.Value,
                    [ActivityParty.Fields.ActivityId] = entity.ToEntityReference(),
                    [ActivityParty.Fields.ParticipationTypeMask] = MapFieldToParticipation(att.Key)
                });
            }
        }

        private static void CreateActivityPartiesFromPartyLists<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            foreach (var att in entity.Attributes.Where(a => a.Value is EntityCollection))
            {
                var entities = (EntityCollection)att.Value;
                foreach (var party in entities.Entities.Where(p => p.LogicalName == ActivityParty.EntityLogicalName))
                {
                    if (party.GetAttributeValue<EntityReference>(ActivityParty.Fields.PartyId) == null)
                    {
                        if(party.GetAttributeValue<string>(ActivityParty.Fields.AddressUsed) == null)
                        {
                             throw new Exception("Activity Party PartyId and AddressUsed were null");
                        }
                    }
                    else
                    {
                        party[ActivityParty.Fields.ActivityId] = entity.ToEntityReference();
                    }

                    if (party.GetAttributeValue<object>(ActivityParty.Fields.ParticipationTypeMask) == null)
                    {
                        party[ActivityParty.Fields.ParticipationTypeMask] = MapFieldToParticipation(att.Key);
                    }

                    service.CreateActivityParty(party);
                }
            }
        }

        private static OptionSetValue MapFieldToParticipation(string field)
        {
            ParticipationMask value;
            switch (field)
            {
                case Email.Fields.To:
                    value = ParticipationMask.ToRecipient;
                    break;
                case Email.Fields.From:
                    value = ParticipationMask.Sender;
                    break;
                case Email.Fields.Bcc:
                    value = ParticipationMask.BCCRecipient;
                    break;
                case Email.Fields.Cc:
                    value = ParticipationMask.CCRecipient;
                    break;
                case Appointment.Fields.RequiredAttendees:
                    value = ParticipationMask.Requiredattendee;
                    break;
                case Appointment.Fields.RegardingObjectId:
                    value = ParticipationMask.Regarding;
                    break;
                case Appointment.Fields.OptionalAttendees:
                    value = ParticipationMask.Optionalattendee;
                    break;
                case Appointment.Fields.Organizer:
                    value = ParticipationMask.Organizer;
                    break;
                case Appointment.Fields.OwnerId:
                case Appointment.Fields.OwningUser:
                    value = ParticipationMask.Owner;
                    break;
                default:
                    throw new NotImplementedException($"Participation Type Mask for creation of ActivityParty field '{field}' not defined");
            }

            return new OptionSetValue((int)value);
        }

        private static void UpdateActivityPointer<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            if (entity.LogicalName == ActivityPointer.EntityLogicalName || !PropertiesCache.For<T>().IsActivityType)
            {
                return; // Type is already an activity pointer, no need to re-update
            }

            service.Update(GetActivityPointerForActivityEntity(entity));
        }

        private static void DeleteActivityPointer<T>(LocalCrmDatabaseOrganizationService service, Guid id) where T : Entity
        {
            if (EntityHelper.GetEntityLogicalName<T>() == ActivityPointer.EntityLogicalName || !PropertiesCache.For<T>().IsActivityType)
            {
                return; // Type is already an activity pointer, no need to redelete, or type is not an activity type
            }

            service.Delete(ActivityPointer.EntityLogicalName, id);
        }
    }
}
