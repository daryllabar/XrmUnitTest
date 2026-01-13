using DLaB.Xrm.Client;
using DLaB.Xrm.LocalCrm.Entities;
using DLaB.Xrm.LocalCrm.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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
        public string? CurrentRequestName { get; private set; }
        
        /// <summary>
        /// If a mirrored entity request is being processed, doesn't re-mirror the request
        /// </summary>
        internal bool MirroredEntityRequestTriggered { get; set; }

        private bool EnforceValidForOperationCheck { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCrmDatabaseOrganizationService"/> class.
        /// </summary>
        /// <param name="info">The info object.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo info)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
            EnforceValidForOperationCheck = true;
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
            AssociateInternal(entityName, entityId, relationship, relatedEntities);
        }

        private AssociateResponse AssociateInternal(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!relatedEntities.Any())
            {
                throw new ArgumentException("Must contain at least one related entity!", nameof(relatedEntities));
            }

            if (relatedEntities.Any(e => e.LogicalName != relatedEntities.First().LogicalName))
            {
                throw new NotImplementedException("Don't currently Support different Entity Types for related Entities!");
            }

            if (relationship.PrimaryEntityRole.GetValueOrDefault(EntityRole.Referenced) == EntityRole.Referencing)
            {
                throw new NotImplementedException("Referencing Not Currently Implemented");
            }

            var response = new AssociateResponse();
            if (Info.ManyToManyAssociationProvider.IsManyToManyRelationship(relationship.SchemaName))
            {
                var originalValue = EnforceValidForOperationCheck;
                EnforceValidForOperationCheck = false;
                try
                {
                    response["CreatedIds"] = Info.ManyToManyAssociationProvider.CreateAssociation(Service, entityName, entityId, relationship, relatedEntities);

                }
                finally
                {
                    EnforceValidForOperationCheck = originalValue;
                }
            }
            else if (EntityHelper.IsTypeDefined(Info.EarlyBoundEntityAssembly, Info.EarlyBoundNamespace, relationship.SchemaName))
            {
                var referencedIdName = EntityHelper.GetIdAttributeName(GetType(entityName));
                var referencingIdName = EntityHelper.GetIdAttributeName(GetType(relatedEntities.First().LogicalName));
                if (referencedIdName == referencingIdName)
                {
                    referencedIdName += "one";
                    referencingIdName += "two";
                }

                Associate1ToN(entityId, relationship, relatedEntities, referencedIdName, referencingIdName);
            }
            else
            {
                throw new NotImplementedException($"No entity found with logical name '{relationship.SchemaName}' for 1:N relationship!  {Info.ManyToManyAssociationProvider.GetNotFoundErrorMessage()}");
            }
            return response;
        }

        private void Associate1ToN(Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
            string referencedIdName, string referencingIdName)
        {
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
            AssertValidForOperation(entity, nameof(Create));
            if (entity.GetType() == typeof(Entity))
            {
                entity = GenericMethodCaller.InvokeToEntity(entity, Info);
            }

            var id = (Guid)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, entity.LogicalName, nameof(Create), this, entity);
            foreach(var relationship in entity.RelatedEntities)
            {
                foreach(var relatedEntity in relationship.Value.Entities)
                {
                    var clone = relatedEntity.Clone();
                    if (clone.GetType() == typeof(Entity))
                    {
                        clone = GenericMethodCaller.InvokeToEntity(clone, Info);
                    }
                    clone[relationship.Key.GetReferencingAttributeName(clone.GetType())] = new EntityReference(entity.LogicalName, id);
                    GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, clone.LogicalName, nameof(Create), this, clone);
                }
            }

            return id;
        }

        /// <summary>
        /// Deletes the specified entity name.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="id">The identifier.</param>
        [DebuggerStepThrough]
        public void Delete(string entityName, Guid id)
        {
            AssertValidForOperation(entityName, nameof(Delete));
            GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, entityName, nameof(Delete), this, id);
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

            if (Info.ManyToManyAssociationProvider.IsManyToManyRelationship(relationship.SchemaName))
            {
                var originalValue = EnforceValidForOperationCheck;
                EnforceValidForOperationCheck = false;
                try
                {
                    Info.ManyToManyAssociationProvider.RemoveAssociation(Service, entityName, entityId, relationship, relatedEntities);
                }
                finally
                {
                    EnforceValidForOperationCheck = originalValue;
                }
            }
            else if (EntityHelper.IsTypeDefined(Info.EarlyBoundEntityAssembly, Info.EarlyBoundNamespace, relationship.SchemaName))
            {
                var referencedIdName = EntityHelper.GetIdAttributeName(GetType(entityName));
                var referencingIdName = EntityHelper.GetIdAttributeName(GetType(relatedEntities.First().LogicalName));
                if (referencedIdName == referencingIdName)
                {
                    referencedIdName += "one";
                    referencingIdName += "two";
                }
                Disassociate1ToN(entityId, relationship, relatedEntities, referencedIdName, referencingIdName);
            }
            else
            {
                throw new NotImplementedException($"No entity found with logical name '{relationship.SchemaName}' for 1:N relationship!  {Info.ManyToManyAssociationProvider.GetNotFoundErrorMessage()}");
            }
        }

        private void Disassociate1ToN(Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities,
            string referencedIdName, string referencingIdName)
        {
            foreach (var entity in relatedEntities
                .Select(e => QueryExpressionFactory.Create(relationship.SchemaName, referencedIdName, entityId,
                    referencingIdName, e.Id))
                .Select(qe => Service.RetrieveMultiple(qe).ToEntityList<Entity>().FirstOrDefault())
                .Where(entity => entity != null))
            {
                Service.Delete(entity!);
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
            return (Entity)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, entityName, "Read", this, id, columnSet);
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
            throw new NotImplementedException("Retrieve Multiple for Query Base Type " + query.GetType().FullName + " not Implemented");
        }

        private EntityCollection RetrieveMultipleInternal(FetchExpression fetchExpression)
        {
            var s = new XmlSerializer(typeof(FetchType));
            FetchType fetch;
            using (var r = new StringReader(fetchExpression.Query))
            {
                fetch = (FetchType)s.Deserialize(r)!;
                r.Close(); 
            }
            return (EntityCollection)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, ((FetchEntityType)fetch.Items[0]).name, "ReadFetchXmlEntities", this, fetch);
        }

        private EntityCollection RetrieveMultipleInternal(QueryExpression qe)
        {
            return (EntityCollection)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, qe.EntityName, "ReadEntities", this, qe);
        }

        private EntityCollection RetrieveMultipleInternal(QueryByAttribute query)
        {
            return (EntityCollection)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, query.EntityName, "ReadEntitiesByAttribute", this, query);
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        [DebuggerStepThrough]
        public void Update(Entity entity)
        {
            AssertValidForOperation(entity, "Update");
            if (entity.GetType() == typeof(Entity))
            {
                entity = GenericMethodCaller.InvokeToEntity(entity, Info);
            }

            GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, entity.LogicalName, "Update", this, entity);
        }

        #endregion

        [DebuggerHidden]
        internal Guid CreateActivityParty(Entity entity)
        {
            if (entity.GetType() == typeof(Entity))
            {
                entity = GenericMethodCaller.InvokeToEntity(entity, Info);
            }
            return (Guid)GenericMethodCaller.InvokeLocalCrmDatabaseStaticGenericMethod(Info, entity.LogicalName, "Create", this, entity);
        }

        /// <summary>
        /// Gets the type of the Entity.
        /// </summary>
        /// <param name="logicalName">Name of the logical.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public Type GetType(string logicalName)
        {
            return LocalCrmDatabase.GetType(Info, logicalName);
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
            foreach (var att in entity.Attributes.Where(a => a.Value is string && string.IsNullOrEmpty((string)a.Value)
                                                          || a.Key.StartsWith(LocalCrmDatabase.JoinAliasEntityPreFix) && a.Value is Entity).ToList())
            {
                entity.Attributes.Remove(att.Key);
            }
        }

        [DebuggerHidden]
        private void AssertValidForOperation(Entity entity, string operation)
        {
            AssertValidForOperation(entity.LogicalName, operation);
        }

        [DebuggerHidden]
        private void AssertValidForOperation(string logicalName, string operation)
        {
            if (!EnforceValidForOperationCheck || Info.AllowCrudOperationsForEntities.Contains(logicalName))
            {
                return;
            }
            switch (logicalName)
            {
                case ActivityParty.EntityLogicalName:
                case ConnectionRoleAssociation.EntityLogicalName when operation is nameof(Create) or nameof(Delete):
                case PrincipalObjectAccess.EntityLogicalName when operation is nameof(Create) or nameof(Update) or nameof(Delete):
                    throw CrmExceptions.GetOperationDoesNotSupportEntitiesOfTypeException(operation, logicalName);
            }
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
