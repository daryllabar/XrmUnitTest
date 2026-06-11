using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Diagnostics;
using System.ServiceModel;
using XrmUnitTest.Test;

namespace DLaB.Xrm.LocalCrm.Tests
{
    public class BaseTestClass
    {
        protected LocalCrmDatabaseOrganizationService Service { get; set; }

        [TestInitialize]
        public void InitializeBase()
        {
            TestInitializer.InitializeTestSettings();
            Service = GetService();
            Initialize();
        }

        protected virtual void Initialize()
        {

        }

        internal LocalCrmDatabaseOrganizationService GetService(bool createUnique = true, Guid? businessUnitId = null)
        {
            var info = createUnique
                ? LocalCrmDatabaseInfo.Create<CrmContext>(Guid.NewGuid().ToString(), userBusinessUnit: businessUnitId)
                : LocalCrmDatabaseInfo.Create<CrmContext>(userBusinessUnit: businessUnitId);
            return new LocalCrmDatabaseOrganizationService(info);
        }

        [DebuggerHidden]
        public static void AssertOrganizationServiceFaultException(string reasonForException, string exceptionMesageContains, Action action)
        {
            try
            {
                action();
                Assert.Fail(reasonForException);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (exceptionMesageContains == null)
                {
                    return;
                }
                Assert.Contains(exceptionMesageContains, ex.Message, "Exception type is different than expected");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception)
            {
                Assert.Fail("Exception type is different than expected");
            }
        }
    }
}
