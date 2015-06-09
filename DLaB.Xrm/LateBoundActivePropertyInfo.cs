using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm
{
    public class LateBoundActivePropertyInfo : ActivePropertyInfo<Entity>
    {
        public LateBoundActivePropertyInfo(String logicalName) : base(logicalName) { }

        public static bool? IsActive(IOrganizationService service, Entity entity)
        {
            return IsActive(service, entity.LogicalName, entity.Id);
        }

        public static bool? IsActive(IOrganizationService service, string logicalName, Guid entityId)
        {
            var info = new LateBoundActivePropertyInfo(logicalName);
            var entity = service.Retrieve(logicalName, entityId, new ColumnSet(info.AttributeName));
            return IsActive(info, entity);
        }

        [Obsolete("IsActive(IOrganizationService,Guid) signature is not supported for LateBound", true)]
        public static new bool? IsActive(IOrganizationService service, Guid entityId)
        {
            throw new NotSupportedException("IsActive(IOrganizationService,Guid) signature is not supported for LateBound");
        }
    }
}
