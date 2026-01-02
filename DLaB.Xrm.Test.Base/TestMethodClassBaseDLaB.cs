using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using DLaB.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
#if DLAB_UNROOT_COMMON_NAMESPACE
using DLaB.Common;
#else
using Source.DLaB.Common;
#endif
using DLaB.Xrm.CrmSdk;

#if NET
using DataverseUnitTest.Assumptions;
using DataverseUnitTest.Builders;
using DLaB.Xrm;

// ReSharper disable once CheckNamespace
namespace DataverseUnitTest
#else
using DLaB.Xrm.Test.Assumptions;
using DLaB.Xrm.Test.Builders;

// ReSharper disable once CheckNamespace
namespace DLaB.Xrm.Test
#endif
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// A Default Unit Test Class that handles Auto-wiring up creation of the IOrganizationService, and creation/deletion of data, and assumptions
    /// </summary>
    public abstract class TestMethodClassBaseDLaB
    {
        /// <summary>
        /// Sets whether tracing of the FakeIOrganizationService is enabled
        /// </summary>
        public bool EnableServiceExecutionTracing { get; set; }

        /// <summary>
        /// Enables multi threaded deletion.
        /// Should only need to be turned off if an entity type that is being deleted, has a reference to itself that requires it
        /// to be deleted in a particular order
        /// </summary>
        public bool MultiThreadPostDeletion { get; set; }

        private static int InstanceCounter;
        private readonly int _instanceId;
        /// <summary>
        /// A unique instance id for this instance of the class
        /// </summary>
        public long UniqueId => _instanceId;

        /// <summary>
        /// Gets or sets the assert CRM.
        /// </summary>
        /// <value>
        /// The assert CRM.
        /// </value>
        protected AssertCrm AssertCrm { get; set; } = null!;

        private string _localServiceOrgName;
        /// <summary>
        /// The Org Name used when creating the GetLocalCrmDatabaseOrganizationService
        /// </summary>
        protected virtual string LocalServiceOrgName { get => _localServiceOrgName; set => _localServiceOrgName = value; }

        /// <summary>
        /// The Org Name used when creating the GetLocalCrmDatabaseOrganizationService
        /// </summary>
        protected virtual Guid LocalServiceImpersonationId { get; set; }

        private LogRecorder? _recorder;
        /// <summary>Gets or sets the logger.</summary>
        /// <value>The logger.</value>
        protected ITestLogger Logger { get => _recorder!; set => _recorder = new LogRecorder(value); }

        /// <summary>
        /// List of Logs that have been recorded.  This includes both internal TestMethodClassBase Logs, and any logs logged with the Logger
        /// </summary>
        protected IEnumerable<TraceParams> Logs => _recorder?.Logs ?? [];


        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodClassBaseDLaB"/> class.
        /// </summary>
        protected TestMethodClassBaseDLaB()
        {
            _instanceId = Interlocked.Increment(ref InstanceCounter);
            _localServiceOrgName = GetType().FullName + " Instance: " + _instanceId;
            EnableServiceExecutionTracing = true;
            MultiThreadPostDeletion = true;
        }

        #region Entity Ids

        /// <summary>
        /// By default, attempts to load entities from internal type with static Id properties and/or an Ids property.
        /// Override GetEntityIdsByLogicalName if you want to customize how Entity Ids are discovered.
        /// </summary>
        protected virtual void InitializeEntityIds()
        {
            // Find all nested Ids By Class
            var idsByLogicalName = GetEntityIdsByLogicalName();

            // Add the nested Ids' Logical Names to the Mapper
            foreach (var key in idsByLogicalName.Keys)
            {
                EntityDependency.Mapper.Add(key);
            }

            // Add the nested Ids in the Deletion Order of the Mapper
            foreach (var entity in EntityDependency.Mapper.EntityDeletionOrder)
            {
                if (idsByLogicalName.TryGetValue(entity, out var ids))
                {
                    EntityIdsByLogicalName.AddOrAppend(entity, ids.ToArray());
                }
            }
        }

        /// <summary>
        /// Retrieves a dictionary mapping entity logical names to their associated identifier lists.
        /// </summary>
        /// <remarks>This method collects identifiers from both nested types and the "Ids" property, if
        /// present, within the current type. Override this method to customize how entity identifiers are discovered or
        /// aggregated in derived classes.</remarks>
        /// <returns>A dictionary where each key is an entity logical name and the corresponding value is a list of identifiers
        /// for that entity. If no identifiers are found, the dictionary will be empty.</returns>
        protected virtual Dictionary<string, List<Id>> GetEntityIdsByLogicalName()
        {
            var idsByLogicalName = new Dictionary<string, List<Id>>();
            foreach (var id in GetType().GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(Extensions.GetIds)) {
                idsByLogicalName.AddOrAppend(id, id);
            }

            var idProperty = GetType().GetProperty("Ids", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (idProperty != null)
            {
                foreach(var id in Id.GetPropertyIds(idProperty.GetValue(this)))
                {
                    idsByLogicalName.AddOrAppend(id, id);
                }
            }

            return idsByLogicalName;
        }

        /// <summary>
        /// Populated with Entities that are loaded as a result of using EntityDataAssumption Attributes on the Test Method Class.
        /// Loaded before GetIOrganizationService() and InitializeTestData(service)
        /// </summary>
        protected AssumedEntities AssumedEntities { get; set; } = null!;

        private Dictionary<string, List<Id>> _entityIdsByLogicalName = new();
        /// <summary>
        /// Populated with EntityLogical name keys, and list of ids.  Default Implementation of Cleanup methods will delete
        /// all entities in the dictionary, GetEntityReference will use it to populate Entities
        /// </summary>
        protected Dictionary<string, List<Id>> EntityIdsByLogicalName
        {
            get => _entityIdsByLogicalName;
            set => _entityIdsByLogicalName = value ?? throw new ArgumentNullException(nameof(EntityIdsByLogicalName) + " must not be null!");
        }

        /// <summary>
        /// Will get populated at the very beginning of Test
        /// </summary>
        protected Dictionary<Guid, Id> EntityIds { get; private set; } = null!;

        private void PopulateEntityReferences()
        {
            EntityIds = new Dictionary<Guid, Id>();
            foreach (var entry in EntityIdsByLogicalName)
            {
                foreach (var id in entry.Value)
                {
                    try
                    {
                        EntityIds.Add(id, id);
                    }
                    catch (ArgumentException ex)
                    {
                        if (ex.Message != "An item with the same key has already been added.") { throw; }

                        if (EntityIds[id].LogicalName == entry.Key)
                        {
                            throw new ArgumentException($"Id {id} is defined twice for entity type {entry.Key}");
                        }
                        throw new ArgumentException($"An attempt was made to define Id {id} as type {entry.Key}, but it has already been defined as a {EntityIds[id].LogicalName}");
                    }
                }
            }
        }

        #endregion Entity Ids

        #region Data Cleanup

        /// <summary>
        /// Cleanups the test data.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="shouldExist">if set to <c>true</c> [should exist].</param>
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
            foreach (var entityType in EntityIdsByLogicalName)
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
                    WhereIn(GetIdAttributeName(entityType.Key), entityType.Value.Select(i => i.EntityId));

                foreach (var entity in service.RetrieveMultiple(qe).Entities)
                {
                    service.TryDelete(entityType.Key, entity.Id);
                }
            }
        }

        private string GetIdAttributeName(string logicalName)
        {
            var type = EntityHelper.GetType(TestSettings.EarlyBound.Assembly, TestSettings.EarlyBound.Namespace, logicalName);
            return EntityHelper.GetIdAttributeName(type);
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

            foreach (var id in EntityIdsByLogicalName.Where(e => e.Key != "businessunit").SelectMany(entityType => entityType.Value))
            {
                requests.Add(new DeleteRequest { Target = EntityIds[id] });
            }

            if (requests.Any())
            {
                var response = (ExecuteMultipleResponse)service.Execute(
                    new ExecuteMultipleRequest
                    {
                        Settings = new ExecuteMultipleSettings
                        {
                            ContinueOnError = true,
                            ReturnResponses = false
                        },
                        Requests = requests,
                    });

                ThrowExceptionForFaults(response, requests);

                totalWatch.Stop();
                Logger.WriteLine("Total Time to delete {0} entities of types {1} (ms): {2}",
                    requests.Count,
                    requests.Select(s => ((DeleteRequest)s).Target.LogicalName).Distinct().ToCsv(),
                    totalWatch.ElapsedMilliseconds);
            }

            if (!EntityIdsByLogicalName.TryGetValue("businessunit", out var businessIds))
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
                if (localFault.ErrorCode == ErrorCodes.ObjectDoesNotExist)
                {
                    Logger.WriteLine("Attempted to Delete Entity that doesn't exist {0} ({1}) - {2}", target.LogicalName, target.Id, localFault.Message);
                    continue;
                }

                while (localFault.InnerFault != null)
                {
                    localFault = localFault.InnerFault;
                }

                var errorDetails = string.Empty;
                if (localFault.ErrorDetails.ContainsKey("CallStack"))
                {
                    errorDetails = Environment.NewLine + localFault.ErrorDetails["CallStack"];
                }
                exceptions.Add(new Exception(localFault.Message + errorDetails));

                Logger.WriteLine("Error Deleting {0} ({1}) - {2}{3}", target.LogicalName, target.Id, localFault.Message, errorDetails);
            }

            if (exceptions.Any())
            {
                throw new AggregateException("One or more faults occured", exceptions);
            }
        }

        #endregion Data Cleanup

        private static bool IsLoaded { get; set; }
        private static readonly object IsLoadedLock = new object();
        private static void LoadConfigurationSettingsOnce(TestMethodClassBaseDLaB value)
        {
            if (IsLoaded) return;
            lock (IsLoadedLock)
            {
                if (IsLoaded) return;

                value.LoadConfigurationSettings();
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Loads the configuration settings.
        /// </summary>
        protected abstract void LoadConfigurationSettings();

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
            return ("Unit Test - " + methodClassName.SpaceOutCamelCase().Replace("_ ", " ").Replace("_", " ")).PadRight(maxNameLength).Substring(0, maxNameLength).TrimEnd();
        }

        /// <summary>
        /// Method to populate the data in the CRM Database to setup a clean test
        /// </summary>
        /// <param name="service"></param>
        protected virtual void InitializeTestData(IOrganizationService service)
        {

        }

        /// <summary>
        /// Gets the organization service builder that will be used to 
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        protected virtual IAgnosticServiceBuilder GetOrganizationServiceBuilder(IOrganizationService service)
        {
            return new OrganizationServiceBuilder(service);
        }

        /// <summary>
        /// Gets either the Local Crm Organization Service, or the real connection to CRM, depending on the UnitTestSettings.user.config settings.
        /// </summary>
        /// <returns></returns>
        protected virtual IOrganizationService GetIOrganizationService()
        {
            var service = GetOrganizationServiceBuilder(GetInternalOrganizationServiceProxy()).
                WithEntityNameDefaulted((_, i) => GetUnitTestName(i.MaximumLength)).
                AssertIdNonEmptyOnCreate().
                WithDefaultParentBu().Build();

            return service;
        }

        private IClientSideOrganizationService? Service { get; set; }
        private IClientSideOrganizationService GetInternalOrganizationServiceProxy()
        {
            return Service ??= (IClientSideOrganizationService) new OrganizationServiceBuilder(new FakeIOrganizationService(TestBase.GetOrganizationService(LocalServiceOrgName, LocalServiceImpersonationId), Logger)).WithBusinessUnitDeleteAsDeactivate().Build();
        }

        /// <summary>
        /// Executes the Unit Test
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Test(ITestLogger logger)
        {
            Logger = logger;
            var timer = new TestActionTimer(logger);
            LoadConfigurationSettingsOnce(this);
            InitializeEntityIds();
            if (EntityIdsByLogicalName.Count > 0)
            {
                timer.Time(PopulateEntityReferences, "Initialization Entity Reference (ms): ");
            }

            using var internalService = GetInternalOrganizationServiceProxy();
            // ReSharper disable once AccessToDisposedClosure
            timer.Time(() => CleanupTestData(internalService, false), "Cleanup PreTestData (ms): ");

            try
            {
                // ReSharper disable once AccessToDisposedClosure
                timer.Time(() => AssumedEntities = AssumedEntities.Load(internalService, GetType()), "Load Assumptions (ms): ");

                var service = GetIOrganizationService();
                AssertCrm = new AssertCrm(service);

                timer.Time(() => InitializeTestData(service), "Initialize TestData (ms): ");
                timer.Time(() => Test(service), "Run Test (ms): ");
            }
            finally
            {
                // ReSharper disable once AccessToDisposedClosure
                timer.Time(() => CleanupTestData(internalService, true), "Cleanup PostTestData (ms): ");
            }
        }
    }
}
