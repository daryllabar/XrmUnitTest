namespace DLaB.Xrm.Entities
{
    public partial class Contact
    {
#if !XRM_2013 && !XRM_2015 && !XRM_2016
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
