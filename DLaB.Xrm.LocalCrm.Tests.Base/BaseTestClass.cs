using System;
using System.Collections.Generic;
using System.Text;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    public class BaseTestClass
    {
        internal static IOrganizationService GetService(bool createUnique = true, Guid? businessUnitId = null)
        {
            var info = createUnique
                ? LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString(), userBusinessUnit: businessUnitId)
                : LocalCrmDatabaseInfo.Create<CrmContext>(userBusinessUnit: businessUnitId);
            return new LocalCrmDatabaseOrganizationService(info);
        }
    }
}
