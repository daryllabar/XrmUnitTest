using System;
using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class ConditionExpressionTests : BaseTestClass
    {

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LikeIsCaseInsensitive()
        {
            var service = GetService();
            service.Create(new Contact { FirstName = "Jimmy" });
            Assert.IsNotNull(service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.Like, "JIM%")));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_LastXDays()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d)
            });

            Func<int, Entity> getContact = (days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.LastXDays, days));

            Assert.IsNull(getContact(1));
            Assert.IsNotNull(getContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(2d) });
            Assert.IsNull(getContact(3));
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Last7Days()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(-6d)
            });

            Func<Entity> getContact = () => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Last7Days));

            Assert.IsNotNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-8d) });
            Assert.IsNull(getContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Next7Days()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(6d)
            });

            Func<Entity> getContact = () => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Next7Days));

            Assert.IsNotNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(8d) });
            Assert.IsNull(getContact());
        }


        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Today()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Func<Entity> getContact = () => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Today));

            Assert.IsNotNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(1d) });
            Assert.IsNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-1d) });
            Assert.IsNull(getContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Tomorrow()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Func<Entity> getContact = () => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Tomorrow));

            Assert.IsNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(1d) });
            Assert.IsNotNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(2d) });
            Assert.IsNull(getContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_Yesterday()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow
            });

            Func<Entity> getContact = () => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.Yesterday));

            Assert.IsNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-1d) });
            Assert.IsNotNull(getContact());
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d) });
            Assert.IsNull(getContact());
        }

        [TestMethod]
        public void LocalCrmTests_ConditionExpression_NextXDays()
        {
            var service = GetService();
            var id = service.Create(new Contact
            {
                LastUsedInCampaign = DateTime.UtcNow.AddDays(2d)
            });

            Func<int, Entity> getContact = (days) => service.GetFirstOrDefault<Contact>(new ConditionExpression(Contact.Fields.LastUsedInCampaign, ConditionOperator.NextXDays, days));

            Assert.IsNull(getContact(1));
            Assert.IsNotNull(getContact(2));
            Assert.IsNotNull(getContact(3));
            service.Update(new Contact { Id = id, LastUsedInCampaign = DateTime.UtcNow.AddDays(-2d) });
            Assert.IsNull(getContact(3));
        }
    }
}
