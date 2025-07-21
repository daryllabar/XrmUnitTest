using DLaB.Xrm;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Ioc;
using DLaB.Xrm.LocalCrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XrmUnitTest.Test.Builders;

#if NET
using DataverseUnitTest;
using DataverseUnitTest.Assumptions;
#else
using DLaB.Xrm.Test;
using DLaB.Xrm.Test.Assumptions;
#endif


namespace XrmUnitTest.Test
{

    public abstract class LocalTestBase
    {
        /// <summary>
        /// Contains all Id instances in the Ids struct of the class.  Useful for being able to enumerate through all the Ids.
        /// </summary>
        protected IEnumerable<Id> IdsList => IdsById.Values;
        private Dictionary<Guid, Id> IdsById { get; set; }

        protected AssumedEntities AssumedEntities { get; set; }

        protected IIocContainer Container { get; private set; }

        /// <summary>
        /// Exposes an CrmEnvironmentBuilder that can be used to reflectively associate records without having to specify the join, as well as create the records in the correct order, even handling circular references.
        /// </summary>
        protected CrmEnvironmentBuilder EnvBuilder { get; private set; }

        /// <summary>
        /// The Business Unit
        /// </summary>
        protected Id<BusinessUnit> CurrentBusinessUnit { get; private set; }

        /// <summary>
        /// The Userid of the current Service
        /// </summary>
        protected Id<SystemUser> CurrentUser { get; private set; }

        private DateTime? _utcNow;
        /// <summary>
        /// A UtcNow value that doesn't change throughout the test.
        /// </summary>
        protected DateTime Now => _utcNow ?? (_utcNow = DateTime.UtcNow).Value;
        /// <summary>
        /// A UtcNow without milliseconds (since CRM trims Milliseconds) value that doesn't change throughout the test.
        /// </summary>
        protected DateTime NowSansMilliseconds => Now.RemoveMilliseconds();

        /// <summary>
        /// Test specific (isolated) organization service.
        /// </summary>
        protected IOrganizationService Service { get; set; }

        public TestContext TestContext { get; set; }

        private DateTime? _today;
        /// <summary>
        /// The local current date for the user at midnight
        /// </summary>
        protected virtual DateTime Today => _today ?? (_today = DateTime.Today).Value;
        /// <summary>
        /// Tomorrow's date for the user at midnight
        /// </summary>
        protected DateTime Tomorrow => Today.AddDays(1);
        /// <summary>
        /// Fake Tracing Service that can be used to assert that the correct messages were logged.
        /// </summary>
        protected FakeTraceService TracingService { get; set; }
        #if !(XRM_2013 || XRM_2015 || XRM_2016)
        /// <summary>
        /// Fake Managed Identity Service that can be used to assert token acquisition in tests.
        /// </summary>
        protected FakeManagedIdentityService ManagedIdentityService { get; set; }
        #endif
        /// <summary>
        /// Yesterday's date for the user at midnight
        /// </summary>
        protected DateTime Yesterday => Today.AddDays(-1);

        /// <summary>
        /// Data Driven tests will have the same test name, so to ensure separate LocalCrmDatabases, a unique id is appended to the test name.
        /// </summary>
        private readonly string _testId = "_" + Guid.NewGuid();
        private string TestId => TestContext.TestName + _testId;

        private readonly DebugLogger _logger = new DebugLogger();

        [TestInitialize]
        public void PreTest()
        {
            _logger.Enabled = false;
            IdsById = IdsById ?? GetType().GetTypeInfo().DeclaredNestedTypes.SelectMany(types => types.GetIds()).ToDictionary(k => k.EntityId, v => v);
            TestInitializer.InitializeTestSettings();
            TestBase.LoadUserUnitTestSettings();
            Service = CreateService();
            CurrentBusinessUnit = new Id<BusinessUnit>(Service.GetFirst<BusinessUnit>().Id);
            CurrentUser = new Id<SystemUser>(Service.GetCurrentlyExecutingUserInfo().UserId);
            TracingService = new FakeTraceService(_logger);
            #if !(XRM_2013 || XRM_2015 || XRM_2016)
            ManagedIdentityService = new FakeManagedIdentityService();
            #endif
            EnvBuilder = new CrmEnvironmentBuilder();
            PreInitialize();
            Initialize();
            _logger.Enabled = true;
        }

        private void PreInitialize()
        {
            AssumedEntities = AssumedEntities.Load(Service, GetType());
            Container = RegisterLocalTestServices(new IocContainer());
            Container.AddSingleton(Service);
        }

        protected virtual IIocContainer RegisterLocalTestServices(IIocContainer container)
        {
            return container;
        }

        [TestCleanup]
        public void PreTestCleanup()
        {
            // Ids are generally instantiated at the class level, so they will need to be cleared out after each test
            foreach (var kvp in IdsById)
            {
                // Remove all attributes.  Future test initializer will repopulate
                kvp.Value.Entity.Attributes.Clear();

                // Reset the Id to repopulate the attribute
                kvp.Value.Entity.Id = kvp.Key;
            }

            TestCleanup();
        }

        /// <summary>
        /// Override to clean up any test specific data
        /// </summary>
        protected virtual void TestCleanup()
        {
        }

        protected virtual IOrganizationService CreateService()
        {
            return CreateLocalService(TestId, _logger);
        }

        /// <summary>
        /// Override to initialize any test specific data
        /// </summary>
        protected virtual void Initialize()
        {

        }

        /// <summary>
        /// Ensures that the exception of the given type was thrown (optionally with the given message) when the given action is performed.
        /// Use ExpectPluginException if the TException is an InvalidPluginExecutionException
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="action"></param>
        /// <param name="message"></param>
        protected void ExpectException<TException>(Action action, string message = null) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail(string.IsNullOrEmpty(message)
                    ? $"Expected exception of type {typeof(TException).Name} to be thrown, but no exception was thrown!"
                    : $"Expected exception of type {typeof(TException).Name} to be thrown with message \"{message}\", but no exception was thrown!");
            }
            catch (TException ex)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    if (message != ex.Message)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that the given action throws an InvalidPluginExecutionException (optionally with the given message) when executed.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        protected void ExpectPluginException(Action action, string message = null)
        {
            ExpectException<InvalidPluginExecutionException>(action, message);
        }

        public static IOrganizationService CreateLocalService<T>(string test, ITestLogger logger = null) where T : class
        {
            return CreateLocalService(typeof(T).FullName + "|" + test, logger);
        }

        public static IOrganizationService CreateLocalService(string testId, ITestLogger logger = null)
        {
            return new FakeIOrganizationService(new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<CrmContext>(testId)), logger ?? new DebugLogger());
        }
    }
}
