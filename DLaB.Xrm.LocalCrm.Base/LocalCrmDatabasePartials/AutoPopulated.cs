using System;
using DLaB.Xrm.Exceptions;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        /// <summary>
        /// Populates the auto-populated columns of the entity and returns a list of attributes that were created / updated
        /// </summary>
        private static void PopulateAutoPopulatedAttributes<T>(LocalCrmDatabaseOrganizationService service, T entity, bool isCreate) where T : Entity
        {
            var properties = PropertiesCache.For<T>();
            var info = service.Info;
            var name = GetFullName(info,
                entity.GetAttributeValue<string>(SystemUser.Fields.FirstName),
                entity.GetAttributeValue<string>(SystemUser.Fields.MiddleName),
                entity.GetAttributeValue<string>(SystemUser.Fields.LastName));

            // TODO: Need to add logic to see if an update to the Full Name is being Performed
            ConditionallyAddValue(entity, properties, SystemUser.Fields.FullName, name, !string.IsNullOrWhiteSpace(name));

            AutoPopulateContactFields(entity, properties);
            AutoPopulateOpportunityFields(service, entity, isCreate);

            if (isCreate)
            {
                SetAttributeId(entity);
                SetStatusToActiveForCreate(entity);
                SetOwnerForCreate(service, entity, properties);
                ConditionallyAddValue(entity, properties, Email.Fields.CreatedBy, info.User, info.User.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddValue(entity, properties, Email.Fields.CreatedOnBehalfBy, info.UserOnBehalfOf, info.UserOnBehalfOf.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddValue(entity, properties, Email.Fields.CreatedOn, entity.Contains(Email.Fields.OverriddenCreatedOn) ? entity[Email.Fields.OverriddenCreatedOn] : DateTime.UtcNow);
                ConditionallyAddValue(entity, properties, Email.Fields.OwningBusinessUnit, info.BusinessUnit, !entity.Contains(Email.Fields.OwningBusinessUnit) && info.BusinessUnit.GetIdOrDefault() != Guid.Empty);
            }
            else if (entity.Contains(Email.Fields.OwnerId))
            {
                UpdateOwningFieldsBasedOnOwner(service, entity, properties);
            }

            PopulateModifiedAttributes(service.Info, entity, properties);
        }

        /// <summary>
        /// Adds the value to the entity only if the attribute is valid for the entity, and the entity doesn't contain the value, or the value is null
        /// </summary>
        private static void AddValueIfNotPresent<T>(Entity entity, EntityProperties properties, string attributeName, T value)
        {
            ConditionallyAddValue(entity, properties, attributeName, value, entity.GetAttributeValue<object>(attributeName) == null);
        }

        /// <summary>
        /// Adds the value to the entity only if the attribute is valid for the entity, and the condition is true
        /// </summary>
        private static void ConditionallyAddValue<T>(Entity entity, EntityProperties properties, string attributeName, T value, bool condition = true)
        {
            if (!condition || !properties.PropertiesByLogicalName.ContainsKey(attributeName))
            {
                return;
            }

            if (value is DateTime date)
            {
                entity[attributeName] = date.RemoveMilliseconds();
            }
            else
            {
                entity[attributeName] = value;
            }
        }

        /// <summary>
        /// Populate the Attribute Id Attribute.  This is required because creating a new Entity of type Entity, doesn't populate the Entity's Typed Id Attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        private static void SetAttributeId<T>(T entity) where T : Entity
        {
            if (entity.Id == Guid.Empty) { return; }
            var attribute = EntityPropertiesCache.Instance.For<T>().GetProperty("Id").GetAttributeLogicalName();
            entity[attribute] = entity.Id;
        }

        private static void SetStatusToActiveForCreate<T>(T entity) where T : Entity
        {
            var activeInfo = new ActivePropertyInfo<T>();
            if (activeInfo.ActiveAttribute == ActiveAttributeType.None) { return; }

            if (entity.Attributes.ContainsKey(activeInfo.AttributeName))
            {
                entity.Attributes.Remove(activeInfo.AttributeName);
            }

            switch (activeInfo.ActiveAttribute)
            {
                case ActiveAttributeType.IsDisabled:
                    entity[activeInfo.AttributeName] = false;
                    break;
                case ActiveAttributeType.StateCode:
                    entity[activeInfo.AttributeName] = new OptionSetValue(activeInfo.ActiveState.GetValueOrDefault());
                    break;
                case ActiveAttributeType.None:
                    break;
                default:
                    throw new EnumCaseUndefinedException<ActiveAttributeType>(activeInfo.ActiveAttribute);
            }
        }

        private static void PopulateModifiedAttributes<T>(LocalCrmDatabaseInfo info, T entity, EntityProperties properties) where T : Entity
        {
            ConditionallyAddValue(entity, properties, Email.Fields.ModifiedBy, info.User, info.User.GetIdOrDefault() != Guid.Empty);
            ConditionallyAddValue(entity, properties, Email.Fields.ModifiedOnBehalfBy, info.UserOnBehalfOf, info.UserOnBehalfOf.GetIdOrDefault() != Guid.Empty);
            ConditionallyAddValue(entity, properties, Email.Fields.ModifiedOn, DateTime.UtcNow);
        }

        #region Full Name Formatting

        private static string GetFullName(LocalCrmDatabaseInfo info, string firstName, string middleName, string lastName)
        {
            var fullNameFormat = RemoveEmptyPartsFromFormat(firstName, middleName, lastName, info.FullNameFormat);

            fullNameFormat = fullNameFormat.Replace("F", "{0}");
            fullNameFormat = fullNameFormat.Replace("L", "{1}");
            fullNameFormat = fullNameFormat.Replace("M", "{2}");
            fullNameFormat = fullNameFormat.Replace("I", "{3}");

            return string.Format(fullNameFormat,
                (firstName ?? "").Trim(),
                (lastName ?? "").Trim(),
                (middleName ?? "").Trim(),
                ((middleName ?? " ").Trim() + " ")[0]);
        }

        /// <summary>
        /// If a part of the name is empty, it should have the spaces/punctuation removed.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <param name="fullNameFormat"></param>
        /// <returns></returns>
        private static string RemoveEmptyPartsFromFormat(string firstName, string middleName, string lastName, string fullNameFormat)
        {
            fullNameFormat = UpdateFormat(fullNameFormat, firstName, 'F');
            fullNameFormat = UpdateFormat(fullNameFormat, lastName, 'L');
            fullNameFormat = UpdateFormat(fullNameFormat, middleName, 'M');
            fullNameFormat = UpdateFormat(fullNameFormat, middleName, 'I');
            while (fullNameFormat.EndsWith(" ")
                || fullNameFormat.EndsWith(","))
            {
                if (fullNameFormat.EndsWith(","))
                {
                    fullNameFormat = fullNameFormat.Remove(fullNameFormat.Length - 1);
                }

                fullNameFormat = fullNameFormat.TrimEnd();
            }

            return fullNameFormat;
        }

        /// <summary>
        /// Checks for white space names and removes whitespace from the format
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="name">The name.</param>
        /// <param name="nameFormatLetter">The name format letter.</param>
        /// <returns></returns>
        private static string UpdateFormat(string format, string name, char nameFormatLetter)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                format = format.Replace(nameFormatLetter + ", ", "");
                format = format.Replace(nameFormatLetter + " ", "");
                format = format.Replace(nameFormatLetter.ToString(), "");
            }

            return format;
        }

        #endregion Full Name Formatting

        #region Entity Specific Autopopulation

        /// <summary>
        /// AccountId/ParentContactId is auto-populated from the ParentCustomerId (
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        private static void AutoPopulateContactFields<T>(T entity, EntityProperties properties) where T : Entity
        {
            if (entity.LogicalName != "contact")
            {
                return;
            }

            var customer = entity.GetAttributeValue<EntityReference>("parentcustomerid");
            if (customer == null)
            {
                ConditionallyAddValue<EntityReference>(entity, properties, "parentcontactid", null, entity.GetAttributeValue<EntityReference>("parentcontactid") != null);
                ConditionallyAddValue<EntityReference>(entity, properties, "accountid", null, entity.GetAttributeValue<EntityReference>("accountid") != null);
                return;
            }

            var field = customer.LogicalName == "account" ? "accountid" : "parentcontactid";
            var siblingField = field == "parentcontactid" ? "accountid" : "parentcontactid"; // Sibling is opposite
            ConditionallyAddValue(entity, properties, field, customer, !customer.Equals(entity.GetAttributeValue<EntityReference>(field)));
            ConditionallyAddValue<EntityReference>(entity, properties, siblingField, null, entity.GetAttributeValue<EntityReference>(siblingField) != null);
        }

        /// <summary>
        /// Contact and Account Ids are populated by the customer id property.  Handle hydrating values.
        /// </summary>
        private static void AutoPopulateOpportunityFields<T>(LocalCrmDatabaseOrganizationService service, T entity, bool isCreate) where T : Entity
        {
            if (entity.LogicalName != Opportunity.EntityLogicalName)
            {
                return;
            }

            var customer = entity.GetAttributeValue<EntityReference>(Opportunity.Fields.CustomerId);
            var opp = isCreate
                ? new Entity()
                : service.Retrieve(Opportunity.EntityLogicalName, entity.Id, new ColumnSet(Opportunity.Fields.ParentAccountId, Opportunity.Fields.ParentContactId));

            var parentName = "parent" + customer?.LogicalName + "id";
            // Customer was set, set correct parent field to customer
            if (customer != null
                && !entity.Contains(parentName))
            {
                entity[parentName] = customer;
            }

            var hadAccount = opp.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentAccountId) != null;
            var hadContact = opp.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentContactId) != null;

            // Customer was cleared, set correct parent field to null;
            if (entity.ContainsNullValue("customerid"))
            {
                if (hadAccount)
                {
                    entity[Opportunity.Fields.ParentAccountId] = null;
                }
                else if (hadContact)
                {
                    entity[Opportunity.Fields.ParentContactId] = null;
                }
            }

            var hasAccount = entity.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentAccountId) != null;
            var hasContact = entity.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentContactId) != null;
            var willHaveAccount = hasAccount
                                  || hadAccount
                                  && !entity.ContainsNullValue(Opportunity.Fields.ParentAccountId);
            var willHaveContact = hasContact
                                  || hadContact
                                  && !entity.ContainsNullValue(Opportunity.Fields.ParentContactId);

            if (hasAccount)
            {
                entity[Opportunity.Fields.CustomerId] = entity[Opportunity.Fields.ParentAccountId];
            }
            else if (hasContact
                && !willHaveAccount)
            {
                entity[Opportunity.Fields.CustomerId] = entity[Opportunity.Fields.ParentContactId];
            }

            if (!willHaveAccount && willHaveContact)
            {
                entity[Opportunity.Fields.CustomerId] = entity.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentContactId)
                                       ?? opp.GetAttributeValue<EntityReference>(Opportunity.Fields.ParentContactId);
            }
            else if (!willHaveAccount)
            {
                entity[Opportunity.Fields.CustomerId] = null;
            }
        }

        #endregion Entity Specific Autopopulation

        #region Ownership

        /// <summary>
        /// Sets the owner to the Info.User if it is null.  Only to be called on Create.
        /// </summary>
        private static void SetOwnerForCreate(LocalCrmDatabaseOrganizationService service, Entity entity, EntityProperties properties)
        {
            if (entity.LogicalName == SystemUser.EntityLogicalName)
            {
                AddValueIfNotPresent(entity, properties, SystemUser.Fields.BusinessUnitId, service.Info.BusinessUnit);
                return;
            }
            if (!properties.PropertiesByLogicalName.ContainsKey(Email.Fields.OwnerId))
            {
                return;
            }

            AddValueIfNotPresent(entity, properties, Email.Fields.OwnerId, service.Info.User);
            UpdateOwningFieldsBasedOnOwner(service, entity, properties);
        }

        private static void UpdateOwningFieldsBasedOnOwner(LocalCrmDatabaseOrganizationService service, Entity entity, EntityProperties properties)
        {
            var ownerRef = entity.GetAttributeValue<EntityReference>(Email.Fields.OwnerId).Clone();
            var owner = service.Retrieve(ownerRef.LogicalName, ownerRef.Id, new ColumnSet(true));
            var bu = owner.GetAttributeValue<EntityReference>(SystemUser.Fields.BusinessUnitId)
                ?? owner.GetAttributeValue<EntityReference>(Email.Fields.OwningBusinessUnit);

            ownerRef.Name = null; // Clear Name value since owning field names aren't populated.
            ConditionallyAddValue(entity, properties, Email.Fields.OwningUser, ownerRef, owner.LogicalName == SystemUser.EntityLogicalName);
            ConditionallyAddValue(entity, properties, Email.Fields.OwningTeam, ownerRef, owner.LogicalName == Team.EntityLogicalName);

            if (bu == null)
            {
                return;
            }

            bu = bu.Clone();
            bu.Name = null;
            ConditionallyAddValue(entity, properties, Email.Fields.OwningBusinessUnit, bu);
            ConditionallyAddValue(entity, properties, SystemUser.Fields.BusinessUnitId, bu);
        }

        #endregion Ownership
    }
}
