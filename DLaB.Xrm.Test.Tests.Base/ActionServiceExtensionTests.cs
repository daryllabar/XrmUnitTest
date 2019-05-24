using System;
using System.Collections.Generic;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Tests
{
    [TestClass]
    public class ActionServiceExtensionTests
    {
        // Test GetExecutedQueryExpressions
        [TestMethod]
        public void ExtensionActionService_GetExecutedQueryExpression_ForAMethodThatPerformsAQuery_Should_ReturnQuery()
        {
            var qe = QueryExpressionFactory.Create<Contact>(Contact.Fields.FirstName, "John");
            Action<IOrganizationService> action = s => { GetJohns(s); };
            var qes = action.GetExecutedQueryExpressions();
            Assert.AreEqual(1, qes.Count);
            Assert.IsTrue(qes[0].IsEqual(qe));
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static List<Contact> GetJohns(IOrganizationService service)
        {
            return service.GetEntities<Contact>(Contact.Fields.FirstName, "John");
        }
    }
}
