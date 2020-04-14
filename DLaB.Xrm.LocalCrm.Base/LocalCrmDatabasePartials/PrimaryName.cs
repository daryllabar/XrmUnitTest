using System;
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm
{
    internal partial class LocalCrmDatabase
    {
        private readonly Dictionary<EntityReference, string> _namesById = new Dictionary<EntityReference, string>();

        private static void SetCachePrimaryName<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            var cache = GetDatabaseForService(service.Info)._namesById;
            var attributeName = service.Info.PrimaryNameProvider.GetPrimaryName<T>();
            var name = string.Empty;
            if (attributeName != string.Empty)
            {
                if (string.IsNullOrWhiteSpace(attributeName))
                {
                    throw new Exception($"Entity {entity.LogicalName} returned a null or whitespace primary name attribute.");
                }

                name = entity.GetAttributeValue<string>(attributeName);
            }

            cache[entity.ToEntityReference()] = name;
        }

        /// <summary>
        /// Attributes where the Name property of the Entity Reference is not populated
        /// </summary>
        private static readonly HashSet<string> EntityRefNameExceptions = new HashSet<string>
        {
            Email.Fields.OwningBusinessUnit,
            Email.Fields.OwningUser,
            Email.Fields.OwningTeam
        };

        private static void PopulateReferenceNames<T>(LocalCrmDatabaseOrganizationService service, T entity) where T : Entity
        {
            var cache = GetDatabaseForService(service.Info)._namesById;
            foreach (var att in entity.Attributes
                                      .Where(a => !EntityRefNameExceptions.Contains(a.Key))
                                      .Select(a => a.Value)
                                      .OfType<EntityReference>())
            {
                try
                {
                    att.Name = cache[att];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("Name Cache did not contain a name for " + att.ToStringDebug());
                }
            }
        }
    }
}
