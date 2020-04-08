using System.Collections.Generic;
using System.ComponentModel;
using DLaB.Xrm.MismatchedAttributeToLogicalName;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class MismatchedAttributeToLogicalNameTests
    {
        [TestMethod]
        public void FilteringByMismatchedAttribute_Should_PerformFilter()
        {
            var info = LocalCrmDatabaseInfo.Create<CrmContext>("FakeEntities");
            info.PrimaryNameProvider = new PrimaryNameViaNonStandardNamesProvider(new Dictionary<string, string>{ {FakeEntity.EntityLogicalName, "fakeentity" }});
            var service = new LocalCrmDatabaseOrganizationService(info);
            var fake = new FakeEntity { FakeEntity1 = "Fake" };
            service.Create(fake);
            var dbFake = service.GetFirst<FakeEntity>("fakeentity", fake.FakeEntity1);
            Assert.AreEqual(dbFake.FakeEntity1, fake.FakeEntity1);
        }
    }
}

namespace DLaB.Xrm.MismatchedAttributeToLogicalName
{

    /// <summary>
    /// Represents a source of entities bound to a CRM service. It tracks and manages changes made to the retrieved entities.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "8.0.1.7297")]
    public class CrmContext : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
    {
        public CrmContext(IOrganizationService service) : base(service) { }
    }

    public enum FakeEntityState
    {

        [System.Runtime.Serialization.EnumMemberAttribute]
        Active = 0,

        [System.Runtime.Serialization.EnumMemberAttribute]
        Inactive = 1,
    }

    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("fakeentity")]
    public class FakeEntity : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {

        public FakeEntity() :
            base(EntityLogicalName) {}

        public const string EntityLogicalName = "fakeentity";

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        [System.Diagnostics.DebuggerNonUserCode]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        [AttributeLogicalName("fakeentity")]
        public string FakeEntity1
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("fakeentity");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("FakeEntity1");
                SetAttributeValue("fakeentity", value);
                OnPropertyChanged("FakeEntity1");
            }
        }
        
        /// <summary>
        /// Shows the status of the campaign. By default, campaigns are active and can't be deactivated.
        /// </summary>
        [AttributeLogicalNameAttribute("statecode")]
        public FakeEntityState? StateCode
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                OptionSetValue optionSet = GetAttributeValue<OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return (FakeEntityState)System.Enum.ToObject(typeof(FakeEntityState), optionSet.Value);
                }
                else
                {
                    return null;
                }
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("StateCode");
                if ((value == null))
                {
                    SetAttributeValue("statecode", null);
                }
                else
                {
                    SetAttributeValue("statecode", new OptionSetValue(((int)(value))));
                }
                OnPropertyChanged("StateCode");
            }
        }

        /// <summary>
        /// Select the campaign's status.
        /// </summary>
        [AttributeLogicalNameAttribute("statuscode")]
        public OptionSetValue StatusCode
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<OptionSetValue>("statuscode");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("StatusCode");
                SetAttributeValue("statuscode", value);
                OnPropertyChanged("StatusCode");
            }
        }
    }

    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Person with access to the Microsoft CRM system and who owns objects in the Microsoft CRM database.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("systemuser")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "8.0.1.7297")]
    public class SystemUser : Entity, INotifyPropertyChanging,
                                      INotifyPropertyChanged
    {

        public struct Fields
        {
            public const string AccessMode = "accessmode";
            public const string Address1_AddressId = "address1_addressid";        
            public const string Address1_AddressTypeCode = "address1_addresstypecode";
            public const string Address1_City = "address1_city";
            public const string Address1_Composite = "address1_composite";
            public const string Address1_Country = "address1_country";
            public const string Address1_County = "address1_county";
            public const string Address1_Fax = "address1_fax";
            public const string Address1_Latitude = "address1_latitude";
            public const string Address1_Line1 = "address1_line1";
            public const string Address1_Line2 = "address1_line2";
            public const string Address1_Line3 = "address1_line3";
            public const string Address1_Longitude = "address1_longitude";
            public const string Address1_Name = "address1_name";
            public const string Address1_PostalCode = "address1_postalcode";
            public const string Address1_PostOfficeBox = "address1_postofficebox";
            public const string Address1_ShippingMethodCode = "address1_shippingmethodcode";
            public const string Address1_StateOrProvince = "address1_stateorprovince";
            public const string Address1_Telephone1 = "address1_telephone1";
            public const string Address1_Telephone2 = "address1_telephone2";
            public const string Address1_Telephone3 = "address1_telephone3";
            public const string Address1_UPSZone = "address1_upszone";
            public const string Address1_UTCOffset = "address1_utcoffset";
            public const string Address2_AddressId = "address2_addressid";
            public const string Address2_AddressTypeCode = "address2_addresstypecode";
            public const string Address2_City = "address2_city";
            public const string Address2_Composite = "address2_composite";
            public const string Address2_Country = "address2_country";
            public const string Address2_County = "address2_county";
            public const string Address2_Fax = "address2_fax";
            public const string Address2_Latitude = "address2_latitude";
            public const string Address2_Line1 = "address2_line1";
            public const string Address2_Line2 = "address2_line2";
            public const string Address2_Line3 = "address2_line3";
            public const string Address2_Longitude = "address2_longitude";
            public const string Address2_Name = "address2_name";
            public const string Address2_PostalCode = "address2_postalcode";
            public const string Address2_PostOfficeBox = "address2_postofficebox";
            public const string Address2_ShippingMethodCode = "address2_shippingmethodcode";
            public const string Address2_StateOrProvince = "address2_stateorprovince";
            public const string Address2_Telephone1 = "address2_telephone1";
            public const string Address2_Telephone2 = "address2_telephone2";
            public const string Address2_Telephone3 = "address2_telephone3";
            public const string Address2_UPSZone = "address2_upszone";
            public const string Address2_UTCOffset = "address2_utcoffset";
            public const string BusinessUnitId = "businessunitid";
            public const string CalendarId = "calendarid";
            public const string CALType = "caltype";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string DefaultFiltersPopulated = "defaultfilterspopulated";
            public const string DefaultMailbox = "defaultmailbox";
            public const string DefaultOdbFolderName = "defaultodbfoldername";
            public const string DisabledReason = "disabledreason";
            public const string DisplayInServiceViews = "displayinserviceviews";
            public const string DomainName = "domainname";
            public const string EmailRouterAccessApproval = "emailrouteraccessapproval";
            public const string EmployeeId = "employeeid";
            public const string EntityImage = "entityimage";
            public const string EntityImage_Timestamp = "entityimage_timestamp";
            public const string EntityImage_URL = "entityimage_url";
            public const string EntityImageId = "entityimageid";
            public const string ExchangeRate = "exchangerate";
            public const string FirstName = "firstname";
            public const string FullName = "fullname";
            public const string GovernmentId = "governmentid";
            public const string HomePhone = "homephone";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string IncomingEmailDeliveryMethod = "incomingemaildeliverymethod";
            public const string InternalEMailAddress = "internalemailaddress";
            public const string InviteStatusCode = "invitestatuscode";
            public const string IsDisabled = "isdisabled";
            public const string IsEmailAddressApprovedByO365Admin = "isemailaddressapprovedbyo365admin";
            public const string IsIntegrationUser = "isintegrationuser";
            public const string IsLicensed = "islicensed";
            public const string IsSyncWithDirectory = "issyncwithdirectory";
            public const string JobTitle = "jobtitle";
            public const string LastName = "lastname";
            public const string MiddleName = "middlename";
            public const string MobileAlertEMail = "mobilealertemail";
            public const string MobileOfflineProfileId = "mobileofflineprofileid";
            public const string MobilePhone = "mobilephone";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string NickName = "nickname";
            public const string OrganizationId = "organizationid";
            public const string OutgoingEmailDeliveryMethod = "outgoingemaildeliverymethod";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string ParentSystemUserId = "parentsystemuserid";
            public const string PassportHi = "passporthi";
            public const string PassportLo = "passportlo";
            public const string PersonalEMailAddress = "personalemailaddress";
            public const string PhotoUrl = "photourl";
            public const string PositionId = "positionid";
            public const string PreferredAddressCode = "preferredaddresscode";
            public const string PreferredEmailCode = "preferredemailcode";
            public const string PreferredPhoneCode = "preferredphonecode";
            public const string ProcessId = "processid";
            public const string QueueId = "queueid";
            public const string Salutation = "salutation";
            public const string SetupUser = "setupuser";
            public const string SharePointEmailAddress = "sharepointemailaddress";
            public const string SiteId = "siteid";
            public const string Skills = "skills";
            public const string StageId = "stageid";
            public const string SystemUserId = "systemuserid";
            public const string Id = "systemuserid";
            public const string TerritoryId = "territoryid";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string Title = "title";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string TraversedPath = "traversedpath";
            public const string UserLicenseType = "userlicensetype";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
            public const string WindowsLiveID = "windowsliveid";
            public const string YammerEmailAddress = "yammeremailaddress";
            public const string YammerUserId = "yammeruserid";
            public const string YomiFirstName = "yomifirstname";
            public const string YomiFullName = "yomifullname";
            public const string YomiLastName = "yomilastname";
            public const string YomiMiddleName = "yomimiddlename";
            public const string business_unit_system_users = "business_unit_system_users";
            public const string calendar_system_users = "calendar_system_users";
            public const string Referencinglk_systemuser_createdonbehalfby = "lk_systemuser_createdonbehalfby";
            public const string Referencinglk_systemuser_modifiedonbehalfby = "lk_systemuser_modifiedonbehalfby";
            public const string Referencinglk_systemuserbase_createdby = "lk_systemuserbase_createdby";
            public const string Referencinglk_systemuserbase_modifiedby = "lk_systemuserbase_modifiedby";
            public const string MobileOfflineProfile_SystemUser = "MobileOfflineProfile_SystemUser";
            public const string organization_system_users = "organization_system_users";
            public const string position_users = "position_users";
            public const string processstage_systemusers = "processstage_systemusers";
            public const string queue_system_user = "queue_system_user";
            public const string site_system_users = "site_system_users";
            public const string systemuser_defaultmailbox_mailbox = "systemuser_defaultmailbox_mailbox";
            public const string territory_system_users = "territory_system_users";
            public const string TransactionCurrency_SystemUser = "TransactionCurrency_SystemUser";
            public const string Referencinguser_parent_user = "user_parent_user";
        }


        /// <summary>
        /// Default Constructor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public SystemUser() :
            base(EntityLogicalName) {}

        public const string EntityLogicalName = "systemuser";

        public const int EntityTypeCode = 8;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        [System.Diagnostics.DebuggerNonUserCode]
        private void OnPropertyChanged(string propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void OnPropertyChanging(string propertyName)
        {
            if ((PropertyChanging != null))
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Active Directory domain of which the user is a member.
        /// </summary>
        [AttributeLogicalNameAttribute("domainname")]
        public string DomainName
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("domainname");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("DomainName");
                SetAttributeValue("domainname", value);
                OnPropertyChanged("DomainName");
            }
        }

        /// <summary>
        /// First name of the user.
        /// </summary>
        [AttributeLogicalNameAttribute("firstname")]
        public string FirstName
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("firstname");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("FirstName");
                SetAttributeValue("firstname", value);
                OnPropertyChanged("FirstName");
            }
        }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        [AttributeLogicalNameAttribute("fullname")]
        public string FullName
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("fullname");
            }
        }

        [AttributeLogicalNameAttribute("isdisabled")]
        public bool? IsDisabled
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<bool?>("isdisabled");
            }
        }

        /// <summary>
        /// Last name of the user.
        /// </summary>
        [AttributeLogicalNameAttribute("lastname")]
        public string LastName
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("lastname");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("LastName");
                SetAttributeValue("lastname", value);
                OnPropertyChanged("LastName");
            }
        }

        /// <summary>
        /// Middle name of the user.
        /// </summary>
        [AttributeLogicalNameAttribute("middlename")]
        public string MiddleName
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<string>("middlename");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("MiddleName");
                SetAttributeValue("middlename", value);
                OnPropertyChanged("MiddleName");
            }
        }

        /// <summary>
        /// Unique identifier of the organization associated with the user.
        /// </summary>
        [AttributeLogicalNameAttribute("organizationid")]
        public System.Nullable<System.Guid> OrganizationId
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<System.Nullable<System.Guid>>("organizationid");
            }
        }


        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        [AttributeLogicalNameAttribute("systemuserid")]
        public System.Nullable<System.Guid> SystemUserId
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return GetAttributeValue<System.Nullable<System.Guid>>("systemuserid");
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                OnPropertyChanging("SystemUserId");
                SetAttributeValue("systemuserid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                OnPropertyChanged("SystemUserId");
            }
        }

        [AttributeLogicalNameAttribute("systemuserid")]
        public override System.Guid Id
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get
            {
                return base.Id;
            }
            [System.Diagnostics.DebuggerNonUserCode]
            set
            {
                SystemUserId = value;
            }
        }
    }
}
// ReSharper restore InconsistentNaming