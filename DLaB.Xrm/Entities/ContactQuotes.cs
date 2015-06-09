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
	/// 
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("contactquotes")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "7.0.0001.0117")]
	public partial class ContactQuotes : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		public struct Fields
		{
			public const string ContactId = "contactid";
			public const string ContactQuoteId = "contactquoteid";
			public const string Id = "contactquoteid";
			public const string QuoteId = "quoteid";
			public const string VersionNumber = "versionnumber";
		}

		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public ContactQuotes() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "contactquotes";
		
		public const int EntityTypeCode = 18;
		
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
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contactid")]
		public System.Nullable<System.Guid> ContactId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("contactid");
			}
		}
		
		/// <summary>
		/// Unique identifier of the quotes for the contact.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contactquoteid")]
		public System.Nullable<System.Guid> ContactQuoteId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("contactquoteid");
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("ContactQuoteId");
				this.SetAttributeValue("contactquoteid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("ContactQuoteId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("contactquoteid")]
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
				this.ContactQuoteId = value;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("quoteid")]
		public System.Nullable<System.Guid> QuoteId
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("quoteid");
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
		/// N:N contactquotes_association
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("contactquotes_association")]
		public System.Collections.Generic.IEnumerable<DLaB.Xrm.Entities.Quote> contactquotes_association
		{
			[System.Diagnostics.DebuggerNonUserCode()]
			get
			{
				return this.GetRelatedEntities<DLaB.Xrm.Entities.Quote>("contactquotes_association", null);
			}
			[System.Diagnostics.DebuggerNonUserCode()]
			set
			{
				this.OnPropertyChanging("contactquotes_association");
				this.SetRelatedEntities<DLaB.Xrm.Entities.Quote>("contactquotes_association", null, value);
				this.OnPropertyChanged("contactquotes_association");
			}
		}
		
		/// <summary>
		/// Constructor for populating via LINQ queries given a LINQ anonymous type
		/// <param name="anonymousType">LINQ anonymous type.</param>
		/// </summary>
		[System.Diagnostics.DebuggerNonUserCode()]
		public ContactQuotes(object anonymousType) : 
				this()
		{
            foreach (var p in anonymousType.GetType().GetProperties())
            {
                var value = p.GetValue(anonymousType, null);
                if (p.PropertyType == typeof(System.Guid))
                {
                    // Type is Guid, must be Id
                    base.Id = (System.Guid)value;
                    Attributes["contactquoteid"] = base.Id;
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