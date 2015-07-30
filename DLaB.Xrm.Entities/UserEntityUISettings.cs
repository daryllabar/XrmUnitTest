//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DLaB.Xrm.Entities
{
	
	/// <summary>
	/// Stores user settings for entity views.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("userentityuisettings")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "7.0.0001.0117")]
	public partial class UserEntityUISettings : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		public struct Fields
		{
			public const string InsertIntoEmailMRUXml = "insertintoemailmruxml";
			public const string LastViewedFormXml = "lastviewedformxml";
			public const string LookupMRUXml = "lookupmruxml";
			public const string MRUXml = "mruxml";
			public const string ObjectTypeCode = "objecttypecode";
			public const string OwnerId = "ownerid";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string ReadingPaneXml = "readingpanexml";
			public const string RecentlyViewedXml = "recentlyviewedxml";
			public const string ShowInAddressBook = "showinaddressbook";
			public const string TabOrderXml = "taborderxml";
			public const string UserEntityUISettingsId = "userentityuisettingsid";
			public const string Id = "userentityuisettingsid";
			public const string VersionNumber = "versionnumber";
			public const string team_userentityuisettings = "owningteam";
			public const string userentityuisettings_businessunit = "owningbusinessunit";
			public const string userentityuisettings_owning_user = "owninguser";
		}

		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public UserEntityUISettings() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "userentityuisettings";
		
		public const int EntityTypeCode = 2500;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		[System.Diagnostics.DebuggerNonUserCode()]
		private void OnPropertyChanged(string propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]
		private void OnPropertyChanging(string propertyName)
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
			}
		}
		
		/// <summary>
		/// Describes which entities are most recently inserted into email for this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("insertintoemailmruxml")]
		public string InsertIntoEmailMRUXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("insertintoemailmruxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("InsertIntoEmailMRUXml");
				this.SetAttributeValue("insertintoemailmruxml", value);
				this.OnPropertyChanged("InsertIntoEmailMRUXml");
			}
		}
		
		/// <summary>
		/// Describes which forms are most recently viewed for this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lastviewedformxml")]
		public string LastViewedFormXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("lastviewedformxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("LastViewedFormXml");
				this.SetAttributeValue("lastviewedformxml", value);
				this.OnPropertyChanged("LastViewedFormXml");
			}
		}
		
		/// <summary>
		/// List of most recently used lookup references for this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("lookupmruxml")]
		public string LookupMRUXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("lookupmruxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("LookupMRUXml");
				this.SetAttributeValue("lookupmruxml", value);
				this.OnPropertyChanged("LookupMRUXml");
			}
		}
		
		/// <summary>
		/// Describes which tabs are most recently used for this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mruxml")]
		public string MRUXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("mruxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("MRUXml");
				this.SetAttributeValue("mruxml", value);
				this.OnPropertyChanged("MRUXml");
			}
		}
		
		/// <summary>
		/// Object Type Code
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("objecttypecode")]
		public System.Nullable<int> ObjectTypeCode
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("objecttypecode");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ObjectTypeCode");
				this.SetAttributeValue("objecttypecode", value);
				this.OnPropertyChanged("ObjectTypeCode");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user or team who owns the settings.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ownerid")]
		public Microsoft.Xrm.Sdk.EntityReference OwnerId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("ownerid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("OwnerId");
				this.SetAttributeValue("ownerid", value);
				this.OnPropertyChanged("OwnerId");
			}
		}
		
		/// <summary>
		/// Unique identifier of the business unit that owns this.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
		public Microsoft.Xrm.Sdk.EntityReference OwningBusinessUnit
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningbusinessunit");
			}
		}
		
		/// <summary>
		/// Unique identifier of the team who owns this saved view.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
		public Microsoft.Xrm.Sdk.EntityReference OwningTeam
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningteam");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user who owns this saved view.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
		public Microsoft.Xrm.Sdk.EntityReference OwningUser
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owninguser");
			}
		}
		
		/// <summary>
		/// Describes the reading pane formatting of this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("readingpanexml")]
		public string ReadingPaneXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("readingpanexml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ReadingPaneXml");
				this.SetAttributeValue("readingpanexml", value);
				this.OnPropertyChanged("ReadingPaneXml");
			}
		}
		
		/// <summary>
		/// Describes which objects are most recently viewed for this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("recentlyviewedxml")]
		public string RecentlyViewedXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("recentlyviewedxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("RecentlyViewedXml");
				this.SetAttributeValue("recentlyviewedxml", value);
				this.OnPropertyChanged("RecentlyViewedXml");
			}
		}
		
		/// <summary>
		/// Determines whether a record type is exposed in the Outlook Address Book
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("showinaddressbook")]
		public System.Nullable<bool> ShowInAddressBook
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("showinaddressbook");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ShowInAddressBook");
				this.SetAttributeValue("showinaddressbook", value);
				this.OnPropertyChanged("ShowInAddressBook");
			}
		}
		
		/// <summary>
		/// Describes the tab ordering for this entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("taborderxml")]
		public string TabOrderXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("taborderxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TabOrderXml");
				this.SetAttributeValue("taborderxml", value);
				this.OnPropertyChanged("TabOrderXml");
			}
		}
		
		/// <summary>
		/// Unique identifier user entity
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("userentityuisettingsid")]
		public System.Nullable<System.Guid> UserEntityUISettingsId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("userentityuisettingsid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("UserEntityUISettingsId");
				this.SetAttributeValue("userentityuisettingsid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("UserEntityUISettingsId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("userentityuisettingsid")]
		public override System.Guid Id
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return base.Id;
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.UserEntityUISettingsId = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
		public System.Nullable<long> VersionNumber
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
			}
		}
		
		/// <summary>
		/// 1:N userentityinstancedata_userentityuisettings
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityinstancedata_userentityuisettings")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.UserEntityInstanceData> userentityinstancedata_userentityuisettings
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_userentityuisettings", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("userentityinstancedata_userentityuisettings");
				this.SetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_userentityuisettings", null, value);
				this.OnPropertyChanged("userentityinstancedata_userentityuisettings");
			}
		}
		
		/// <summary>
		/// N:1 team_userentityuisettings
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_userentityuisettings")]
		public DLaB.Xrm.Entities.Team team_userentityuisettings
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.Team>("team_userentityuisettings", null);
			}
		}
		
		/// <summary>
		/// N:1 userentityuisettings_businessunit
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityuisettings_businessunit")]
		public DLaB.Xrm.Entities.BusinessUnit userentityuisettings_businessunit
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.BusinessUnit>("userentityuisettings_businessunit", null);
			}
		}
		
		/// <summary>
		/// N:1 userentityuisettings_owning_user
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityuisettings_owning_user")]
		public DLaB.Xrm.Entities.SystemUser userentityuisettings_owning_user
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("userentityuisettings_owning_user", null);
			}
		}
		
		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public UserEntityUISettings(object anonymousType) : 
				this()
		{
            foreach (var p in anonymousType.GetType().GetProperties())
            {
                var value = p.GetValue(anonymousType, null);
                if (p.PropertyType == typeof(System.Guid))
                {
                    // Type is Guid, must be Id
                    base.Id = (System.Guid)value;
                    Attributes["userentityuisettingsid"] = base.Id;
                }
                else if (p.Name == "FormattedValues")
                {
                    // Add Support for FormattedValues
                    FormattedValues.AddRange((Microsoft.Xrm.Sdk.FormattedValueCollection)value);
                }
                else
                {
                    Attributes[p.Name.ToLower()] = value;
                }
            }
		}
	}
}