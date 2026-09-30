using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DLaB.Xrm.LocalCrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest
#else

namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Contains the information required to associate an N:N relationship (intersect) entity, for N:N relationships that have not been
    /// defined in the CrmEntities.Many2ManyAssociationDefinitions app config.
    /// </summary>
    internal class N2NIntersectInfo
    {
        /// <summary>
        /// The logical name of the intersect entity.  This is also the schema name of the relationship.
        /// </summary>
        public string LogicalName { get; }

        /// <summary>
        /// The logical name of the entity that will be used as the target of the associate.
        /// </summary>
        public string PrimaryEntityLogicalName { get; }

        /// <summary>
        /// The attribute of the intersect entity that contains the id of the <see cref="PrimaryEntityLogicalName"/> entity.
        /// </summary>
        public string PrimaryEntityIdName { get; }

        /// <summary>
        /// The logical name of the entity that will be used as the related entity of the associate.
        /// </summary>
        public string AssociatedEntityLogicalName { get; }

        /// <summary>
        /// The attribute of the intersect entity that contains the id of the <see cref="AssociatedEntityLogicalName"/> entity.
        /// </summary>
        public string AssociatedEntityIdName { get; }

        private N2NIntersectInfo(string logicalName, string primaryEntityLogicalName, string primaryEntityIdName, string associatedEntityLogicalName, string associatedEntityIdName)
        {
            LogicalName = logicalName;
            PrimaryEntityLogicalName = primaryEntityLogicalName;
            PrimaryEntityIdName = primaryEntityIdName;
            AssociatedEntityLogicalName = associatedEntityLogicalName;
            AssociatedEntityIdName = associatedEntityIdName;
        }

        private static readonly ConcurrentDictionary<string, N2NIntersectInfo?> InfosByLogicalName = new ConcurrentDictionary<string, N2NIntersectInfo?>();

        /// <summary>
        /// Returns the N2NIntersectInfo for the given logical name, or null if the entity is not an N:N relationship (intersect) entity,
        /// or the entities it relates could not be determined.
        /// </summary>
        /// <param name="logicalName">The logical name of the entity.</param>
        public static N2NIntersectInfo? GetOrDefault(string logicalName)
        {
            return InfosByLogicalName.GetOrAdd(logicalName, CreateOrDefault);
        }

        private static N2NIntersectInfo? CreateOrDefault(string logicalName)
        {
            if (!EntityHelper.IsTypeDefined(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, logicalName))
            {
                return null;
            }

            var properties = EntityPropertiesCache.Instance.For(TestBase.GetType(logicalName));
            if (!properties.IsManyToManyIntersect
                || properties.ManyToManyIntersectIdAttributes.Length != 2)
            {
                return null;
            }

            var primaryIdName = properties.ManyToManyIntersectIdAttributes[0];
            var associatedIdName = properties.ManyToManyIntersectIdAttributes[1];
            var primaryLogicalName = GetEntityLogicalNameOrDefault(primaryIdName);
            var associatedLogicalName = GetEntityLogicalNameOrDefault(associatedIdName);

            return primaryLogicalName == null || associatedLogicalName == null
                ? null
                : new N2NIntersectInfo(logicalName, primaryLogicalName, primaryIdName, associatedLogicalName, associatedIdName);
        }

        /// <summary>
        /// The attributes of an intersect entity are the primary id attributes of the two entities being related.
        /// </summary>
        private static string? GetEntityLogicalNameOrDefault(string idAttributeName)
        {
            if (!idAttributeName.EndsWith("id", StringComparison.Ordinal))
            {
                return null;
            }

            var logicalName = idAttributeName.Substring(0, idAttributeName.Length - "id".Length);
            return EntityHelper.IsTypeDefined(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, logicalName)
                   && EntityHelper.GetIdAttributeName(TestBase.GetType(logicalName)) == idAttributeName
                ? logicalName
                : null;
        }

        /// <summary>
        /// Returns the logical name of the related entity, by the attribute name that contains its id.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> GetRelatedEntityLogicalNamesByIdAttribute()
        {
            yield return new KeyValuePair<string, string>(PrimaryEntityIdName, PrimaryEntityLogicalName);
            yield return new KeyValuePair<string, string>(AssociatedEntityIdName, AssociatedEntityLogicalName);
        }

        /// <summary>
        /// Creates the AssociateRequest for the given intersect entity.
        /// </summary>
        /// <param name="intersectEntity">The N:N relationship (intersect) entity.</param>
        public AssociateRequest CreateAssociateRequest(Entity intersectEntity)
        {
            return new AssociateRequest
            {
                Relationship = new Relationship(LogicalName),
                Target = new EntityReference(PrimaryEntityLogicalName, GetId(intersectEntity, PrimaryEntityIdName)),
                RelatedEntities = new EntityReferenceCollection
                {
                    new EntityReference(AssociatedEntityLogicalName, GetId(intersectEntity, AssociatedEntityIdName))
                }
            };
        }

        private Guid GetId(Entity intersectEntity, string attributeName)
        {
            var value = intersectEntity.Contains(attributeName)
                ? intersectEntity[attributeName]
                : null;
            switch (value)
            {
                case Guid id when id != Guid.Empty:
                    return id;
                case EntityReference reference when reference.Id != Guid.Empty:
                    return reference.Id;
                default:
                    throw new Exception($"Unable to associate {LogicalName}.  The attribute \"{attributeName}\" must be populated with the id of the {(attributeName == PrimaryEntityIdName ? PrimaryEntityLogicalName : AssociatedEntityLogicalName)} to associate.");
            }
        }
    }
}
