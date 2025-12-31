using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// POCO for configuring the Many2ManyAssociationProvider
    /// </summary>
    public class Many2ManyRelationshipDefinition
    {
        /// <summary>
        /// The primary entity logical name.
        /// </summary>
        public string? PrimaryEntityType  { get; set; }
        /// <summary>
        /// The attribute name of the primary entity on the relationship entity.
        /// </summary>
        public string? PrimaryEntityIdName { get; set; }
        /// <summary>
        /// The attribute name of the associated entity on the relationship entity
        /// </summary>
        public string? AssociatedEntityIdName { get; set; }
        /// <summary>
        /// The associated entity logical name
        /// </summary>
        public string? AssociatedEntityType { get; set; }
        /// <summary>
        /// The entity logical name of the N2N entity.
        /// </summary>
        public string? AssociationLogicalName { get; set; }
        /// <summary>
        /// Expects format of {RelationshipEntityLogicalName},{PrimaryEntityLogicalName},{PrimaryEntityIdName},{AssociatedEntityIdType},{AssociatedEntityIdName}.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <returns></returns>
        public static Many2ManyRelationshipDefinition Parse(List<string> parts)
        {
            if(parts.Count != 5)
            {
                throw new FormatException($"Unable to parse {string.Join(", ", parts)}!  Expected 5 items ({{RelationshipEntityLogicalName}},{{PrimaryEntityLogicalName}},{{PrimaryEntityIdName}},{{AssociatedEntityIdType}},{{AssociatedEntityIdName}})!");
            }

            return new Many2ManyRelationshipDefinition
            {
                AssociationLogicalName = parts[0],
                PrimaryEntityType = parts[1],
                PrimaryEntityIdName = parts[2],
                AssociatedEntityType = parts[3],
                AssociatedEntityIdName = parts[4]
            };
        }
    }
}
