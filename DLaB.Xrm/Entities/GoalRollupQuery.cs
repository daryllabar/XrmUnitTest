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
	
	[System.Runtime.Serialization.DataContractAttribute()]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "7.0.0001.0117")]
	public enum GoalRollupQueryState
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Active = 0,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Inactive = 1,
	}
	
	/// <summary>
	/// Query that is used to filter the results of the goal rollup.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("goalrollupquery")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "7.0.0001.0117")]
	public partial class GoalRollupQuery : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		public struct Fields
		{
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string FetchXml = "fetchxml";
			public const string GoalRollupQueryId = "goalrollupqueryid";
			public const string Id = "goalrollupqueryid";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string Name = "name";
			public const string OverriddenCreatedOn = "overriddencreatedon";
			public const string OwnerId = "ownerid";
			public const string OwningBusinessUnit = "owningbusinessunit";
			public const string OwningTeam = "owningteam";
			public const string OwningUser = "owninguser";
			public const string QueryEntityType = "queryentitytype";
			public const string StateCode = "statecode";
			public const string StatusCode = "statuscode";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
			public const string business_unit_goalrollupquery = "owningbusinessunit";
			public const string lk_goalrollupquery_createdby = "createdby";
			public const string lk_goalrollupquery_createdonbehalfby = "createdonbehalfby";
			public const string lk_goalrollupquery_modifiedby = "modifiedby";
			public const string lk_goalrollupquery_modifiedonbehalfby = "modifiedonbehalfby";
			public const string team_goalrollupquery = "owningteam";
		}

		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public GoalRollupQuery() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "goalrollupquery";
		
		public const int EntityTypeCode = 9602;
		
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
		/// Shows who created the record.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedBy
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
			}
		}
		
		/// <summary>
		/// Shows the date and time when the record was created. The date and time are displayed in the time zone selected in Microsoft Dynamics CRM options.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
		public System.Nullable<System.DateTime> CreatedOn
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
			}
		}
		
		/// <summary>
		/// Shows who created the record on behalf of another user.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
			}
		}
		
		/// <summary>
		/// String that specifies the condition criteria in FetchXML.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("fetchxml")]
		public string FetchXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("fetchxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("FetchXml");
				this.SetAttributeValue("fetchxml", value);
				this.OnPropertyChanged("FetchXml");
			}
		}
		
		/// <summary>
		/// Unique identifier of the rollup query.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("goalrollupqueryid")]
		public System.Nullable<System.Guid> GoalRollupQueryId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("goalrollupqueryid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("GoalRollupQueryId");
				this.SetAttributeValue("goalrollupqueryid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("GoalRollupQueryId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("goalrollupqueryid")]
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
				this.GoalRollupQueryId = value;
			}
		}
		
		/// <summary>
		/// Sequence number of the import that created this record.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("importsequencenumber")]
		public System.Nullable<int> ImportSequenceNumber
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("importsequencenumber");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ImportSequenceNumber");
				this.SetAttributeValue("importsequencenumber", value);
				this.OnPropertyChanged("ImportSequenceNumber");
			}
		}
		
		/// <summary>
		/// Shows who last updated the record.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
			}
		}
		
		/// <summary>
		/// Shows the date and time when the record was last updated. The date and time are displayed in the time zone selected in Microsoft Dynamics CRM options.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
		public System.Nullable<System.DateTime> ModifiedOn
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
			}
		}
		
		/// <summary>
		/// Shows who last updated the record on behalf of another user.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
			}
		}
		
		/// <summary>
		/// Type a descriptive name for the goal rollup query.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("name")]
		public string Name
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("name");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("Name");
				this.SetAttributeValue("name", value);
				this.OnPropertyChanged("Name");
			}
		}
		
		/// <summary>
		/// Date and time that the record was migrated.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overriddencreatedon")]
		public System.Nullable<System.DateTime> OverriddenCreatedOn
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("OverriddenCreatedOn");
				this.SetAttributeValue("overriddencreatedon", value);
				this.OnPropertyChanged("OverriddenCreatedOn");
			}
		}
		
		/// <summary>
		/// Enter the user or team who is assigned to manage the record. This field is updated every time the record is assigned to a different user.
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
		/// Unique identifier of the business unit that owns the record.
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
		/// Unique identifier of the team who owns the record.
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
		/// Unique identifier of the user who owns the record.
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
		/// Enter the record type of the rollup query.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("queryentitytype")]
		public string QueryEntityType
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("queryentitytype");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("QueryEntityType");
				this.SetAttributeValue("queryentitytype", value);
				this.OnPropertyChanged("QueryEntityType");
			}
		}
		
		/// <summary>
		/// Shows whether the goal rollup query is active or inactive.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
		public System.Nullable<DLaB.Xrm.Entities.GoalRollupQueryState> StateCode
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
				if ((optionSet != null))
				{
					return ((DLaB.Xrm.Entities.GoalRollupQueryState)(System.Enum.ToObject(typeof(DLaB.Xrm.Entities.GoalRollupQueryState), optionSet.Value)));
				}
				else
				{
					return null;
				}
			}
		}
		
		/// <summary>
		/// Select the goal rollup query's status.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
		public Microsoft.Xrm.Sdk.OptionSetValue StatusCode
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("StatusCode");
				this.SetAttributeValue("statuscode", value);
				this.OnPropertyChanged("StatusCode");
			}
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timezoneruleversionnumber")]
		public System.Nullable<int> TimeZoneRuleVersionNumber
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("timezoneruleversionnumber");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TimeZoneRuleVersionNumber");
				this.SetAttributeValue("timezoneruleversionnumber", value);
				this.OnPropertyChanged("TimeZoneRuleVersionNumber");
			}
		}
		
		/// <summary>
		/// Time zone code that was in use when the record was created.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("utcconversiontimezonecode")]
		public System.Nullable<int> UTCConversionTimeZoneCode
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("utcconversiontimezonecode");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("UTCConversionTimeZoneCode");
				this.SetAttributeValue("utcconversiontimezonecode", value);
				this.OnPropertyChanged("UTCConversionTimeZoneCode");
			}
		}
		
		/// <summary>
		/// Version number of the goal rollup query.
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
		/// 1:N goal_rollupquery_actualdecimal
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_actualdecimal")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_actualdecimal
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_actualdecimal", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_actualdecimal");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_actualdecimal", null, value);
				this.OnPropertyChanged("goal_rollupquery_actualdecimal");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_actualmoney
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_actualmoney")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_actualmoney
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_actualmoney", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_actualmoney");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_actualmoney", null, value);
				this.OnPropertyChanged("goal_rollupquery_actualmoney");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_customdecimal
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_customdecimal")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_customdecimal
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_customdecimal", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_customdecimal");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_customdecimal", null, value);
				this.OnPropertyChanged("goal_rollupquery_customdecimal");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_customint
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_customint")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_customint
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_customint", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_customint");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_customint", null, value);
				this.OnPropertyChanged("goal_rollupquery_customint");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_custommoney
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_custommoney")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_custommoney
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_custommoney", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_custommoney");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_custommoney", null, value);
				this.OnPropertyChanged("goal_rollupquery_custommoney");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_inprogressdecimal
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_inprogressdecimal")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_inprogressdecimal
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressdecimal", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_inprogressdecimal");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressdecimal", null, value);
				this.OnPropertyChanged("goal_rollupquery_inprogressdecimal");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_inprogressint
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_inprogressint")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_inprogressint
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressint", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_inprogressint");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressint", null, value);
				this.OnPropertyChanged("goal_rollupquery_inprogressint");
			}
		}
		
		/// <summary>
		/// 1:N goal_rollupquery_inprogressmoney
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goal_rollupquery_inprogressmoney")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goal_rollupquery_inprogressmoney
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressmoney", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goal_rollupquery_inprogressmoney");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goal_rollupquery_inprogressmoney", null, value);
				this.OnPropertyChanged("goal_rollupquery_inprogressmoney");
			}
		}
		
		/// <summary>
		/// 1:N goalrollupquery_actualint
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goalrollupquery_actualint")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Goal> goalrollupquery_actualint
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Goal>("goalrollupquery_actualint", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goalrollupquery_actualint");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Goal>("goalrollupquery_actualint", null, value);
				this.OnPropertyChanged("goalrollupquery_actualint");
			}
		}
		
		/// <summary>
		/// 1:N goalrollupquery_AsyncOperations
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goalrollupquery_AsyncOperations")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.AsyncOperation> goalrollupquery_AsyncOperations
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.AsyncOperation>("goalrollupquery_AsyncOperations", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goalrollupquery_AsyncOperations");
				this.SetRelatedEntities<DLaB.Xrm.Entities.AsyncOperation>("goalrollupquery_AsyncOperations", null, value);
				this.OnPropertyChanged("goalrollupquery_AsyncOperations");
			}
		}
		
		/// <summary>
		/// 1:N GoalRollupQuery_DuplicateBaseRecord
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("GoalRollupQuery_DuplicateBaseRecord")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.DuplicateRecord> GoalRollupQuery_DuplicateBaseRecord
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.DuplicateRecord>("GoalRollupQuery_DuplicateBaseRecord", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("GoalRollupQuery_DuplicateBaseRecord");
				this.SetRelatedEntities<DLaB.Xrm.Entities.DuplicateRecord>("GoalRollupQuery_DuplicateBaseRecord", null, value);
				this.OnPropertyChanged("GoalRollupQuery_DuplicateBaseRecord");
			}
		}
		
		/// <summary>
		/// 1:N GoalRollupQuery_DuplicateMatchingRecord
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("GoalRollupQuery_DuplicateMatchingRecord")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.DuplicateRecord> GoalRollupQuery_DuplicateMatchingRecord
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.DuplicateRecord>("GoalRollupQuery_DuplicateMatchingRecord", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("GoalRollupQuery_DuplicateMatchingRecord");
				this.SetRelatedEntities<DLaB.Xrm.Entities.DuplicateRecord>("GoalRollupQuery_DuplicateMatchingRecord", null, value);
				this.OnPropertyChanged("GoalRollupQuery_DuplicateMatchingRecord");
			}
		}
		
		/// <summary>
		/// 1:N goalrollupquery_ProcessSessions
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("goalrollupquery_ProcessSessions")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.ProcessSession> goalrollupquery_ProcessSessions
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.ProcessSession>("goalrollupquery_ProcessSessions", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("goalrollupquery_ProcessSessions");
				this.SetRelatedEntities<DLaB.Xrm.Entities.ProcessSession>("goalrollupquery_ProcessSessions", null, value);
				this.OnPropertyChanged("goalrollupquery_ProcessSessions");
			}
		}
		
		/// <summary>
		/// 1:N userentityinstancedata_goalrollupquery
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityinstancedata_goalrollupquery")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.UserEntityInstanceData> userentityinstancedata_goalrollupquery
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_goalrollupquery", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("userentityinstancedata_goalrollupquery");
				this.SetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_goalrollupquery", null, value);
				this.OnPropertyChanged("userentityinstancedata_goalrollupquery");
			}
		}
		
		/// <summary>
		/// N:1 business_unit_goalrollupquery
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_goalrollupquery")]
		public DLaB.Xrm.Entities.BusinessUnit business_unit_goalrollupquery
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.BusinessUnit>("business_unit_goalrollupquery", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_goalrollupquery_createdby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_goalrollupquery_createdby")]
		public DLaB.Xrm.Entities.SystemUser lk_goalrollupquery_createdby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_goalrollupquery_createdby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_goalrollupquery_createdonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_goalrollupquery_createdonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_goalrollupquery_createdonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_goalrollupquery_createdonbehalfby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_goalrollupquery_modifiedby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_goalrollupquery_modifiedby")]
		public DLaB.Xrm.Entities.SystemUser lk_goalrollupquery_modifiedby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_goalrollupquery_modifiedby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_goalrollupquery_modifiedonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_goalrollupquery_modifiedonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_goalrollupquery_modifiedonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_goalrollupquery_modifiedonbehalfby", null);
			}
		}
		
		/// <summary>
		/// N:1 team_goalrollupquery
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_goalrollupquery")]
		public DLaB.Xrm.Entities.Team team_goalrollupquery
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.Team>("team_goalrollupquery", null);
			}
		}
		
		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public GoalRollupQuery(object anonymousType) : 
				this()
		{
            foreach (var p in anonymousType.GetType().GetProperties())
            {
                var value = p.GetValue(anonymousType, null);
                if (p.PropertyType == typeof(System.Guid))
                {
                    // Type is Guid, must be Id
                    base.Id = (System.Guid)value;
                    Attributes["goalrollupqueryid"] = base.Id;
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