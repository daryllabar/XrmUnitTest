using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else

namespace DLaB.Xrm.Test.Builders
#endif
{
    internal static class ReadOnlyFail
    {
        internal static Dictionary<string, Action<OrganizationRequest>> FailsByRequestName = new Dictionary<string, Action<OrganizationRequest>> {
            { new CreateRequest().RequestName, r => OnCreate(new CreateRequest {Parameters = r.Parameters}.Target) },
            { new DeleteRequest().RequestName, r => OnExecute(new DeleteRequest {Parameters = r.Parameters}) },
            { new UpdateRequest().RequestName, r => OnUpdate(new UpdateRequest {Parameters = r.Parameters}.Target) },
            { new AssociateRequest().RequestName, r => OnExecute(new AssociateRequest {Parameters = r.Parameters}) },
            { new DisassociateRequest().RequestName, r => OnExecute(new DisassociateRequest {Parameters = r.Parameters}) },
        };

        internal static List<string> ValidStartsWithRequestNames = new List<string>{
            "CanBe",
            "CanManyToMany",
            "Download",
            "Execute",
            "Export",
            "FetchXmlToQueryExpression",
            "FindParentResourceGroup",
            "Get",
            "Is",
            "LocalTimeFromUtcTime",
            "Query",
            "Retrieve",
            "Search",
            "UtcTimeFromLocalTime",
            "WhoAmI"
        };

        internal static void AssertValid(OrganizationRequest request)
        {
            if (FailsByRequestName.TryGetValue(request.RequestName, out var fail))
            {
                fail(request);
            }

            if (!ValidStartsWithRequestNames.Any(n => request.RequestName.StartsWith(n)))
            {
                TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Execute Request {request.RequestName} with a ReadOnly Service");
            }

            AssertValidMultipleRequest(request);
        }

        internal static void OnCreate(Entity e)
        {
            TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Create a(n) {e?.LogicalName} Entity with a ReadOnly Service{Environment.NewLine + Environment.NewLine}Entity Attributes:{e?.ToStringAttributes()}");
        }

        private static void OnExecute(DeleteRequest request)
        {
            OnDelete(request.Target?.LogicalName, request.Target?.Id ?? Guid.Empty);
        }
        internal static void OnDelete(string? name, Guid id)
        {
            TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Delete a(n) {name ?? "NULL"} Entity with id {id}, using a ReadOnly Service");
        }

        internal static void OnUpdate(Entity e)
        {
            TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Update a(n) {e.LogicalName} Entity with id {e.Id}, using a ReadOnly Service");
        }

        private static void OnExecute(AssociateRequest request)
        {
            OnAssociate(request.Target?.LogicalName ?? "NULL", request.Target?.Id ?? Guid.Empty, request.Relationship);
        }
        public static void OnAssociate(string entity, Guid id, Relationship relationship)
        {
            TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Associate Entities to Entity {entity} ({id}) for relationship {relationship?.SchemaName} with a ReadOnly Service");
        }

        private static void OnExecute(DisassociateRequest request)
        {
            OnDisassociate(request.Target?.LogicalName ?? "NULL", request.Target?.Id ?? Guid.Empty, request.Relationship);
        }
        public static void OnDisassociate(string entity, Guid id, Relationship relationship)
        {
            TestSettings.TestFrameworkProvider.AssertFail($"An attempt was made to Disassociate Entities to Entity {entity} ({id}) for relationship {relationship?.SchemaName} with a ReadOnly Service");
        }

        public static void OnExecute(OrganizationRequest request)
        {
            AssertValid(request);
        }

        private static void AssertValidMultipleRequest(OrganizationRequest request)
        {
            OrganizationRequestCollection? requests = null;
            switch (request)
            {
                case ExecuteMultipleRequest mr:
                    requests = mr.Requests;
                    break;
#if !PRE_KEYATTRIBUTE
                case ExecuteTransactionRequest tr:
                    requests = tr.Requests;
                    break;
#endif
            }
            if (requests != null)
            {
                foreach (var nestedRequest in requests)
                {
                    AssertValid(nestedRequest);
                }
            }
        }
    }
}
