using System;
using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

namespace Example.Plugin.Advanced
{
    /// <summary>
    /// Plugin to remove all formatting of phone numbers.  
    /// </summary>
    public class RemovePhoneNumberFormatting : PluginBase
    {
        #region Constructors

        public RemovePhoneNumberFormatting() : this(null, null) { }
        public RemovePhoneNumberFormatting(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
        }

        #endregion Constructors

        protected override IRegisteredEventsPluginHandler GetPluginHandler()
        {
            return new RemovePhoneNumberFormattingLogic();
        }
    }

    internal class RemovePhoneNumberFormattingLogic : PluginHandlerBase
    {
        public override void RegisterEvents()
        {
            RegisteredEvents.AddRange(
                new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create, MessageType.Update).
                    ForEntities(Account.EntityLogicalName, Contact.EntityLogicalName, Lead.EntityLogicalName).
                    WithExecuteAction(ExecuteCrmPhoneNumber).Build());

            RegisteredEvents.AddRange(
                new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create, MessageType.Update).
                    ForEntities(BusinessUnit.EntityLogicalName, Competitor.EntityLogicalName, Site.EntityLogicalName, SystemUser.EntityLogicalName).
                    WithExecuteAction(ExecuteCrmAddresses).Build());
        }

        protected override void ExecuteInternal(ExtendedPluginContext context)
        {
            throw new InvalidOperationException("Should Never Get Called!");
        }

        private void ExecuteCrmPhoneNumber(ExtendedPluginContext context)
        {
            // Account is used here, but since all Entities have the same exact field names, this works just fine
            var target = context.GetTarget<Entity>();
            RemoveFormatting(target, Account.Fields.Telephone1);
            RemoveFormatting(target, Account.Fields.Telephone2);
            RemoveFormatting(target, Account.Fields.Telephone3);

            ExecuteCrmAddresses(context);
        }

        private void RemoveFormatting(Entity entity, string attribute)
        {
            var number = entity.GetAttributeValue<string>(attribute);
            if (number == null)
            {
                return;
            }

            entity[attribute] = new string(number.Where(char.IsNumber).ToArray());
        }

        private void ExecuteCrmAddresses(ExtendedPluginContext context)
        {
            // Account is used here, but since all Entities have the same exact field names, this works just fine
            var target = context.GetTarget<Entity>();

            switch (target.LogicalName)
            {
                case Contact.EntityLogicalName:
                    RemoveFormatting(target, Contact.Fields.Address3_Telephone1);
                    RemoveFormatting(target, Contact.Fields.Address3_Telephone2);
                    RemoveFormatting(target, Contact.Fields.Address3_Telephone3);
                    RemoveFormatting(target, Contact.Fields.MobilePhone);
                    break;
                case Lead.EntityLogicalName:
                case SystemUser.EntityLogicalName:
                    RemoveFormatting(target, Contact.Fields.MobilePhone);
                    break;
            }

            RemoveFormatting(target, Account.Fields.Address1_Telephone1);
            RemoveFormatting(target, Account.Fields.Address1_Telephone2);
            RemoveFormatting(target, Account.Fields.Address1_Telephone3);
            RemoveFormatting(target, Account.Fields.Address2_Telephone1);
            RemoveFormatting(target, Account.Fields.Address2_Telephone2);
            RemoveFormatting(target, Account.Fields.Address2_Telephone3);
        }
    }
}
