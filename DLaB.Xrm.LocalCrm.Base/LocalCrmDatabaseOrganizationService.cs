using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using DLaB.Xrm.Client;
using DLaB.Xrm.Exceptions;
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// A ClientSide OrganizationService that connects to an in memory OrganizationService
    /// </summary>
    public partial class LocalCrmDatabaseOrganizationService : IClientSideOrganizationService
    {
        /// <summary>
        /// Gets the Crm Database Info.
        /// </summary>
        /// <value>
        /// The information.
        /// </value>
        public LocalCrmDatabaseInfo Info { get; }
        /// <summary>
        /// Gets the name of the current request.
        /// </summary>
        /// <value>
        /// The name of the current request.
        /// </value>
        public string CurrentRequestName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCrmDatabaseOrganizationService"/> class.
        /// </summary>
        /// <param name="info">The info object.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo info)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            CreateRequiredEntitiesIfNeeded();
        }

        /// <summary>
        /// Creates the current user and Business Unit entities if needed.
        /// </summary>
        private void CreateRequiredEntitiesIfNeeded()
        {
            // Create System User and Business Unit
            var user = new Entity(SystemUser.EntityLogicalName)
            {
                Id = Info.User.Id,
                [SystemUser.Fields.FirstName] = "LocalCrm",
                [SystemUser.Fields.LastName] = "DefaultUser"
            };

            if (Info.BusinessUnit.Id != Guid.Empty && this.GetEntityOrDefault(BusinessUnit.EntityLogicalName, Info.BusinessUnit.Id) == null)
            {
                Create(new Entity(BusinessUnit.EntityLogicalName)
                {
                    Id = Info.BusinessUnit.Id,
                    [BusinessUnit.Fields.Name] = "LocalCrm Default Business Unit"
                });
                user[SystemUser.Fields.BusinessUnitId] = Info.BusinessUnit;
            }
            
            if (this.GetEntityOrDefault(SystemUser.EntityLogicalName, Info.User.Id) == null)
            {
                Create(user);
            }
        }

        #region Factory Methods

        /// <summary>
        /// Creates the organization service.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static LocalCrmDatabaseOrganizationService CreateOrganizationService(LocalCrmDatabaseInfo info)
        {
            return new LocalCrmDatabaseOrganizationService(info);
        }

        /// <summary>
        /// Creates the organization service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static LocalCrmDatabaseOrganizationService CreateOrganizationService<T>()
                    where T : OrganizationServiceContext
        {
            return new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<T>());
        }

        #endregion Factory Methods

        #region IOrganizationService Members

        /// <summary>
        /// Associates the specified entity name.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
        [DebuggerStepThrough]
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!relatedEntities.Any()) { throw new ArgumentException("Must contain at least one related entity!", nameof(relatedEntities)); }
            if (relatedEntities.Any(e => e.LogicalName != relatedEntities.First().LogicalName)) { throw new NotImplementedException("Don't currently Support different Entity Types for related Entities!"); }

            if (relationship.PrimaryEntityRole.GetValueOrDefault(EntityRole.Referenced) == EntityRole.Referencing)
            {
                throw new NotImplementedException("Referencing Not Currently Implemented");
            }

            var referencedIdName = EntityHelper.GetIdAttributeName(entityName);
            var referencingIdName = EntityHelper.GetIdAttributeName(relatedEntities.First().LogicalName);
            if (referencedIdName == referencingIdName)
            {
                referencedIdName += "one";
                referencingIdName += "two";
            }


            foreach (var relation in relatedEntities.Select(relatedEntity => new Entity(relationship.SchemaName)
            {
                [referencedIdName] = entityId,
                [referencingIdName] = relatedEntity.Id
            }))
            {
                Service.Create(relation);
            }
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public Guid Create(Entity entity)
        {
            var typedEntity = entity;
            if (entity.GetType() == typeof(Entity))
            {
                typedEntity = (Entity)InvokeGenericMethod<Entity>(entity, "ToEntity", null);
            }

            return (Guid)InvokeStaticGenericMethod(entity.LogicalName, "Create", this, typedEntity);
        }

        /// <summary>
        /// Deletes the specified entity name.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        [DebuggerStepThrough]
        public void Delete(string entityName, Guid id)
        {
            InvokeStaticGenericMethod(entityName, "Delete", this, id);
        }

        /// <summary>
        /// Disassociates the entities.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="relatedEntities">The related entities.</param>
        [DebuggerStepThrough]
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!relatedEntities.Any()) { throw new ArgumentException("Must contain at least one related entity!", nameof(relatedEntities)); }
            if (relatedEntities.Any(e => e.LogicalName != relatedEntities.First().LogicalName)) { throw new NotImplementedException("Don't currently Support different Entity Types for related Entities!"); }

            if (relationship.PrimaryEntityRole.GetValueOrDefault(EntityRole.Referenced) == EntityRole.Referencing)
            {
                throw new NotImplementedException("Referencing Not Currently Implemented");
            }

            var referencedIdName = EntityHelper.GetIdAttributeName(entityName);
            var referencingIdName = EntityHelper.GetIdAttributeName(relatedEntities.First().LogicalName);
            if (referencedIdName == referencingIdName)
            {
                referencedIdName += "one";
                referencingIdName += "two";
            }


            foreach (var entity in relatedEntities.
                Select(e => QueryExpressionFactory.Create(relationship.SchemaName, referencedIdName, entityId, referencingIdName, e.Id)).
                Select(qe => Service.RetrieveMultiple(qe).ToEntityList<Entity>().FirstOrDefault()).
                Where(entity => entity != null))
            {
                Service.Delete(entity);
            }
        }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            CurrentRequestName = request.RequestName;
            var response = ExecuteInternal((dynamic)request);
            CurrentRequestName = null;
            return response;
        }

        /// <summary>
        /// Retrieves the specified entity.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="columnSet">The column set.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return (Entity)InvokeStaticGenericMethod(entityName, "Read", this, id, columnSet);
        }

        /// <summary>
        /// Retrieves the entities defined by the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return RetrieveMultipleInternal((dynamic)query);
        }

        private EntityCollection RetrieveMultipleInternal(QueryBase query)
        {
            throw new NotImplementedException("Retrieve Multiple for Query Base Type " + query.GetType().FullName + " not Impemented");
        }

        private EntityCollection RetrieveMultipleInternal(FetchExpression fetchExpression)
        {
            var s = new XmlSerializer(typeof(FetchType));
            FetchType fetch;
            using (var r = new StringReader(fetchExpression.Query))
            {
                fetch = (FetchType)s.Deserialize(r);
                r.Close();
            }
            return (EntityCollection)InvokeStaticGenericMethod(((FetchEntityType)fetch.Items[0]).name, "ReadFetchXmlEntities", this, fetch);
        }

        private EntityCollection RetrieveMultipleInternal(QueryExpression qe)
        {
            return (EntityCollection)InvokeStaticGenericMethod(qe.EntityName, "ReadEntities", this, qe);
        }

        private EntityCollection RetrieveMultipleInternal(QueryByAttribute query)
        {
            return (EntityCollection)InvokeStaticGenericMethod(query.EntityName, "ReadEntitiesByAttribute", this, query);
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        [DebuggerStepThrough]
        public void Update(Entity entity)
        {
            var typedEntity = entity;
            if (entity.GetType() == typeof(Entity))
            {
                typedEntity = (Entity)InvokeGenericMethod<Entity>(entity, "ToEntity", null);
            }

            InvokeStaticGenericMethod(entity.LogicalName, "Update", this, typedEntity);
        }

        #endregion

        /// <summary>
        /// Gets the type of the Entity.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public Type GetType(string logicalName)
        {
            return EntityHelper.GetType(Info.EarlyBoundEntityAssembly, Info.EarlyBoundNamespace, logicalName);
        }

        [DebuggerHidden]
        private object InvokeStaticGenericMethod(string logicalName, string methodName, params object[] parameters)
        {
            try
            {
                return typeof(LocalCrmDatabase).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                                                .MakeGenericMethod(GetType(logicalName))
                                                .Invoke(null, parameters);
            }
            catch (TargetInvocationException ex)
            {
                ThrowInnerException(ex);
                throw new Exception("Throw InnerException didn't throw exception");
            }
        }

        /// <summary>
        /// Attempts to throw the inner exception of the TargetInvocationException
        /// </summary>
        /// <param name="ex"></param>
        [DebuggerHidden]
        private static void ThrowInnerException(TargetInvocationException ex)
        {
            if (ex.InnerException == null) { throw new NullReferenceException("TargetInvocationException did not contain an InnerException", ex); }

            Exception exception = null;
            try
            {
                //Assume typed Exception has "new (String message, Exception innerException)" signature
                exception = (Exception) Activator.CreateInstance(ex.InnerException.GetType(), ex.InnerException.Message, ex.InnerException);
            }
            catch
            {
                //Constructor doesn't have the right constructor, eat the error and throw the inner exception below
            }

            if (exception?.InnerException == null || ex.InnerException.Message != exception.Message)
            {
                // Wasn't able to correctly create the new Exception.  Fall back to just throwing the inner exception
                throw ex.InnerException;
            }
            throw exception;
        }

        private object InvokeGenericMethod<T>(Entity entity, string methodName, params object[] parameters)
        {
            try
            {
                return typeof(T).GetMethod(methodName).MakeGenericMethod(GetType(entity.LogicalName)).Invoke(entity, parameters);
            }
            catch (TargetInvocationException ex)
            {
                ThrowInnerException(ex);
                throw new Exception("Throw InnerException didn't throw exception");
            }
        }

        /// <summary>
        /// Removes Any fields that CRM no longer returns the the client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal void RemoveFieldsCrmDoesNotReturn<T>(T entity) where T : Entity
        {
            if (entity.LogicalName == "contact")
            {
                entity.Attributes.Remove("accountid");
                entity.Attributes.Remove("parentcontactid");
            }

            // Remove all string values that are empty
            foreach (var att in entity.Attributes.Where(a => a.Value is string && string.IsNullOrEmpty((string)a.Value)).ToList())
            {
                entity.Attributes.Remove(att.Key);
            }
        }

        /// <summary>
        /// Populates the auto-populated columns of the entity and returns a list of attributes that were created / updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isCreate"></param>
        /// <returns></returns>
        internal void PopulateAutoPopulatedAttributes<T>(T entity, bool isCreate) where T : Entity
        {
            var properties = typeof (T).GetProperties();
            var name = GetFullName(entity.GetAttributeValue<string>("firstname"), 
                                   entity.GetAttributeValue<string>("middlename"),
                                   entity.GetAttributeValue<string>("lastname"));

            // TODO: Need to add logic to see if an update to the Full Name is being Performed
            ConditionallyAddAutoPopulatedValue(entity, properties, "fullname", name, !string.IsNullOrWhiteSpace(name));
            
            AutoPopulateOpportunityFields(entity, properties, isCreate);
            AutoPopulateContactFields(entity, properties);

            if (isCreate)
            {
                SetAttributeId(entity);
                SetStatusToActiveForCreate(entity);
                SetOwnerForCreate(entity, properties);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdby", Info.User, Info.User.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdonbehalfby", Info.UserOnBehalfOf, Info.UserOnBehalfOf.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdon", entity.Contains("overriddencreatedon") ? entity["overriddencreatedon"] : DateTime.UtcNow);
                ConditionallyAddAutoPopulatedValue(entity, properties, "owningbusinessunit", Info.BusinessUnit, Info.BusinessUnit.GetIdOrDefault() != Guid.Empty);
            }

            PopulateModifiedAttributes(entity, properties);
        }

        private static string GetFullName(string firstName, string middleName, string lastName)
        {
            var fullNameFormat = AppConfig.CrmSystemSettings.FullNameFormat;
            fullNameFormat = UpdateFormat(fullNameFormat, firstName, 'F');
            fullNameFormat = UpdateFormat(fullNameFormat, lastName, 'L');
            fullNameFormat = UpdateFormat(fullNameFormat, middleName, 'M');
            fullNameFormat = UpdateFormat(fullNameFormat, middleName, 'I');
            fullNameFormat = fullNameFormat.Replace("F", "{0}");
            fullNameFormat = fullNameFormat.Replace("L", "{1}");
            fullNameFormat = fullNameFormat.Replace("M", "{2}");
            fullNameFormat = fullNameFormat.Replace("I", "{3}");

            return String.Format(fullNameFormat,
                (firstName ?? "").Trim(),
                (lastName ?? "").Trim(),
                (middleName ?? "").Trim(),
                ((middleName ?? " ").Trim() + " ")[0]); 
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
            if (String.IsNullOrWhiteSpace(name))
            {
                format = format.Replace(nameFormatLetter + ", ", "");
                format = format.Replace(nameFormatLetter + " ", "");
                format = format.Replace(nameFormatLetter.ToString(), "");
            }

            return format;
        }

        /// <summary>
        /// Contact and Account Ids are populated by the customer id property.  Handle hydrating values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="isCreate">If called from Create Message</param>
        private void AutoPopulateOpportunityFields<T>(T entity, PropertyInfo[] properties, bool isCreate) where T : Entity
        {
            if (entity.LogicalName != "opportunity")
            {
                return;
            }

            var customer = entity.GetAttributeValue<EntityReference>("customerid");
            var opp = isCreate 
                ? new Entity()
                : Retrieve("opportunity", entity.Id, new ColumnSet(true));

            var parentName = "parent" + customer?.LogicalName + "id";
            // Customer was set, set correct parent field to customer
            if (customer != null 
                && !entity.Contains(parentName))
            {
                entity[parentName] = customer;
            }
            
            var hadAccount = opp.GetAttributeValue<EntityReference>("parentaccountid") != null;
            var hadContact = opp.GetAttributeValue<EntityReference>("parentcontactid") != null;

            // Customer was cleared, set correct parent field to null;
            if (entity.ContainsNullValue("customerid"))
            {
                if (hadAccount)
                {
                    entity["parentaccountid"] = null;
                }
                else if (hadContact)
                {
                    entity["parentcontactid"] = null;
                }
            }

            var hasAccount = entity.GetAttributeValue<EntityReference>("parentaccountid") != null;
            var hasContact = entity.GetAttributeValue<EntityReference>("parentcontactid") != null;
            var willHaveAccount = hasAccount
                                  || hadAccount
                                  && !entity.ContainsNullValue("parentaccountid");
            var willHaveContact = hasContact
                                  || hadContact
                                  && !entity.ContainsNullValue("parentcontactid");

            if (hasAccount)
            {
                entity["customerid"] = entity["parentaccountid"];
            }
            else if (hasContact
                && !willHaveAccount)
            {
                entity["customerid"] = entity["parentcontactid"];
            }

            if (!willHaveAccount && willHaveContact)
            {
                entity["customerid"] = entity.GetAttributeValue<EntityReference>("parentcontactid")
                                       ?? opp.GetAttributeValue<EntityReference>("parentcontactid");
            }
            else if (!willHaveAccount && !willHaveContact)
            {
                entity["customerid"] = null;
            }
        }

        /// <summary>
        /// AccountId/ParentContactId is autopopulated from the ParentCustomerId (
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        private void AutoPopulateContactFields<T>(T entity, PropertyInfo[] properties) where T : Entity
        {
            if (entity.LogicalName != "contact")
            {
                return;
            }

            var customer = entity.GetAttributeValue<EntityReference>("parentcustomerid");
            if (customer == null)
            {
                ConditionallyAddAutoPopulatedValue<EntityReference>(entity, properties, "parentcontactid", null, entity.GetAttributeValue<EntityReference>("parentcontactid") != null);
                ConditionallyAddAutoPopulatedValue<EntityReference>(entity, properties, "accountid", null, entity.GetAttributeValue<EntityReference>("accountid") != null);
                return;
            }
 
            var field = customer.LogicalName == "account" ? "accountid" : "parentcontactid";
            var siblingField = field == "parentcontactid" ? "accountid" : "parentcontactid"; // Sibling is opposite
            ConditionallyAddAutoPopulatedValue(entity, properties, field, customer, !customer.Equals(entity.GetAttributeValue<EntityReference>(field)));
            ConditionallyAddAutoPopulatedValue<EntityReference>(entity, properties, siblingField, null, entity.GetAttributeValue<EntityReference>(siblingField) != null);
        }

        private void PopulateModifiedAttributes<T>(T entity, PropertyInfo[] properties) where T : Entity
        {
            ConditionallyAddAutoPopulatedValue(entity, properties, "modifiedby", Info.User, Info.User.GetIdOrDefault() != Guid.Empty);
            ConditionallyAddAutoPopulatedValue(entity, properties, "modifiedonbehalfby", Info.UserOnBehalfOf, Info.UserOnBehalfOf.GetIdOrDefault() != Guid.Empty);
            ConditionallyAddAutoPopulatedValue(entity, properties, "modifiedon", DateTime.UtcNow);
        }

        private void ConditionallyAddAutoPopulatedValue<T>(Entity entity, PropertyInfo[] properties, string attributeName, T value, bool condition = true)
        {
            if (!condition || !properties.Any(p => p.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var date = value as DateTime?;
            if (date.HasValue)
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
        private void SetAttributeId<T>(T entity) where T : Entity
        {
            if (entity.Id == Guid.Empty) { return; }
            var attribute = EntityPropertiesCache.Instance.For<T>().GetProperty("Id").GetAttributeLogicalName();
            entity[attribute] = entity.Id;
        }

        private void SetStatusToActiveForCreate<T>(T entity) where T : Entity
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

        /// <summary>
        /// Sets the owner to the Info.User if it is null.  Only to be called on Create.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        private void SetOwnerForCreate(Entity entity, PropertyInfo[] properties)
        {
            var owner = properties.FirstOrDefault(p => p.Name.Equals(Email.Fields.OwnerId, StringComparison.InvariantCultureIgnoreCase));
            if (owner == null || entity.Attributes.ContainsKey(Email.Fields.OwnerId))
            {
                return;
            }

            entity[Email.Fields.OwnerId] = Info.User;
        }

        #region IClientSideOrganizationService Members

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public IOrganizationService Service
        {
            get
            {
                return this;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the service URI.
        /// </summary>
        /// <returns></returns>
        public Uri GetServiceUri()
        {
            return new Uri(@"http://www.LocalCrmDatabase.com/" + Info.DatabaseName);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            // Nothing to Dispose
        }

        #endregion
    }
}
