namespace DLaB.Xrm.Entities
{
    public partial class Contact
    {
#if !PRE_MULTISELECT
        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendartypes")]
        public virtual System.Collections.Generic.IEnumerable<Calendar_Type> CalendarTypes
        {
            [System.Diagnostics.DebuggerNonUserCode()]
            get
            {
                return EntityOptionSetEnum.GetMultiEnum<Calendar_Type>(this, "calendartypes");
            }
            [System.Diagnostics.DebuggerNonUserCode()]
            set
            {
                this.OnPropertyChanging("CalendarTypes");
                this.SetAttributeValue("calendartypes", EntityOptionSetEnum.GetMultiEnum(this, "calendartypes", value));
                this.OnPropertyChanged("CalendarTypes");
            }
        }
#endif
    }
}
