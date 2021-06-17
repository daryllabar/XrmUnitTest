using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// IMany2ManyAssociationProvider Implementation
    /// </summary>
    public class Many2ManyAssociationProvider : IMany2ManyAssociationProvider
    {
        private Dictionary<string, Many2ManyRelationshipDefinition> DefinitionsByRelationshipName { get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions"></param>
        public Many2ManyAssociationProvider(Dictionary<string, Many2ManyRelationshipDefinition> definitions)
        {
            DefinitionsByRelationshipName = definitions;
        }

        /// <summary>
        /// Returns true of the relationship name is a many to many relationship
        /// </summary>
        /// <param name="relationshipName">The name of the relationship</param>
        /// <returns></returns>
        public bool IsManyToManyRelationship(string relationshipName)
        {
            return DefinitionsByRelationshipName.ContainsKey(relationshipName);
        }

        /// <summary>
        /// Returns true of the entity logical name is a many to many relationship entity.
        /// </summary>
        /// <param name="logicalName">The logicalName of the entity.</param>
        /// <returns></returns>
        public bool IsManyToManyJoinEntity(string logicalName)
        {
            return DefinitionsByRelationshipName.Values.Any(v => v.AssociationLogicalName == logicalName);
        }


        /// <summary>
        /// Gets the relationship name for the given N:N entity.
        /// </summary>
        /// <param name="logicalName">The N:N Entity.</param>
        /// <returns></returns>
        public string GetRelationshipNameForJoinEntity(string logicalName)
        {
            var entry = DefinitionsByRelationshipName.FirstOrDefault(kvp => kvp.Value.AssociationLogicalName == logicalName);
            if (entry.Equals(default(KeyValuePair<string, Many2ManyRelationshipDefinition>))) 
            {
                throw new KeyNotFoundException($"Join Entity {logicalName} was not found in the DefinitionsByRelationshipName dictionary.");
            }
            return entry.Key;
        }

        /// <summary>
        /// Creates the Association
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Type: String. The logical name of the entity that is specified in the <paramref name="entityId" /> parameter.</param>
        /// <param name="entityId">Type: Guid. The Id of the record to which the related records are associated.</param>
        /// <param name="relationship">Type: <see cref="T:Microsoft.Xrm.Sdk.Relationship" />. The name of the relationship to be used to create the link.</param>
        /// <param name="relatedEntities">Type: <see cref="T:Microsoft.Xrm.Sdk.EntityReferenceCollection" />. A collection of entity references (references to records) to be associated.</param>
        public Guid[] CreateAssociation(IOrganizationService service, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if(!DefinitionsByRelationshipName.TryGetValue(relationship.SchemaName, out var definition))
            {
                throw new KeyNotFoundException($"Schema name {relationship.SchemaName} was not found in the DefinitionsByRelationshipName dictionary.");
            }

            var primaryAttributeName = entityName == definition.PrimaryEntityType
                ? definition.PrimaryEntityIdName
                : definition.AssociatedEntityIdName;
            var associatedAttributeName = entityName == definition.PrimaryEntityType
                ? definition.AssociatedEntityIdName
                : definition.PrimaryEntityIdName;
            var ids = new List<Guid>();
            foreach (var relation in relatedEntities.Select(relatedEntity => new Entity(definition.AssociationLogicalName)
            {
                [primaryAttributeName] = entityId,
                [associatedAttributeName] = relatedEntity.Id
            }))
            {
                ids.Add(service.Create(relation));
            }

            return ids.ToArray();
        }

        /// <summary>
        /// Creates the Association Request
        /// </summary>
        /// <param name="many2ManyEntity">Many to Many Entity.</param>
        public AssociateRequest CreateAssociateRequest(Entity many2ManyEntity)
        {
            var schemaName = GetRelationshipNameForJoinEntity(many2ManyEntity.LogicalName);
            var info = DefinitionsByRelationshipName[schemaName];
            var request = new AssociateRequest
            {
                Relationship = new Relationship(schemaName),
                Target = new EntityReference(info.PrimaryEntityType, many2ManyEntity.GetAttributeValue<Guid?>(info.PrimaryEntityIdName).GetValueOrDefault()),
                RelatedEntities = new EntityReferenceCollection(new List<EntityReference>{
                    new EntityReference(info.AssociatedEntityType, many2ManyEntity.GetAttributeValue<Guid?>(info.AssociatedEntityIdName).GetValueOrDefault())
                })
            };

            if(request.Target.LogicalName == request.RelatedEntities.First().LogicalName)
            {
                request.Relationship.PrimaryEntityRole = EntityRole.Referenced;
            }

            return request;
        }

        /// <summary>
        /// Error given if the relationship is not defined.
        /// </summary>
        /// <returns></returns>
        public string GetNotFoundErrorMessage()
        {
            return "If this is a N:N relationship make sure it is defined in the CrmEntities.Many2ManyAssociationDefinitions app config.";
        }

        /// <summary>
        /// Removes the Association
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Type: String. The logical name of the entity that is specified in the <paramref name="entityId" /> parameter.</param>
        /// <param name="entityId">Type: Guid. The Id of the record to which the related records are associated.</param>
        /// <param name="relationship">Type: <see cref="T:Microsoft.Xrm.Sdk.Relationship" />. The name of the relationship to be used to create the link.</param>
        /// <param name="relatedEntities">Type: <see cref="T:Microsoft.Xrm.Sdk.EntityReferenceCollection" />. A collection of entity references (references to records) to be associated.</param>
        public void RemoveAssociation(IOrganizationService service, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (!DefinitionsByRelationshipName.TryGetValue(relationship.SchemaName, out var definition))
            {
                throw new KeyNotFoundException($"Schema name {relationship.SchemaName} was not found in the DefinitionsByRelationshipName dictionary.");
            }

            var primaryAttributeName = entityName == definition.PrimaryEntityType
                ? definition.PrimaryEntityIdName
                : definition.AssociatedEntityIdName;
            var associatedAttributeName = entityName == definition.PrimaryEntityType
                ? definition.AssociatedEntityIdName
                : definition.PrimaryEntityIdName;
            foreach (var relation in relatedEntities.Select(relatedEntity => new Entity(definition.AssociationLogicalName)
            {
                [primaryAttributeName] = entityId,
                [associatedAttributeName] = relatedEntity.Id
            }))
            {
                service.Create(relation);
            }
        }

        /// <summary>
        /// Removes the Association
        /// </summary>
        /// <param name="many2ManyEntity">Many to Many Entity.</param>
        public DisassociateRequest CreateDisassociateRequest(Entity many2ManyEntity)
        {
            var schemaName = GetRelationshipNameForJoinEntity(many2ManyEntity.LogicalName);
            var info = DefinitionsByRelationshipName[schemaName];
            return new DisassociateRequest
            {
                Relationship = new Relationship(schemaName),
                Target = new EntityReference(info.PrimaryEntityType, many2ManyEntity.GetAttributeValue<Guid?>(info.PrimaryEntityIdName).GetValueOrDefault()),
                RelatedEntities = new EntityReferenceCollection(new List<EntityReference>
                {
                    new EntityReference(info.AssociationLogicalName, many2ManyEntity.GetAttributeValue<Guid?>(info.AssociatedEntityIdName).GetValueOrDefault())
                })
            };
        }
    }
}
