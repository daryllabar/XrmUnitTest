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
	/// A trace log.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("tracelog")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "7.0.0001.0117")]
	public partial class TraceLog : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		public struct Fields
		{
			public const string CanBeDeleted = "canbedeleted";
			public const string CollationLevel = "collationlevel";
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string IsUnique = "isunique";
			public const string Level = "level";
			public const string MachineName = "machinename";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string OrganizationId = "organizationid";
			public const string ParentTraceLogId = "parenttracelogid";
			public const string RegardingObjectId = "regardingobjectid";
			public const string RegardingObjectOwnerId = "regardingobjectownerid";
			public const string RegardingObjectOwningBusinessUnit = "regardingobjectowningbusinessunit";
			public const string Text = "text";
			public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
			public const string TraceActionXml = "traceactionxml";
			public const string TraceCode = "tracecode";
			public const string TraceDetailXml = "tracedetailxml";
			public const string TraceLogId = "tracelogid";
			public const string Id = "tracelogid";
			public const string TraceParameterHash = "traceparameterhash";
			public const string TraceParameterXml = "traceparameterxml";
			public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
			public const string lk_tracelog_createdby = "createdby";
			public const string lk_tracelog_createdonbehalfby = "createdonbehalfby";
			public const string lk_tracelog_modifiedby = "modifiedby";
			public const string lk_tracelog_modifiedonbehalfby = "modifiedonbehalfby";
			public const string organization_tracelog = "organizationid";
			public const string tracelog_EmailServerProfile = "regardingobjectid";
			public const string tracelog_Mailbox = "regardingobjectid";
			public const string Referencingtracelog_parent_tracelog = "parenttracelogid";
		}

		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public TraceLog() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "tracelog";
		
		public const int EntityTypeCode = 8050;
		
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
		/// Indicates if this trace log can be deleted.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("canbedeleted")]
		public System.Nullable<bool> CanBeDeleted
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("canbedeleted");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("CanBeDeleted");
				this.SetAttributeValue("canbedeleted", value);
				this.OnPropertyChanged("CanBeDeleted");
			}
		}
		
		/// <summary>
		/// Indicates the collation level
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("collationlevel")]
		public System.Nullable<int> CollationLevel
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("collationlevel");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("CollationLevel");
				this.SetAttributeValue("collationlevel", value);
				this.OnPropertyChanged("CollationLevel");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user who created the trace.
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
		/// Date and time when the trace was created.
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
		/// Unique identifier of the delegate user who created the trace.
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
		/// Tells if this traceLog is created uniquely(only one) for the associated entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isunique")]
		public System.Nullable<bool> IsUnique
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("isunique");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("IsUnique");
				this.SetAttributeValue("isunique", value);
				this.OnPropertyChanged("IsUnique");
			}
		}
		
		/// <summary>
		/// Information about the trace level.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("level")]
		public Microsoft.Xrm.Sdk.OptionSetValue Level
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("level");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("Level");
				this.SetAttributeValue("level", value);
				this.OnPropertyChanged("Level");
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("machinename")]
		public string MachineName
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("machinename");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("MachineName");
				this.SetAttributeValue("machinename", value);
				this.OnPropertyChanged("MachineName");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user who modified the trace.
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
		/// Date and time when the trace was modified.
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
		/// Unique identifier of the delegate user who modified the trace.
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
		/// Unique identifier of the organization associated with the trace.
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
		/// Indicates the parent ID of the trace log.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("parenttracelogid")]
		public Microsoft.Xrm.Sdk.EntityReference ParentTraceLogId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("parenttracelogid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ParentTraceLogId");
				this.SetAttributeValue("parenttracelogid", value);
				this.OnPropertyChanged("ParentTraceLogId");
			}
		}
		
		/// <summary>
		/// Unique identifier of the object the trace is regarding.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("regardingobjectid")]
		public Microsoft.Xrm.Sdk.EntityReference RegardingObjectId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("regardingobjectid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("RegardingObjectId");
				this.SetAttributeValue("regardingobjectid", value);
				this.OnPropertyChanged("RegardingObjectId");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user or team who owns the regarding object.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("regardingobjectownerid")]
		public Microsoft.Xrm.Sdk.EntityReference RegardingObjectOwnerId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("regardingobjectownerid");
			}
		}
		
		/// <summary>
		/// Unique identifier of the business unit that owns the regarding object.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("regardingobjectowningbusinessunit")]
		public Microsoft.Xrm.Sdk.EntityReference RegardingObjectOwningBusinessUnit
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("regardingobjectowningbusinessunit");
			}
		}
		
		/// <summary>
		/// Text of the trace.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("text")]
		public string Text
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("text");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("Text");
				this.SetAttributeValue("text", value);
				this.OnPropertyChanged("Text");
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
		/// XML representation of the trace actions.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("traceactionxml")]
		public string TraceActionXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("traceactionxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TraceActionXml");
				this.SetAttributeValue("traceactionxml", value);
				this.OnPropertyChanged("TraceActionXml");
			}
		}
		
		/// <summary>
		/// Trace code for the trace.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("tracecode")]
		public System.Nullable<int> TraceCode
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("tracecode");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TraceCode");
				this.SetAttributeValue("tracecode", value);
				this.OnPropertyChanged("TraceCode");
			}
		}
		
		/// <summary>
		/// XML representation of the trace details.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("tracedetailxml")]
		public string TraceDetailXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("tracedetailxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TraceDetailXml");
				this.SetAttributeValue("tracedetailxml", value);
				this.OnPropertyChanged("TraceDetailXml");
			}
		}
		
		/// <summary>
		/// Unique identifier of the trace.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("tracelogid")]
		public System.Nullable<System.Guid> TraceLogId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("tracelogid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TraceLogId");
				this.SetAttributeValue("tracelogid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("TraceLogId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("tracelogid")]
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
				this.TraceLogId = value;
			}
		}
		
		/// <summary>
		/// Stores the hash of the entity object associated with this tracelog. Hash is computed using the object type code and its id.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("traceparameterhash")]
		public System.Nullable<int> TraceParameterHash
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("traceparameterhash");
			}
		}
		
		/// <summary>
		/// XML representation of the trace parameters.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("traceparameterxml")]
		public string TraceParameterXml
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<string>("traceparameterxml");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("TraceParameterXml");
				this.SetAttributeValue("traceparameterxml", value);
				this.OnPropertyChanged("TraceParameterXml");
			}
		}
		
		/// <summary>
		/// Time zone code that was in use when the trace was created.
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
		/// 1:N tracelog_parent_tracelog
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referenced)]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.TraceLog> Referencedtracelog_parent_tracelog
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.TraceLog>("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referenced);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("Referencedtracelog_parent_tracelog");
				this.SetRelatedEntities<DLaB.Xrm.Entities.TraceLog>("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referenced, value);
				this.OnPropertyChanged("Referencedtracelog_parent_tracelog");
			}
		}
		
		/// <summary>
		/// N:1 lk_tracelog_createdby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_tracelog_createdby")]
		public DLaB.Xrm.Entities.SystemUser lk_tracelog_createdby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_tracelog_createdby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_tracelog_createdonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_tracelog_createdonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_tracelog_createdonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_tracelog_createdonbehalfby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_tracelog_modifiedby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_tracelog_modifiedby")]
		public DLaB.Xrm.Entities.SystemUser lk_tracelog_modifiedby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_tracelog_modifiedby", null);
			}
		}
		
		/// <summary>
		/// N:1 lk_tracelog_modifiedonbehalfby
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_tracelog_modifiedonbehalfby")]
		public DLaB.Xrm.Entities.SystemUser lk_tracelog_modifiedonbehalfby
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.SystemUser>("lk_tracelog_modifiedonbehalfby", null);
			}
		}
		
		/// <summary>
		/// N:1 organization_tracelog
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("organization_tracelog")]
		public DLaB.Xrm.Entities.Organization organization_tracelog
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.Organization>("organization_tracelog", null);
			}
		}
		
		/// <summary>
		/// N:1 tracelog_EmailServerProfile
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("regardingobjectid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("tracelog_EmailServerProfile")]
		public DLaB.Xrm.Entities.EmailServerProfile tracelog_EmailServerProfile
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.EmailServerProfile>("tracelog_EmailServerProfile", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("tracelog_EmailServerProfile");
				this.SetRelatedEntity<DLaB.Xrm.Entities.EmailServerProfile>("tracelog_EmailServerProfile", null, value);
				this.OnPropertyChanged("tracelog_EmailServerProfile");
			}
		}
		
		/// <summary>
		/// N:1 tracelog_Mailbox
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("regardingobjectid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("tracelog_Mailbox")]
		public DLaB.Xrm.Entities.Mailbox tracelog_Mailbox
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.Mailbox>("tracelog_Mailbox", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("tracelog_Mailbox");
				this.SetRelatedEntity<DLaB.Xrm.Entities.Mailbox>("tracelog_Mailbox", null, value);
				this.OnPropertyChanged("tracelog_Mailbox");
			}
		}
		
		/// <summary>
		/// N:1 tracelog_parent_tracelog
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("parenttracelogid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referencing)]
		public DLaB.Xrm.Entities.TraceLog Referencingtracelog_parent_tracelog
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntity<DLaB.Xrm.Entities.TraceLog>("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referencing);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("Referencingtracelog_parent_tracelog");
				this.SetRelatedEntity<DLaB.Xrm.Entities.TraceLog>("tracelog_parent_tracelog", Microsoft.Xrm.Sdk.EntityRole.Referencing, value);
				this.OnPropertyChanged("Referencingtracelog_parent_tracelog");
			}
		}
		
		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public TraceLog(object anonymousType) : 
				this()
		{
            foreach (var p in anonymousType.GetType().GetProperties())
            {
                var value = p.GetValue(anonymousType, null);
                if (p.PropertyType == typeof(System.Guid))
                {
                    // Type is Guid, must be Id
                    base.Id = (System.Guid)value;
                    Attributes["tracelogid"] = base.Id;
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
		
		public virtual tracelog_level? LevelEnum
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return ((tracelog_level?)(EntityOptionSetEnum.GetEnum(this, "level")));
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				Level = value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null;
			}
		}
	}
}