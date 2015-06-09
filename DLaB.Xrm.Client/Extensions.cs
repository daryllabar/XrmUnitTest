using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace DLaB.Xrm.Client
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the actual type of the entity as defined by the entity LogicalName, even if it is just an entity.
        /// ie: (new Entity(Contact.EntityLogicalName)).GetEntityType() == (new Contact()).GetType()
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Type GetEntityType(this Entity entity)
        {
            var assembly = CrmServiceUtility.GetEarlyBoundProxyAssembly();
            foreach (var t in assembly.GetTypes())
            {
                var attribute =
                    (EntityLogicalNameAttribute)
                    t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), false).FirstOrDefault();
                if (attribute != null && attribute.LogicalName == entity.LogicalName)
                {
                    return t;
                }
            }
            throw new Exception("Type " + entity.LogicalName + " Not found!");
        }
    }
}
