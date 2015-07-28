using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using DLaB.Common;
using DLaB.Xrm.Client;
using DLaB.Xrm.Exceptions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm
{
    public partial class LocalCrmDatabaseOrganizationService : IClientSideOrganizationService
    {
        public LocalCrmDatabaseInfo Info { get; private set; }

        // ReSharper disable once UnusedMember.Local
        private LocalCrmDatabaseOrganizationService()
        {
            
        }

        public LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Info = info;
        }

        public static LocalCrmDatabaseOrganizationService CreateOrganizationService<T>(LocalCrmDatabaseInfo info = null) 
            where T : OrganizationServiceContext
        {
            info = info ?? LocalCrmDatabaseInfo.Create<T>();
            return new LocalCrmDatabaseOrganizationService(info);
        }

        #region IOrganizationService Members

        [DebuggerStepThrough]
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!relatedEntities.Any()) { throw new ArgumentException("Must contain at least one related entity!", "relatedEntities");}
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


            foreach (var relatedEntity in relatedEntities)
            {
                var relation = new Entity(relationship.SchemaName);
                relation[referencedIdName] = entityId;
                relation[referencingIdName] = relatedEntity.Id;
                Service.Create(relation);
            }
        }

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

        [DebuggerStepThrough]
        public void Delete(string entityName, Guid id)
        {
            InvokeStaticGenericMethod(entityName, "Delete", this, id);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!relatedEntities.Any()) { throw new ArgumentException("Must contain at least one related entity!", "relatedEntities"); }
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

        [DebuggerStepThrough]
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return ExecuteInternal((dynamic)request);
        }

        [DebuggerStepThrough]
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return (Entity)InvokeStaticGenericMethod(entityName, "Read", this, id, columnSet);
        }

        [DebuggerStepThrough]
        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            var fetchQuery = query as FetchExpression;
            if (fetchQuery != null)
            {
                XmlSerializer s = new XmlSerializer(typeof(FetchXml.FetchType));
                FetchXml.FetchType fetch;
                using (var r = new System.IO.StringReader(fetchQuery.Query))
                {
                    fetch = (FetchXml.FetchType)s.Deserialize(r);
                    r.Close();
                }
                return (EntityCollection)InvokeStaticGenericMethod(((FetchXml.FetchEntityType)fetch.Items[0]).name, "ReadFetchXmlEntities", this, fetch);
            }
            var qe = (QueryExpression)query;

            return (EntityCollection)InvokeStaticGenericMethod(qe.EntityName, "ReadEntities", this, qe);
        }

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

            if (exception == null ||
                exception.InnerException == null ||
                ex.InnerException.Message != exception.Message)
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
            ConditionallyAddAutoPopulatedValue(entity, properties, "fullname", name, !String.IsNullOrWhiteSpace(name));
            
            AutoPopulateOpportunityFields(entity, properties);

            if (isCreate)
            {
                // Set Id
                SetAttributeId(entity);
                SetStatusToActiveForCreate(entity);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdby", Info.User, Info.User.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdonbehalfby", Info.UserOnBehalfOf, Info.UserOnBehalfOf.GetIdOrDefault() != Guid.Empty);
                ConditionallyAddAutoPopulatedValue(entity, properties, "createdon", entity.Contains("overriddencreatedon") ? entity["overriddencreatedon"] : DateTime.UtcNow);
                ConditionallyAddAutoPopulatedValue(entity, properties, "owningbusinessunit", Info.BusinessUnit, Info.BusinessUnit.GetIdOrDefault() != Guid.Empty);
            }

            PopulateModifiedAttributes(entity, properties);
        }

        private static string GetFullName(string firstName, string middleName, string lastName)
        {
            var fullNameFormat = Config.GetAppSettingOrDefault("CrmSystemSettings.FullNameFormat", "F I L").ToUpper();
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
        /// Contact and Account Ids are popualted by the customer id property.  Handle hydrating values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        private void AutoPopulateOpportunityFields<T>(T entity, PropertyInfo[] properties) where T : Entity
        {
            if (entity.LogicalName != "opportunity")
            {
                return;
            }

            var customer = entity.GetAttributeValue<EntityReference>("customerid");
            if (customer == null)
            {
                ConditionallyAddAutoPopulatedValue<EntityReference>(entity, properties, "parentcontactid", null, entity.GetAttributeValue<EntityReference>("parentcontactid") != null);
                ConditionallyAddAutoPopulatedValue<EntityReference>(entity, properties, "parentaccountid", null, entity.GetAttributeValue<EntityReference>("parentaccountid") != null);
                return;
            }

            var field = "parent" + entity.GetAttributeValue<EntityReference>("customerid").LogicalName + "id";
            var siblingField = field == "parentcontactid" ? "parentaccountid" : "parentcontactid"; // Sibling is opposite
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

            entity[attributeName] = value;
        }

        /// <summary>
        /// Populate the Attribute Id Attribute.  This is required because creating a new Entity of type Entity, doesn't populate the Entity's Typed Id Attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        private void SetAttributeId<T>(T entity) where T : Entity
        {
            if (entity.Id == Guid.Empty) { return; }
            var attribute = (AttributeLogicalNameAttribute) typeof (T).GetProperty("Id").GetCustomAttributes(typeof (AttributeLogicalNameAttribute), false)[0];
            entity[attribute.LogicalName] = entity.Id;
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

        #region IClientSideOrganizationService Members

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

        #endregion

        #region IIOrganizationServiceWrapper Members

        public Uri GetServiceUri()
        {
            return new Uri(@"http://www.LocalCrmDatabase.com/" + Info.DatabaseName);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // Nothing to Dispose
        }

        #endregion
    }
}
