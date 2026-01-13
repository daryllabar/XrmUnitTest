using DLaB.Xrm.Client;
using DLaB.Xrm.LocalCrm.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace DLaB.Xrm.LocalCrm
{
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
                throw new ArgumentNullException(nameof(request.PrincipalAccess.Principal), "Principal must be specified in PrincipalAccess");
            }

            // Ensure the PrincipalObjectAccess entity type exists in the early bound assembly
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType(PrincipalObjectAccess.EntityLogicalName);
            if (entityType == null)
            {
                throw new Exception("PrincipalObjectAccess entity is not defined in the early bound assembly. Please ensure it is generated.");
            }

            // Create a PrincipalObjectAccess record
            var accessRecord = new Entity(PrincipalObjectAccess.EntityLogicalName)
            {
                [PrincipalObjectAccess.Fields.AccessRightsMask] = (int)request.PrincipalAccess.AccessMask,
                [PrincipalObjectAccess.Fields.ChangedOn] = Info.TimeProvider.GetUtcNow(),
                [PrincipalObjectAccess.Fields.ObjectId] = request.Target.Id,
                [PrincipalObjectAccess.Fields.ObjectTypeCode] = request.Target.LogicalName,
                [PrincipalObjectAccess.Fields.PrincipalId] = request.PrincipalAccess.Principal.Id,
                [PrincipalObjectAccess.Fields.PrincipalTypeCode] = request.PrincipalAccess.Principal.LogicalName,
            };

            var existing = this.GetFirstOrDefault(PrincipalObjectAccess.EntityLogicalName, 
                PrincipalObjectAccess.Fields.PrincipalId, request.PrincipalAccess.Principal.Id,
                PrincipalObjectAccess.Fields.ObjectId, request.Target.Id
                );

            var originalValue = EnforceValidForOperationCheck;
            EnforceValidForOperationCheck = false;
            try
            {
                if (existing == null)
                {
                    Create(accessRecord);
                }
                else
                {
                    accessRecord.Id = existing.Id;
                    Update(accessRecord);
                }
            }
            finally
            {
                EnforceValidForOperationCheck = originalValue;
            }

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

            // Ensure the PrincipalObjectAccess entity type exists in the early bound assembly
            var entityType = CrmServiceUtility.GetEarlyBoundProxyAssembly().GetEntityType(PrincipalObjectAccess.EntityLogicalName);
            if (entityType == null)
            {
                throw new Exception("PrincipalObjectAccess entity is not defined in the early bound assembly. Please ensure it is generated.");
            }

            // Query for existing PrincipalObjectAccess records
            var existing = this.GetFirstOrDefault(PrincipalObjectAccess.EntityLogicalName,
                PrincipalObjectAccess.Fields.PrincipalId, request.Revokee.Id,
                PrincipalObjectAccess.Fields.ObjectId, request.Target.Id
            );

            // Delete all matching records.  Dataverse does not error if none exists.
            if (existing != null)
            {
                var originalValue = EnforceValidForOperationCheck;
                EnforceValidForOperationCheck = false;
                try
                {
                    Delete(PrincipalObjectAccess.EntityLogicalName, existing.Id);
                }
                finally
                {
                    EnforceValidForOperationCheck = originalValue;
                }
            }

            return new RevokeAccessResponse();
        }
    }
}
