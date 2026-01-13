using DLaB.Xrm.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace DLaB.Xrm.LocalCrm
{
#if !DEBUG_XRM_UNIT_TEST_CODE
    [System.Diagnostics.DebuggerNonUserCode]
#endif
    partial class LocalCrmDatabaseOrganizationService
    {
        private GrantAccessResponse ExecuteInternal(GrantAccessRequest request)
        {
            if (request.Target == null)
            {
                throw new ArgumentNullException(nameof(request.Target), "Target must be specified for GrantAccessRequest");
            }

            if (request.PrincipalAccess == null)
            {
                throw new ArgumentNullException(nameof(request.PrincipalAccess), "PrincipalAccess must be specified for GrantAccessRequest");
            }

            if (request.PrincipalAccess.Principal == null)
            {
                throw new ArgumentNullException("PrincipalAccess.Principal", "Principal must be specified in PrincipalAccess");
            }

            // Ensure the PrincipalObjectAttributeAccess entity type exists in the early bound assembly
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType("principalobjectattributeaccess");
            if (entityType == null)
            {
                throw new Exception("PrincipalObjectAttributeAccess entity is not defined in the early bound assembly. Please ensure it is generated.");
            }

            // Create a PrincipalObjectAttributeAccess record
            var accessRecord = new Entity("principalobjectattributeaccess")
            {
                ["principalid"] = request.PrincipalAccess.Principal,
                ["objectid"] = request.Target,
                ["readaccess"] = (request.PrincipalAccess.AccessMask & AccessRights.ReadAccess) == AccessRights.ReadAccess,
                ["updateaccess"] = (request.PrincipalAccess.AccessMask & AccessRights.WriteAccess) == AccessRights.WriteAccess
            };

            Create(accessRecord);

            return new GrantAccessResponse();
        }

        private RevokeAccessResponse ExecuteInternal(RevokeAccessRequest request)
        {
            if (request.Target == null)
            {
                throw new ArgumentNullException(nameof(request.Target), "Target must be specified for RevokeAccessRequest");
            }

            if (request.Revokee == null)
            {
                throw new ArgumentNullException(nameof(request.Revokee), "Revokee must be specified for RevokeAccessRequest");
            }

            // Ensure the PrincipalObjectAttributeAccess entity type exists in the early bound assembly
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType("principalobjectattributeaccess");
            if (entityType == null)
            {
                throw new Exception("PrincipalObjectAttributeAccess entity is not defined in the early bound assembly. Please ensure it is generated.");
            }

            // Query for existing PrincipalObjectAttributeAccess records
            var query = new QueryExpression("principalobjectattributeaccess")
            {
                ColumnSet = new ColumnSet("principalobjectattributeaccessid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("principalid", ConditionOperator.Equal, request.Revokee.Id),
                        new ConditionExpression("objectid", ConditionOperator.Equal, request.Target.Id)
                    }
                }
            };

            var results = RetrieveMultiple(query);

            // Delete all matching records
            foreach (var record in results.Entities)
            {
                Delete("principalobjectattributeaccess", record.Id);
            }

            return new RevokeAccessResponse();
        }
    }
}
