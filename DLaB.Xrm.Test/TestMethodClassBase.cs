using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DLaB.Common;
using DLaB.Xrm.Client;
using DLaB.Xrm.Test.Builders;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.Test
{
    public abstract class TestMethodClassBase
    {
        public struct CrmErrorCodes
        {
            public const int EntityToDeleteDoesNotExist = -2147220969;
        }

        /// <summary>
        /// Enables tracing in the FakeIOrganizationService 
        /// </summary>
        public bool EnableServiceExecutionTracing { get; set; }

        /// <summary>
        /// Enables multi threaded deletion.
        /// Should only need to be turned off if an entity type that is being deleted, has a reference to itself that requires it
        /// to be deleted in a particular order
        /// </summary>
        public bool MultiThreadPostDeletion { get; set; }

        protected AssertCrm AssertCrm { get; set; }

        protected TestMethodClassBase()
        {
            AssumedEntities = new Assumptions.AssumedEntities();
            EntityIds = new Dictionary<string, List<Guid>>();
            EnableServiceExecutionTracing = true;
            MultiThreadPostDeletion = true;
            NewEntityDefaultIds = new List<Id>();
        }

        #region Entity Ids

        /// <summary>
        /// By default attempts to load entities from internal type with static Id properties
        /// </summary>
        protected virtual void InitializeEntityIds()
        {
            // Find all nested Ids
            var nestedIds = new Dictionary<string, List<Guid>>();
            foreach (var id in GetType().GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetIds)) {
                nestedIds.AddOrAppend(id);
            }

            // Add the nested Ids' Logical Names to the Mapper
            foreach (var key in nestedIds.Keys)
            {
                EntityDependency.Mapper.Add(key);
            }

            // Add the nested Ids in the Deletion Order of the Mapper
            foreach (var entity in EntityDependency.Mapper.EntityDeletionOrder)
            {
                List<Guid> ids;
                if (nestedIds.TryGetValue(entity, out ids))
                {
                    EntityIds.AddOrAppend(entity, ids.ToArray());
                }
            }
        }

        private IEnumerable<Id> GetIds(Type type)
        {
            foreach (var field in type.GetFields().Where(field => field.FieldType == typeof (Id))) {
                yield return GetValue(field);
            }
            foreach (var id in type.GetNestedTypes().SelectMany(GetIds)) {
                yield return id;
            }
        }

        /// <summary>
        /// Gets the Id value of the field.  It is very easy to declare an Id&lt;Entity&gt;, but this isn't valid
        /// This will make the error much more readable and understandable
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        [DebuggerHidden]
        private static Id GetValue(FieldInfo field)
        {
            try
            {
                return (Id) field.GetValue(null);
            }catch(TargetInvocationException ex)
            {
                if (ex.InnerException != null && ex.InnerException.InnerException != null)
                {
                    var idEx = ex.InnerException.InnerException;
                    if (idEx.Message.Contains("\"Entity\" is not a valid entityname"))
                    {
                        throw idEx;
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Populated with Entities that are loaded as a result of using EntityDataAssumption Attributes on the Test Method Class.
        /// </summary>
        protected Assumptions.AssumedEntities AssumedEntities { get; set; }

        /// <summary>
        /// Populate with EntityLogical name keys, and list of ids.  Default Implementation of Cleanup methods will delete
        /// all entities in the dictionary, GetEntityReference will use it to populate Entities
        /// </summary>
        protected Dictionary<string, List<Guid>> EntityIds { get; set; }

        /// <summary>
        /// Will get populated at the very beginning of Test
        /// </summary>
        protected Dictionary<Guid, Id> Entities { get; private set; }

        /// <summary>
        /// Populate with EntityLogical name keys, and queue of ids.  Will be used to default the ids of entities
        /// that are being created by the unit test, without an already populated id
        /// </summary>
        protected List<Id> NewEntityDefaultIds { get; private set; }

        private void PopulateEntityReferences()
        {
            Entities = new Dictionary<Guid, Id>();
            foreach (var entry in EntityIds)
            {
                foreach (var id in entry.Value)
                {
                    try
                    {
                        Entities.Add(id, new Id(entry.Key, id));
                    }
                    catch (ArgumentException ex)
                    {
                        if (ex.Message != "An item with the same key has already been added.") { throw; }

                        if (Entities[id].LogicalName == entry.Key)
                        {
                            throw new ArgumentException(String.Format("Id {0} is defined twice for entity type {1}", id, entry.Key));
                        }
                        throw new ArgumentException(String.Format("An attempt was made to define Id {0} as type {1}, but it has already been defined as a {2}", id, entry.Key, Entities[id].LogicalName));
                    }
                }
            }
        }

        #endregion // Entity Ids

        #region Data Cleanup

        protected virtual void CleanupTestData(IOrganizationService service, bool shouldExist)
        {
            if (shouldExist)
            {
                CleanupDataPostInitialization(service);
            }
            else
            {
                CleanupDataPreInitialization(service);
            }
        }

        /// <summary>
        /// Data is assumed to not be there, so use DeleteIfExists
        /// </summary>
        /// <param name="service"></param>
        protected virtual void CleanupDataPreInitialization(IOrganizationService service)
        {
            foreach (var entityType in EntityIds)
            {
                var entityIdsToDelete = entityType.Value;

                if (!entityIdsToDelete.Any())
                {
                    continue;
                }

                var qe = QueryExpressionFactory.Create(
                    new LateBoundQuerySettings(entityType.Key)
                    {
                        ActiveOnly = false,
                        Columns = new ColumnSet(false),
                    }).
                    WhereIn(EntityHelper.GetIdAttributeName(entityType.Key), entityType.Value);

                foreach (var entity in service.RetrieveMultiple(qe).Entities)
                {
                    service.TryDelete(entityType.Key, entity.Id);
                }
            }
        }

        /// <summary>
        /// Data is assumed to be there, so use TryDelete
        /// </summary>
        /// <param name="service"></param>
        protected virtual void CleanupDataPostInitialization(IOrganizationService service)
        {
            var totalWatch = new Stopwatch();
            totalWatch.Start();

            var requests = new OrganizationRequestCollection();

            foreach (var id in EntityIds.Where(e => e.Key != "businessunit").SelectMany(entityType => entityType.Value))
            {
                requests.Add(new DeleteRequest { Target = Entities[id] });
            }

            if (requests.Any())
            {
                var response = (ExecuteMultipleResponse)service.Execute(
                    new ExecuteMultipleRequest()
                    {
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = true,
                            ReturnResponses = false
                        },
                        Requests = requests,
                    });

                ThrowExceptionForFaults(response, requests);

                totalWatch.Stop();
                Debug.WriteLine("Total Time to delete {0} entities of types {1} (ms): {2}",
                    requests.Count,
                    String.Join(", ", requests.Select(s => ((DeleteRequest)s).Target.LogicalName).Distinct()),
                    totalWatch.ElapsedMilliseconds);
            }

            List<Guid> businessIds;
            if (!EntityIds.TryGetValue("businessunit", out businessIds))
            {
                return;
            }

            foreach (var id in businessIds)
            {
                service.DeleteBusinessUnit(id);
            }
        }

        private void ThrowExceptionForFaults(ExecuteMultipleResponse response, OrganizationRequestCollection requests)
        {
            if (!response.IsFaulted) { return; }

            var exceptions = new List<Exception>();
            foreach (var fault in response.Responses.Select(r => new { r.Fault, r.RequestIndex }))
            {
                var localFault = fault.Fault;
                var target = ((DeleteRequest)requests[fault.RequestIndex]).Target;
                if (localFault.ErrorCode == CrmErrorCodes.EntityToDeleteDoesNotExist)
                {
                    Debug.WriteLine("Attempted to Delete Entity that doesn't exist {0} ({1}) - {2}", target.LogicalName, target.Id, localFault.Message);
                    continue;
                }

                while (localFault.InnerFault != null)
                {
                    localFault = localFault.InnerFault;
                }

                var errorDetails = String.Empty;
                if (localFault.ErrorDetails.ContainsKey("CallStack"))
                {
                    errorDetails = Environment.NewLine + localFault.ErrorDetails["CallStack"];
                }
                exceptions.Add(new Exception(localFault.Message + errorDetails));

                Debug.WriteLine("Error Deleting {0} ({1}) - {2}{3}", target.LogicalName, target.Id, localFault.Message, errorDetails);
            }

            if (exceptions.Any())
            {
                throw new AggregateException("One or more faults occured", exceptions);
            }
        }

        #endregion // Data Cleanup

        #region Assumptions

        protected void ValidateAssumptions(IOrganizationService service)
        {
            foreach (var entityAssumption in GetType().GetCustomAttributes(true).
                                                       Select(a => a as Assumptions.EntityDataAssumptionBaseAttribute).
                                                       Where(a => a != null))
            {
                entityAssumption.AddAssumedEntities(service, AssumedEntities);
            }
        }

        #endregion // Assumptions

        /// <summary>
        /// Method to populate the data in the CRM Database to setup a clean test
        /// </summary>
        /// <param name="service"></param>
        protected abstract void InitializeTestData(IOrganizationService service);

        /// <summary>
        /// The actual test to run, where the required data has already been initialized
        /// </summary>
        /// <param name="service"></param>
        protected abstract void Test(IOrganizationService service);

        /// <summary>
        /// Used by the default implementation of GetIOrganizationService to insert a unit test name for any entity
        /// created by the MockIOrganization
        /// </summary>
        /// <returns></returns>
        internal virtual string GetUnitTestName(int maxNameLength)
        {
            var methodClassName = GetType().Name;
            if (methodClassName.ToLower().EndsWith("method"))
            {
                methodClassName = methodClassName.Substring(0, methodClassName.Length - "method".Length);
            }
            return ("Unit Test - " + methodClassName.SpaceOutCamelCase()).PadRight(maxNameLength).Substring(0, maxNameLength).TrimEnd();
        }

        protected virtual IOrganizationService GetIOrganizationService()
        {
            return new OrganizationServiceBuilder(GetInternalOrganizationServiceProxy()).
                WithEntityNameDefaulted((e, i) => GetUnitTestName(i.MaximumLength)).
                AssertIdNonEmptyOnCreate().
                WithIdsDefaultedForCreate(NewEntityDefaultIds.ToArray()).
                WithDefaultParentBu().Build();
        }

        private IClientSideOrganizationService Service { get; set; }
        private IClientSideOrganizationService GetInternalOrganizationServiceProxy()
        {
            return Service ?? (Service = (IClientSideOrganizationService)new OrganizationServiceBuilder(new FakeIOrganizationService(TestBase.GetOrganizationService())
            {
                EnableExecutionTracing = EnableServiceExecutionTracing
            }).WithBusinessUnitDeleteAsDeactivate().Build());
        }

        public void Test()
        {
            InitializeEntityIds();
            if (EntityIds != null && EntityIds.Count > 0)
            {
                DebugLog.Time(PopulateEntityReferences, "Initialization Entity Reference (ms): ");
            }
            using (var internalService = GetInternalOrganizationServiceProxy())
            {
                // ReSharper disable once AccessToDisposedClosure
                DebugLog.Time(() => CleanupTestData(internalService, false), "Cleanup PreTestData (ms): ");

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    DebugLog.Time(() => ValidateAssumptions(internalService), "Validate Assumptions (ms): ");

                    var service = GetIOrganizationService();
                    AssertCrm = new AssertCrm(service);

                    DebugLog.Time(() => InitializeTestData(service), "Initialize TestData (ms): ");
                    DebugLog.Time(() => Test(service), "Run Test (ms): ");
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    DebugLog.Time(() => CleanupTestData(internalService, true), "Cleanup PostTestData (ms): ");
                }
            }
        }
    }
}
