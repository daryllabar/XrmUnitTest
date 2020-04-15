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

#region Entity Definitions

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
        /// Fiscal calendar associated with the business unit.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
        public Microsoft.Xrm.Sdk.EntityReference BusinessUnitId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("businessunitid");
            }
            set
            {
                this.OnPropertyChanging("BusinessUnitId");
                this.SetAttributeValue("businessunitid", value);
                this.OnPropertyChanged("BusinessUnitId");
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

	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("businessunit")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.42")]
	public partial class BusinessUnit : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{

		public static class Fields
		{
			public const string Address1_AddressId = "address1_addressid";
			public const string Address1_AddressTypeCode = "address1_addresstypecode";
			public const string Address1_City = "address1_city";
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
			public const string Id = "businessunitid";
			public const string CalendarId = "calendarid";
			public const string CostCenter = "costcenter";
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string CreditLimit = "creditlimit";
			public const string Description = "description";
			public const string DisabledReason = "disabledreason";
			public const string DivisionName = "divisionname";
			public const string EMailAddress = "emailaddress";
			public const string ExchangeRate = "exchangerate";
			public const string FileAsName = "fileasname";
			public const string FtpSiteUrl = "ftpsiteurl";
			public const string hc_CompliancePackageId = "hc_compliancepackageid";
			public const string hc_MarketingTextNumber = "hc_marketingtextnumber";
			public const string hc_Office = "hc_office";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string InheritanceMask = "inheritancemask";
			public const string IsDisabled = "isdisabled";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string Name = "name";
			public const string OrganizationId = "organizationid";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string ParentBusinessUnitId = "parentbusinessunitid";
			public const string Picture = "picture";
			public const string StockExchange = "stockexchange";
			public const string TickerSymbol = "tickersymbol";
			public const string TransactionCurrencyId = "transactioncurrencyid";
			public const string UserGroupId = "usergroupid";
			public const string UTCOffset = "utcoffset";
			public const string VersionNumber = "versionnumber";
			public const string WebSiteUrl = "websiteurl";
			public const string WorkflowSuspended = "workflowsuspended";
		}

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public BusinessUnit() :
				base(EntityLogicalName)
		{
		}

		public const string EntityLogicalName = "businessunit";

		public const string EntitySchemaName = "BusinessUnit";

		public const string PrimaryIdAttribute = "businessunitid";

		public const string PrimaryNameAttribute = "name";

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		private void OnPropertyChanged(string propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private void OnPropertyChanging(string propertyName)
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
			}
		}

		/// <summary>
		/// Unique identifier for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_addressid")]
		public System.Nullable<System.Guid> Address1_AddressId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("address1_addressid");
			}
			set
			{
				this.OnPropertyChanging("Address1_AddressId");
				this.SetAttributeValue("address1_addressid", value);
				this.OnPropertyChanged("Address1_AddressId");
			}
		}

		/// <summary>
		/// City name for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_city")]
		public string Address1_City
		{
			get
			{
				return this.GetAttributeValue<string>("address1_city");
			}
			set
			{
				this.OnPropertyChanging("Address1_City");
				this.SetAttributeValue("address1_city", value);
				this.OnPropertyChanged("Address1_City");
			}
		}

		/// <summary>
		/// Country/region name for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_country")]
		public string Address1_Country
		{
			get
			{
				return this.GetAttributeValue<string>("address1_country");
			}
			set
			{
				this.OnPropertyChanging("Address1_Country");
				this.SetAttributeValue("address1_country", value);
				this.OnPropertyChanged("Address1_Country");
			}
		}

		/// <summary>
		/// County name for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_county")]
		public string Address1_County
		{
			get
			{
				return this.GetAttributeValue<string>("address1_county");
			}
			set
			{
				this.OnPropertyChanging("Address1_County");
				this.SetAttributeValue("address1_county", value);
				this.OnPropertyChanged("Address1_County");
			}
		}

		/// <summary>
		/// Fax number for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_fax")]
		public string Address1_Fax
		{
			get
			{
				return this.GetAttributeValue<string>("address1_fax");
			}
			set
			{
				this.OnPropertyChanging("Address1_Fax");
				this.SetAttributeValue("address1_fax", value);
				this.OnPropertyChanged("Address1_Fax");
			}
		}

		/// <summary>
		/// Latitude for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_latitude")]
		public System.Nullable<double> Address1_Latitude
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<double>>("address1_latitude");
			}
			set
			{
				this.OnPropertyChanging("Address1_Latitude");
				this.SetAttributeValue("address1_latitude", value);
				this.OnPropertyChanged("Address1_Latitude");
			}
		}

		/// <summary>
		/// First line for entering address 1 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_line1")]
		public string Address1_Line1
		{
			get
			{
				return this.GetAttributeValue<string>("address1_line1");
			}
			set
			{
				this.OnPropertyChanging("Address1_Line1");
				this.SetAttributeValue("address1_line1", value);
				this.OnPropertyChanged("Address1_Line1");
			}
		}

		/// <summary>
		/// Second line for entering address 1 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_line2")]
		public string Address1_Line2
		{
			get
			{
				return this.GetAttributeValue<string>("address1_line2");
			}
			set
			{
				this.OnPropertyChanging("Address1_Line2");
				this.SetAttributeValue("address1_line2", value);
				this.OnPropertyChanged("Address1_Line2");
			}
		}

		/// <summary>
		/// Third line for entering address 1 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_line3")]
		public string Address1_Line3
		{
			get
			{
				return this.GetAttributeValue<string>("address1_line3");
			}
			set
			{
				this.OnPropertyChanging("Address1_Line3");
				this.SetAttributeValue("address1_line3", value);
				this.OnPropertyChanged("Address1_Line3");
			}
		}

		/// <summary>
		/// Longitude for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_longitude")]
		public System.Nullable<double> Address1_Longitude
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<double>>("address1_longitude");
			}
			set
			{
				this.OnPropertyChanging("Address1_Longitude");
				this.SetAttributeValue("address1_longitude", value);
				this.OnPropertyChanged("Address1_Longitude");
			}
		}

		/// <summary>
		/// Name to enter for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_name")]
		public string Address1_Name
		{
			get
			{
				return this.GetAttributeValue<string>("address1_name");
			}
			set
			{
				this.OnPropertyChanging("Address1_Name");
				this.SetAttributeValue("address1_name", value);
				this.OnPropertyChanged("Address1_Name");
			}
		}

		/// <summary>
		/// ZIP Code or postal code for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_postalcode")]
		public string Address1_PostalCode
		{
			get
			{
				return this.GetAttributeValue<string>("address1_postalcode");
			}
			set
			{
				this.OnPropertyChanging("Address1_PostalCode");
				this.SetAttributeValue("address1_postalcode", value);
				this.OnPropertyChanged("Address1_PostalCode");
			}
		}

		/// <summary>
		/// Post office box number for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_postofficebox")]
		public string Address1_PostOfficeBox
		{
			get
			{
				return this.GetAttributeValue<string>("address1_postofficebox");
			}
			set
			{
				this.OnPropertyChanging("Address1_PostOfficeBox");
				this.SetAttributeValue("address1_postofficebox", value);
				this.OnPropertyChanged("Address1_PostOfficeBox");
			}
		}

		/// <summary>
		/// State or province for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_stateorprovince")]
		public string Address1_StateOrProvince
		{
			get
			{
				return this.GetAttributeValue<string>("address1_stateorprovince");
			}
			set
			{
				this.OnPropertyChanging("Address1_StateOrProvince");
				this.SetAttributeValue("address1_stateorprovince", value);
				this.OnPropertyChanged("Address1_StateOrProvince");
			}
		}

		/// <summary>
		/// First telephone number associated with address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_telephone1")]
		public string Address1_Telephone1
		{
			get
			{
				return this.GetAttributeValue<string>("address1_telephone1");
			}
			set
			{
				this.OnPropertyChanging("Address1_Telephone1");
				this.SetAttributeValue("address1_telephone1", value);
				this.OnPropertyChanged("Address1_Telephone1");
			}
		}

		/// <summary>
		/// Second telephone number associated with address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_telephone2")]
		public string Address1_Telephone2
		{
			get
			{
				return this.GetAttributeValue<string>("address1_telephone2");
			}
			set
			{
				this.OnPropertyChanging("Address1_Telephone2");
				this.SetAttributeValue("address1_telephone2", value);
				this.OnPropertyChanged("Address1_Telephone2");
			}
		}

		/// <summary>
		/// Third telephone number associated with address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_telephone3")]
		public string Address1_Telephone3
		{
			get
			{
				return this.GetAttributeValue<string>("address1_telephone3");
			}
			set
			{
				this.OnPropertyChanging("Address1_Telephone3");
				this.SetAttributeValue("address1_telephone3", value);
				this.OnPropertyChanged("Address1_Telephone3");
			}
		}

		/// <summary>
		/// United Parcel Service (UPS) zone for address 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_upszone")]
		public string Address1_UPSZone
		{
			get
			{
				return this.GetAttributeValue<string>("address1_upszone");
			}
			set
			{
				this.OnPropertyChanging("Address1_UPSZone");
				this.SetAttributeValue("address1_upszone", value);
				this.OnPropertyChanged("Address1_UPSZone");
			}
		}

		/// <summary>
		/// UTC offset for address 1. This is the difference between local time and standard Coordinated Universal Time.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address1_utcoffset")]
		public System.Nullable<int> Address1_UTCOffset
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("address1_utcoffset");
			}
			set
			{
				this.OnPropertyChanging("Address1_UTCOffset");
				this.SetAttributeValue("address1_utcoffset", value);
				this.OnPropertyChanged("Address1_UTCOffset");
			}
		}

		/// <summary>
		/// Unique identifier for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_addressid")]
		public System.Nullable<System.Guid> Address2_AddressId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("address2_addressid");
			}
			set
			{
				this.OnPropertyChanging("Address2_AddressId");
				this.SetAttributeValue("address2_addressid", value);
				this.OnPropertyChanged("Address2_AddressId");
			}
		}

		/// <summary>
		/// City name for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_city")]
		public string Address2_City
		{
			get
			{
				return this.GetAttributeValue<string>("address2_city");
			}
			set
			{
				this.OnPropertyChanging("Address2_City");
				this.SetAttributeValue("address2_city", value);
				this.OnPropertyChanged("Address2_City");
			}
		}

		/// <summary>
		/// Country/region name for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_country")]
		public string Address2_Country
		{
			get
			{
				return this.GetAttributeValue<string>("address2_country");
			}
			set
			{
				this.OnPropertyChanging("Address2_Country");
				this.SetAttributeValue("address2_country", value);
				this.OnPropertyChanged("Address2_Country");
			}
		}

		/// <summary>
		/// County name for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_county")]
		public string Address2_County
		{
			get
			{
				return this.GetAttributeValue<string>("address2_county");
			}
			set
			{
				this.OnPropertyChanging("Address2_County");
				this.SetAttributeValue("address2_county", value);
				this.OnPropertyChanged("Address2_County");
			}
		}

		/// <summary>
		/// Fax number for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_fax")]
		public string Address2_Fax
		{
			get
			{
				return this.GetAttributeValue<string>("address2_fax");
			}
			set
			{
				this.OnPropertyChanging("Address2_Fax");
				this.SetAttributeValue("address2_fax", value);
				this.OnPropertyChanged("Address2_Fax");
			}
		}

		/// <summary>
		/// Latitude for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_latitude")]
		public System.Nullable<double> Address2_Latitude
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<double>>("address2_latitude");
			}
			set
			{
				this.OnPropertyChanging("Address2_Latitude");
				this.SetAttributeValue("address2_latitude", value);
				this.OnPropertyChanged("Address2_Latitude");
			}
		}

		/// <summary>
		/// First line for entering address 2 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_line1")]
		public string Address2_Line1
		{
			get
			{
				return this.GetAttributeValue<string>("address2_line1");
			}
			set
			{
				this.OnPropertyChanging("Address2_Line1");
				this.SetAttributeValue("address2_line1", value);
				this.OnPropertyChanged("Address2_Line1");
			}
		}

		/// <summary>
		/// Second line for entering address 2 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_line2")]
		public string Address2_Line2
		{
			get
			{
				return this.GetAttributeValue<string>("address2_line2");
			}
			set
			{
				this.OnPropertyChanging("Address2_Line2");
				this.SetAttributeValue("address2_line2", value);
				this.OnPropertyChanged("Address2_Line2");
			}
		}

		/// <summary>
		/// Third line for entering address 2 information.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_line3")]
		public string Address2_Line3
		{
			get
			{
				return this.GetAttributeValue<string>("address2_line3");
			}
			set
			{
				this.OnPropertyChanging("Address2_Line3");
				this.SetAttributeValue("address2_line3", value);
				this.OnPropertyChanged("Address2_Line3");
			}
		}

		/// <summary>
		/// Longitude for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_longitude")]
		public System.Nullable<double> Address2_Longitude
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<double>>("address2_longitude");
			}
			set
			{
				this.OnPropertyChanging("Address2_Longitude");
				this.SetAttributeValue("address2_longitude", value);
				this.OnPropertyChanged("Address2_Longitude");
			}
		}

		/// <summary>
		/// Name to enter for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_name")]
		public string Address2_Name
		{
			get
			{
				return this.GetAttributeValue<string>("address2_name");
			}
			set
			{
				this.OnPropertyChanging("Address2_Name");
				this.SetAttributeValue("address2_name", value);
				this.OnPropertyChanged("Address2_Name");
			}
		}

		/// <summary>
		/// ZIP Code or postal code for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_postalcode")]
		public string Address2_PostalCode
		{
			get
			{
				return this.GetAttributeValue<string>("address2_postalcode");
			}
			set
			{
				this.OnPropertyChanging("Address2_PostalCode");
				this.SetAttributeValue("address2_postalcode", value);
				this.OnPropertyChanged("Address2_PostalCode");
			}
		}

		/// <summary>
		/// Post office box number for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_postofficebox")]
		public string Address2_PostOfficeBox
		{
			get
			{
				return this.GetAttributeValue<string>("address2_postofficebox");
			}
			set
			{
				this.OnPropertyChanging("Address2_PostOfficeBox");
				this.SetAttributeValue("address2_postofficebox", value);
				this.OnPropertyChanged("Address2_PostOfficeBox");
			}
		}

		/// <summary>
		/// State or province for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_stateorprovince")]
		public string Address2_StateOrProvince
		{
			get
			{
				return this.GetAttributeValue<string>("address2_stateorprovince");
			}
			set
			{
				this.OnPropertyChanging("Address2_StateOrProvince");
				this.SetAttributeValue("address2_stateorprovince", value);
				this.OnPropertyChanged("Address2_StateOrProvince");
			}
		}

		/// <summary>
		/// First telephone number associated with address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_telephone1")]
		public string Address2_Telephone1
		{
			get
			{
				return this.GetAttributeValue<string>("address2_telephone1");
			}
			set
			{
				this.OnPropertyChanging("Address2_Telephone1");
				this.SetAttributeValue("address2_telephone1", value);
				this.OnPropertyChanged("Address2_Telephone1");
			}
		}

		/// <summary>
		/// Second telephone number associated with address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_telephone2")]
		public string Address2_Telephone2
		{
			get
			{
				return this.GetAttributeValue<string>("address2_telephone2");
			}
			set
			{
				this.OnPropertyChanging("Address2_Telephone2");
				this.SetAttributeValue("address2_telephone2", value);
				this.OnPropertyChanged("Address2_Telephone2");
			}
		}

		/// <summary>
		/// Third telephone number associated with address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_telephone3")]
		public string Address2_Telephone3
		{
			get
			{
				return this.GetAttributeValue<string>("address2_telephone3");
			}
			set
			{
				this.OnPropertyChanging("Address2_Telephone3");
				this.SetAttributeValue("address2_telephone3", value);
				this.OnPropertyChanged("Address2_Telephone3");
			}
		}

		/// <summary>
		/// United Parcel Service (UPS) zone for address 2.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_upszone")]
		public string Address2_UPSZone
		{
			get
			{
				return this.GetAttributeValue<string>("address2_upszone");
			}
			set
			{
				this.OnPropertyChanging("Address2_UPSZone");
				this.SetAttributeValue("address2_upszone", value);
				this.OnPropertyChanged("Address2_UPSZone");
			}
		}

		/// <summary>
		/// UTC offset for address 2. This is the difference between local time and standard Coordinated Universal Time.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("address2_utcoffset")]
		public System.Nullable<int> Address2_UTCOffset
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("address2_utcoffset");
			}
			set
			{
				this.OnPropertyChanging("Address2_UTCOffset");
				this.SetAttributeValue("address2_utcoffset", value);
				this.OnPropertyChanged("Address2_UTCOffset");
			}
		}

		/// <summary>
		/// Unique identifier of the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
		public System.Nullable<System.Guid> BusinessUnitId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("businessunitid");
			}
			set
			{
				this.OnPropertyChanging("BusinessUnitId");
				this.SetAttributeValue("businessunitid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("BusinessUnitId");
			}
		}

		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
		public override System.Guid Id
		{
			get
			{
				return base.Id;
			}
			set
			{
				this.BusinessUnitId = value;
			}
		}

		/// <summary>
		/// Fiscal calendar associated with the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendarid")]
		public Microsoft.Xrm.Sdk.EntityReference CalendarId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("calendarid");
			}
			set
			{
				this.OnPropertyChanging("CalendarId");
				this.SetAttributeValue("calendarid", value);
				this.OnPropertyChanged("CalendarId");
			}
		}

		/// <summary>
		/// Name of the business unit cost center.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("costcenter")]
		public string CostCenter
		{
			get
			{
				return this.GetAttributeValue<string>("costcenter");
			}
			set
			{
				this.OnPropertyChanging("CostCenter");
				this.SetAttributeValue("costcenter", value);
				this.OnPropertyChanged("CostCenter");
			}
		}

		/// <summary>
		/// Unique identifier of the user who created the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
			}
			set
			{
				this.OnPropertyChanging("CreatedBy");
				this.SetAttributeValue("createdby", value);
				this.OnPropertyChanged("CreatedBy");
			}
		}

		/// <summary>
		/// Date and time when the business unit was created.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
		public System.Nullable<System.DateTime> CreatedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
			}
			set
			{
				this.OnPropertyChanging("CreatedOn");
				this.SetAttributeValue("createdon", value);
				this.OnPropertyChanged("CreatedOn");
			}
		}

		/// <summary>
		/// Unique identifier of the delegate user who created the businessunit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
			}
			set
			{
				this.OnPropertyChanging("CreatedOnBehalfBy");
				this.SetAttributeValue("createdonbehalfby", value);
				this.OnPropertyChanged("CreatedOnBehalfBy");
			}
		}

		/// <summary>
		/// Credit limit for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("creditlimit")]
		public System.Nullable<double> CreditLimit
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<double>>("creditlimit");
			}
			set
			{
				this.OnPropertyChanging("CreditLimit");
				this.SetAttributeValue("creditlimit", value);
				this.OnPropertyChanged("CreditLimit");
			}
		}

		/// <summary>
		/// Description of the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("description")]
		public string Description
		{
			get
			{
				return this.GetAttributeValue<string>("description");
			}
			set
			{
				this.OnPropertyChanging("Description");
				this.SetAttributeValue("description", value);
				this.OnPropertyChanged("Description");
			}
		}

		/// <summary>
		/// Reason for disabling the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("disabledreason")]
		public string DisabledReason
		{
			get
			{
				return this.GetAttributeValue<string>("disabledreason");
			}
			set
			{
				this.OnPropertyChanging("DisabledReason");
				this.SetAttributeValue("disabledreason", value);
				this.OnPropertyChanged("DisabledReason");
			}
		}

		/// <summary>
		/// Name of the division to which the business unit belongs.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("divisionname")]
		public string DivisionName
		{
			get
			{
				return this.GetAttributeValue<string>("divisionname");
			}
			set
			{
				this.OnPropertyChanging("DivisionName");
				this.SetAttributeValue("divisionname", value);
				this.OnPropertyChanged("DivisionName");
			}
		}

		/// <summary>
		/// Email address for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("emailaddress")]
		public string EMailAddress
		{
			get
			{
				return this.GetAttributeValue<string>("emailaddress");
			}
			set
			{
				this.OnPropertyChanging("EMailAddress");
				this.SetAttributeValue("emailaddress", value);
				this.OnPropertyChanged("EMailAddress");
			}
		}

		/// <summary>
		/// Exchange rate for the currency associated with the businessunit with respect to the base currency.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("exchangerate")]
		public System.Nullable<decimal> ExchangeRate
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<decimal>>("exchangerate");
			}
			set
			{
				this.OnPropertyChanging("ExchangeRate");
				this.SetAttributeValue("exchangerate", value);
				this.OnPropertyChanged("ExchangeRate");
			}
		}

		/// <summary>
		/// Alternative name under which the business unit can be filed.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("fileasname")]
		public string FileAsName
		{
			get
			{
				return this.GetAttributeValue<string>("fileasname");
			}
			set
			{
				this.OnPropertyChanging("FileAsName");
				this.SetAttributeValue("fileasname", value);
				this.OnPropertyChanged("FileAsName");
			}
		}

		/// <summary>
		/// FTP site URL for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ftpsiteurl")]
		public string FtpSiteUrl
		{
			get
			{
				return this.GetAttributeValue<string>("ftpsiteurl");
			}
			set
			{
				this.OnPropertyChanging("FtpSiteUrl");
				this.SetAttributeValue("ftpsiteurl", value);
				this.OnPropertyChanged("FtpSiteUrl");
			}
		}

		/// <summary>
		/// Unique identifier for Compliance Package associated with Business Unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("hc_compliancepackageid")]
		public Microsoft.Xrm.Sdk.EntityReference hc_CompliancePackageId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("hc_compliancepackageid");
			}
			set
			{
				this.OnPropertyChanging("hc_CompliancePackageId");
				this.SetAttributeValue("hc_compliancepackageid", value);
				this.OnPropertyChanged("hc_CompliancePackageId");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("hc_marketingtextnumber")]
		public string hc_MarketingTextNumber
		{
			get
			{
				return this.GetAttributeValue<string>("hc_marketingtextnumber");
			}
			set
			{
				this.OnPropertyChanging("hc_MarketingTextNumber");
				this.SetAttributeValue("hc_marketingtextnumber", value);
				this.OnPropertyChanged("hc_MarketingTextNumber");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("hc_office")]
		public Microsoft.Xrm.Sdk.EntityReference hc_Office
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("hc_office");
			}
			set
			{
				this.OnPropertyChanging("hc_Office");
				this.SetAttributeValue("hc_office", value);
				this.OnPropertyChanged("hc_Office");
			}
		}

		/// <summary>
		/// Unique identifier of the data import or data migration that created this record.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("importsequencenumber")]
		public System.Nullable<int> ImportSequenceNumber
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("importsequencenumber");
			}
			set
			{
				this.OnPropertyChanging("ImportSequenceNumber");
				this.SetAttributeValue("importsequencenumber", value);
				this.OnPropertyChanged("ImportSequenceNumber");
			}
		}

		/// <summary>
		/// Inheritance mask for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("inheritancemask")]
		public System.Nullable<int> InheritanceMask
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("inheritancemask");
			}
			set
			{
				this.OnPropertyChanging("InheritanceMask");
				this.SetAttributeValue("inheritancemask", value);
				this.OnPropertyChanged("InheritanceMask");
			}
		}

		/// <summary>
		/// Information about whether the business unit is enabled or disabled.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isdisabled")]
		public System.Nullable<bool> IsDisabled
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("isdisabled");
			}
			set
			{
				this.OnPropertyChanging("IsDisabled");
				this.SetAttributeValue("isdisabled", value);
				this.OnPropertyChanged("IsDisabled");
			}
		}

		/// <summary>
		/// Unique identifier of the user who last modified the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
			}
			set
			{
				this.OnPropertyChanging("ModifiedBy");
				this.SetAttributeValue("modifiedby", value);
				this.OnPropertyChanged("ModifiedBy");
			}
		}

		/// <summary>
		/// Date and time when the business unit was last modified.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
		public System.Nullable<System.DateTime> ModifiedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
			}
			set
			{
				this.OnPropertyChanging("ModifiedOn");
				this.SetAttributeValue("modifiedon", value);
				this.OnPropertyChanged("ModifiedOn");
			}
		}

		/// <summary>
		/// Unique identifier of the delegate user who last modified the businessunit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
			}
			set
			{
				this.OnPropertyChanging("ModifiedOnBehalfBy");
				this.SetAttributeValue("modifiedonbehalfby", value);
				this.OnPropertyChanged("ModifiedOnBehalfBy");
			}
		}

		/// <summary>
		/// Name of the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("name")]
		public string Name
		{
			get
			{
				return this.GetAttributeValue<string>("name");
			}
			set
			{
				this.OnPropertyChanging("Name");
				this.SetAttributeValue("name", value);
				this.OnPropertyChanged("Name");
			}
		}

		/// <summary>
		/// Unique identifier of the organization associated with the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
		public Microsoft.Xrm.Sdk.EntityReference OrganizationId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("organizationid");
			}
			set
			{
				this.OnPropertyChanging("OrganizationId");
				this.SetAttributeValue("organizationid", value);
				this.OnPropertyChanged("OrganizationId");
			}
		}

		/// <summary>
		/// Date and time that the record was migrated.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overriddencreatedon")]
		public System.Nullable<System.DateTime> OverriddenCreatedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
			}
			set
			{
				this.OnPropertyChanging("OverriddenCreatedOn");
				this.SetAttributeValue("overriddencreatedon", value);
				this.OnPropertyChanged("OverriddenCreatedOn");
			}
		}

		/// <summary>
		/// Unique identifier for the parent business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("parentbusinessunitid")]
		public Microsoft.Xrm.Sdk.EntityReference ParentBusinessUnitId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("parentbusinessunitid");
			}
			set
			{
				this.OnPropertyChanging("ParentBusinessUnitId");
				this.SetAttributeValue("parentbusinessunitid", value);
				this.OnPropertyChanged("ParentBusinessUnitId");
			}
		}

		/// <summary>
		/// Picture or diagram of the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("picture")]
		public string Picture
		{
			get
			{
				return this.GetAttributeValue<string>("picture");
			}
			set
			{
				this.OnPropertyChanging("Picture");
				this.SetAttributeValue("picture", value);
				this.OnPropertyChanged("Picture");
			}
		}

		/// <summary>
		/// Stock exchange on which the business is listed.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("stockexchange")]
		public string StockExchange
		{
			get
			{
				return this.GetAttributeValue<string>("stockexchange");
			}
			set
			{
				this.OnPropertyChanging("StockExchange");
				this.SetAttributeValue("stockexchange", value);
				this.OnPropertyChanged("StockExchange");
			}
		}

		/// <summary>
		/// Stock exchange ticker symbol for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("tickersymbol")]
		public string TickerSymbol
		{
			get
			{
				return this.GetAttributeValue<string>("tickersymbol");
			}
			set
			{
				this.OnPropertyChanging("TickerSymbol");
				this.SetAttributeValue("tickersymbol", value);
				this.OnPropertyChanged("TickerSymbol");
			}
		}

		/// <summary>
		/// Unique identifier of the currency associated with the businessunit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("transactioncurrencyid")]
		public Microsoft.Xrm.Sdk.EntityReference TransactionCurrencyId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("transactioncurrencyid");
			}
			set
			{
				this.OnPropertyChanging("TransactionCurrencyId");
				this.SetAttributeValue("transactioncurrencyid", value);
				this.OnPropertyChanged("TransactionCurrencyId");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("usergroupid")]
		public System.Nullable<System.Guid> UserGroupId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("usergroupid");
			}
			set
			{
				this.OnPropertyChanging("UserGroupId");
				this.SetAttributeValue("usergroupid", value);
				this.OnPropertyChanged("UserGroupId");
			}
		}

		/// <summary>
		/// UTC offset for the business unit. This is the difference between local time and standard Coordinated Universal Time.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("utcoffset")]
		public System.Nullable<int> UTCOffset
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("utcoffset");
			}
			set
			{
				this.OnPropertyChanging("UTCOffset");
				this.SetAttributeValue("utcoffset", value);
				this.OnPropertyChanged("UTCOffset");
			}
		}

		/// <summary>
		/// Version number of the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
		public System.Nullable<long> VersionNumber
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
			}
			set
			{
				this.OnPropertyChanging("VersionNumber");
				this.SetAttributeValue("versionnumber", value);
				this.OnPropertyChanged("VersionNumber");
			}
		}

		/// <summary>
		/// Website URL for the business unit.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("websiteurl")]
		public string WebSiteUrl
		{
			get
			{
				return this.GetAttributeValue<string>("websiteurl");
			}
			set
			{
				this.OnPropertyChanging("WebSiteUrl");
				this.SetAttributeValue("websiteurl", value);
				this.OnPropertyChanged("WebSiteUrl");
			}
		}

		/// <summary>
		/// Information about whether workflow or sales process rules have been suspended.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("workflowsuspended")]
		public System.Nullable<bool> WorkflowSuspended
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("workflowsuspended");
			}
			set
			{
				this.OnPropertyChanging("WorkflowSuspended");
				this.SetAttributeValue("workflowsuspended", value);
				this.OnPropertyChanged("WorkflowSuspended");
			}
		}

		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		public BusinessUnit(object anonymousType) :
				this()
		{
			foreach (var p in anonymousType.GetType().GetProperties())
			{
				var value = p.GetValue(anonymousType, null);
				var name = p.Name.ToLower();

				if (name.EndsWith("enum") && value.GetType().BaseType == typeof(System.Enum))
				{
					value = new Microsoft.Xrm.Sdk.OptionSetValue((int)value);
					name = name.Remove(name.Length - "enum".Length);
				}

				switch (name)
				{
					case "id":
						base.Id = (System.Guid)value;
						Attributes["businessunitid"] = base.Id;
						break;
					case "businessunitid":
						var id = (System.Nullable<System.Guid>)value;
						if (id == null) { continue; }
						base.Id = id.Value;
						Attributes[name] = base.Id;
						break;
					case "formattedvalues":
						// Add Support for FormattedValues
						FormattedValues.AddRange((Microsoft.Xrm.Sdk.FormattedValueCollection)value);
						break;
					default:
						Attributes[name] = value;
						break;
				}
			}
		}
	}
}
// ReSharper restore InconsistentNaming

#endregion Entity Definitions