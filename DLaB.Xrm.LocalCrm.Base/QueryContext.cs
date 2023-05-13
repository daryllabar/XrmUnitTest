using System;

namespace DLaB.Xrm.LocalCrm
{
    internal class QueryContext
    {
        public Guid UserId { get; set; }
        public Guid BusinessUnitId { get; set; }

        public QueryContext() { }

        public QueryContext(LocalCrmDatabaseInfo info)
        {
            UserId = info.User.Id;
            BusinessUnitId = info.BusinessUnit.Id;
        }
    }
}
