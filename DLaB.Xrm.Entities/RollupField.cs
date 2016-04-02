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
	/// Field to be rolled up to calculate the actual and in-progress values against the goal.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("rollupfield")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "8.0.1.7297")]
	public partial class RollupField : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		public struct Fields
		{
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string DateAttribute = "dateattribute";
			public const string EntityForDateAttribute = "entityfordateattribute";
			public const string GoalAttribute = "goalattribute";
			public const string ImportSequenceNumber = "importsequencenumber";
			public const string IsStateParentEntityAttribute = "isstateparententityattribute";
			public const string MetricId = "metricid";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string OrganizationId = "organizationid";
			public const string RollupFieldId = "rollupfieldid";
			public const string Id = "rollupfieldid";
			public const string SourceAttribute = "sourceattribute";
			public const string SourceEntity = "sourceentity";
			public const string SourceState = "sourcestate";
			public const string SourceStatus = "sourcestatus";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string VersionNumber = "versionnumber";
			public const string lk_rollupfield_createdby = "lk_rollupfield_createdby";
			public const string lk_rollupfield_createdonbehalfby = "lk_rollupfield_createdonbehalfby";
			public const string lk_rollupfield_modifiedby = "lk_rollupfield_modifiedby";
			public const string lk_rollupfield_modifiedonbehalfby = "lk_rollupfield_modifiedonbehalfby";
			public const string metric_rollupfield = "metric_rollupfield";
		}

		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public RollupField() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "rollupfield";
		
		public const int EntityTypeCode = 9604;
		
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("CreatedBy");
				this.SetAttributeValue("createdby", value);
				this.OnPropertyChanged("CreatedBy");
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("CreatedOn");
				this.SetAttributeValue("createdon", value);
				this.OnPropertyChanged("CreatedOn");
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("CreatedOnBehalfBy");
				this.SetAttributeValue("createdonbehalfby", value);
				this.OnPropertyChanged("CreatedOnBehalfBy");
			}
		}
		
		/// <summary>
		/// Select a date field for the selected record type, such as Actual Closed Date for the Opportunity record type. A record participates in the goal rollup, if the selected date falls between the start date and the end date for the goal.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("dateattribute")]
		public string DateAttribute
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("dateattribute");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("DateAttribute");
				this.SetAttributeValue("dateattribute", value);
				this.OnPropertyChanged("DateAttribute");
			}
		}
		
		/// <summary>
		/// Select the record type that contains the date field that will be considered while rolling up data to the goal.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("entityfordateattribute")]
		public string EntityForDateAttribute
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("entityfordateattribute");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("EntityForDateAttribute");
				this.SetAttributeValue("entityfordateattribute", value);
				this.OnPropertyChanged("EntityForDateAttribute");
			}
		}
		
		/// <summary>
		/// Select a rollup field where the metric rollup data will be displayed in the goal. The options are an integer or money, depending on the Metric Type you chose for the goal metric.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("goalattribute")]
		public string GoalAttribute
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("goalattribute");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("GoalAttribute");
				this.SetAttributeValue("goalattribute", value);
				this.OnPropertyChanged("GoalAttribute");
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
		/// Tells whether the state or status belongs to the parent entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isstateparententityattribute")]
		public System.Nullable<bool> IsStateParentEntityAttribute
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("isstateparententityattribute");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("IsStateParentEntityAttribute");
				this.SetAttributeValue("isstateparententityattribute", value);
				this.OnPropertyChanged("IsStateParentEntityAttribute");
			}
		}
		
		/// <summary>
		/// Unique identifier of the goal metric associated with the rollup field.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("metricid")]
		public Microsoft.Xrm.Sdk.EntityReference MetricId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("metricid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("MetricId");
				this.SetAttributeValue("metricid", value);
				this.OnPropertyChanged("MetricId");
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ModifiedBy");
				this.SetAttributeValue("modifiedby", value);
				this.OnPropertyChanged("ModifiedBy");
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ModifiedOn");
				this.SetAttributeValue("modifiedon", value);
				this.OnPropertyChanged("ModifiedOn");
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
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ModifiedOnBehalfBy");
				this.SetAttributeValue("modifiedonbehalfby", value);
				this.OnPropertyChanged("ModifiedOnBehalfBy");
			}
		}
		
		/// <summary>
		/// Choose the ID of the organization that the record is associated with.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
		public Microsoft.Xrm.Sdk.EntityReference OrganizationId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("organizationid");
			}
		}
		
		/// <summary>
		/// Unique identifier of the rollup field.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("rollupfieldid")]
		public System.Nullable<System.Guid> RollupFieldId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("rollupfieldid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("RollupFieldId");
				this.SetAttributeValue("rollupfieldid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("RollupFieldId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("rollupfieldid")]
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
				this.RollupFieldId = value;
			}
		}
		
		/// <summary>
		/// Type the name of the field that the data for the goal rolls up from.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourceattribute")]
		public string SourceAttribute
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("sourceattribute");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("SourceAttribute");
				this.SetAttributeValue("sourceattribute", value);
				this.OnPropertyChanged("SourceAttribute");
			}
		}
		
		/// <summary>
		/// Type the name of the record type (entity) that the data for the goal must roll up from.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourceentity")]
		public string SourceEntity
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("sourceentity");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("SourceEntity");
				this.SetAttributeValue("sourceentity", value);
				this.OnPropertyChanged("SourceEntity");
			}
		}
		
		/// <summary>
		/// Select the state of the records you want to use as the source of the rollup data for the metric.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourcestate")]
		public System.Nullable<int> SourceState
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("sourcestate");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("SourceState");
				this.SetAttributeValue("sourcestate", value);
				this.OnPropertyChanged("SourceState");
			}
		}
		
		/// <summary>
		/// Select the status of the records you want to use as the source of the rollup data for the metric.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourcestatus")]
		public System.Nullable<int> SourceStatus
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("sourcestatus");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("SourceStatus");
				this.SetAttributeValue("sourcestatus", value);
				this.OnPropertyChanged("SourceStatus");
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
		/// Version number of the rollup field.
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
		/// 1:N rollupfield_AsyncOperations
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("rollupfield_AsyncOperations")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.AsyncOperation> rollupfield_AsyncOperations
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.AsyncOperation>("rollupfield_AsyncOperations", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("rollupfield_AsyncOperations");
				this.SetRelatedEntities<DLaB.Xrm.Entities.AsyncOperation>("rollupfield_AsyncOperations", null, value);
				this.OnPropertyChanged("rollupfield_AsyncOperations");
			}
		}
		
		/// <summary>
		/// 1:N rollupfield_ProcessSessions
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("rollupfield_ProcessSessions")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.ProcessSession> rollupfield_ProcessSessions
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.ProcessSession>("rollupfield_ProcessSessions", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("rollupfield_ProcessSessions");
				this.SetRelatedEntities<DLaB.Xrm.Entities.ProcessSession>("rollupfield_ProcessSessions", null, value);
				this.OnPropertyChanged("rollupfield_ProcessSessions");
			}
		}
		
		/// <summary>
		/// 1:N userentityinstancedata_rollupfield
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityinstancedata_rollupfield")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.UserEntityInstanceData> userentityinstancedata_rollupfield
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_rollupfield", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("userentityinstancedata_rollupfield");
				this.SetRelatedEntities<DLaB.Xrm.Entities.UserEntityInstanceData>("userentityinstancedata_rollupfield", null, value);
				this.OnPropertyChanged("userentityinstancedata_rollupfield");
			}
		}
		
		/// <summary>
		/// N:1 lk_rollupfield_createdby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_rollupfield_createdby")]
		public DLaB.Xrm.Entities.SystemUser lk_rollupfield_createdby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_createdby", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("lk_rollupfield_createdby");
				this.SetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_createdby", null, value);
				this.OnPropertyChanged("lk_rollupfield_createdby");
			}
		}
		
		/// <summary>
		/// N:1 lk_rollupfield_createdonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_rollupfield_createdonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_rollupfield_createdonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_createdonbehalfby", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("lk_rollupfield_createdonbehalfby");
				this.SetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_createdonbehalfby", null, value);
				this.OnPropertyChanged("lk_rollupfield_createdonbehalfby");
			}
		}
		
		/// <summary>
		/// N:1 lk_rollupfield_modifiedby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_rollupfield_modifiedby")]
		public DLaB.Xrm.Entities.SystemUser lk_rollupfield_modifiedby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_modifiedby", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("lk_rollupfield_modifiedby");
				this.SetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_modifiedby", null, value);
				this.OnPropertyChanged("lk_rollupfield_modifiedby");
			}
		}
		
		/// <summary>
		/// N:1 lk_rollupfield_modifiedonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_rollupfield_modifiedonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_rollupfield_modifiedonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_modifiedonbehalfby", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("lk_rollupfield_modifiedonbehalfby");
				this.SetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_rollupfield_modifiedonbehalfby", null, value);
				this.OnPropertyChanged("lk_rollupfield_modifiedonbehalfby");
			}
		}
		
		/// <summary>
		/// N:1 metric_rollupfield
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("metricid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("metric_rollupfield")]
		public DLaB.Xrm.Entities.Metric metric_rollupfield
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.Metric>("metric_rollupfield", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("metric_rollupfield");
				this.SetRelatedEntity<DLaB.Xrm.Entities.Metric>("metric_rollupfield", null, value);
				this.OnPropertyChanged("metric_rollupfield");
			}
		}
		
		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public RollupField(object anonymousType) : 
				this()
		{
            foreach (var p in anonymousType.GetType().GetProperties())
            {
                var value = p.GetValue(anonymousType, null);
                var name = p.Name.ToLower();
            
                if (name.EndsWith("enum") && value.GetType().BaseType == typeof(System.Enum))
                {
                    value = new Microsoft.Xrm.Sdk.OptionSetValue((int) value);
                    name = name.Remove(name.Length - "enum".Length);
                }
            
                switch (name)
                {
                    case "id":
                        base.Id = (System.Guid)value;
                        Attributes["rollupfieldid"] = base.Id;
                        break;
                    case "rollupfieldid":
                        var id = (System.Nullable<System.Guid>) value;
                        if(id == null){ continue; }
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