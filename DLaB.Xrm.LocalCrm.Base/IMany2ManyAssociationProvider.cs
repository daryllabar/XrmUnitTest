using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Defines an Interface for a Many 2 Many Association provider.  This is needed to be able to correctly handle N2N Associations.
    /// </summary>
    public interface IMany2ManyAssociationProvider
    {
        /// <summary>
        /// Returns true of the relationship name is a many to many relationship
        /// </summary>
        /// <param name="relationshipName">The name of the relationship</param>
        /// <returns></returns>
        bool IsManyToManyRelationship(string relationshipName);

        /// <summary>
        /// Returns true of the entity logical name is a many to many relationship entity.
        /// </summary>
        /// <param name="logicalName">The logicalName of the entity.</param>
        /// <returns></returns>
        bool IsManyToManyJoinEntity(string logicalName);

        /// <summary>
        /// Creates the Association, returning the created ids
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Type: String. The logical name of the entity that is specified in the <paramref name="entityId" /> parameter.</param>
        /// <param name="entityId">Type: Guid. The Id of the record to which the related records are associated.</param>
        /// <param name="relationship">Type: <see cref="T:Microsoft.Xrm.Sdk.Relationship" />. The name of the relationship to be used to create the link.</param>
        /// <param name="relatedEntities">Type: <see cref="T:Microsoft.Xrm.Sdk.EntityReferenceCollection" />. A collection of entity references (references to records) to be associated.</param>
        Guid[] CreateAssociation(IOrganizationService service, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities);

        /// <summary>
        /// Creates the AssociationRequest request
        /// </summary>
        /// <param name="many2ManyEntity">Many to Many Entity.</param>
        AssociateRequest CreateAssociateRequest(Entity many2ManyEntity);

        /// <summary>
        /// Gets the relationship name for the given N:N entity.
        /// </summary>
        /// <param name="logicalName">The N:N Entity.</param>
        /// <returns></returns>
        string GetRelationshipNameForJoinEntity(string logicalName);

        /// <summary>
        /// Error given if the relationship is not defined.
        /// </summary>
        /// <returns></returns>
        string GetNotFoundErrorMessage();

        /// <summary>
        /// Removes the Association
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entityName">Type: String. The logical name of the entity that is specified in the <paramref name="entityId" /> parameter.</param>
        /// <param name="entityId">Type: Guid. The Id of the record to which the related records are associated.</param>
        /// <param name="relationship">Type: <see cref="T:Microsoft.Xrm.Sdk.Relationship" />. The name of the relationship to be used to create the link.</param>
        /// <param name="relatedEntities">Type: <see cref="T:Microsoft.Xrm.Sdk.EntityReferenceCollection" />. A collection of entity references (references to records) to be associated.</param>
        void RemoveAssociation(IOrganizationService service, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities);

        /// <summary>
        /// Creates a DisassociateRequest request
        /// </summary>
        /// <param name="many2ManyEntity">Many to Many Entity.</param>
        DisassociateRequest CreateDisassociateRequest(Entity many2ManyEntity);
    }
}
