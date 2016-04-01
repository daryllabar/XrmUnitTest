using System.Linq;
using DLaB.Xrm.Entities;
using DLaB.Xrm.Plugin;
using Microsoft.Xrm.Sdk;

namespace Example.Plugin.Simple
{
    /// <summary>
    /// Class to Sync a Contact's Address Information to the Account, if empty
    /// </summary>
    public class SyncContactToAccount : DLaBPluginBase
    {
        #region Constructors

        public SyncContactToAccount() : this(null, null) { }
        public SyncContactToAccount(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
        }

        #endregion Constructors

        protected override IRegisteredEventsPluginHandler GetPluginHandler()
        {
            return new SyncContactToAccountLogic();
        }
    }

    internal class SyncContactToAccountLogic : DLaBPluginHandlerBase
    {
        public override void RegisterEvents()
        {
            RegisteredEvents.AddRange(new RegisteredEventBuilder(PipelineStage.PostOperation, MessageType.Create, MessageType.Update).
                ForEntities(Contact.EntityLogicalName).Build());
        }

        protected override void ExecuteInternal(IExtendedPluginContext context)
        {
            // Get the Target, plus the AccountId from the Post Entity since it may not have changed and is most likely not in the Target
            var contact = context.CoallesceTargetWithPostEntity<Contact>();
            if (contact.AccountId == null || string.IsNullOrWhiteSpace(contact.Address1_Line1))
            {
                // No Account or no address, no Update Needed
                return;
            }

            using (var crm = new CrmContext(context.OrganizationService))
            {
                var account = crm.AccountSet.First(a => a.AccountId == contact.AccountId.Id);
                if (string.IsNullOrWhiteSpace(account.Address1_Line1))
                {
                    UpdateAccountAddress(context.OrganizationService, contact);
                }
            }
        }

        private void UpdateAccountAddress(IOrganizationService service, Contact contact)
        {
            var account = new Account
            {
                Address1_Line1 = contact.Address1_Line1,
                Address1_Line2 = contact.Address1_Line2,
                Address1_Line3 = contact.Address1_Line3,
                Address1_AddressTypeCode = contact.Address1_AddressTypeCode,
                Address1_City = contact.Address1_City,
                Address1_Country = contact.Address1_Country,
                Address1_County = contact.Address1_County,
                Address1_StateOrProvince = contact.Address1_StateOrProvince,
                Address1_Name = contact.Address1_Name,
                Address1_PostalCode = contact.Address1_PostalCode,
                Id = contact.AccountId.Id
            };

            service.Update(account);
        }
    }
}
