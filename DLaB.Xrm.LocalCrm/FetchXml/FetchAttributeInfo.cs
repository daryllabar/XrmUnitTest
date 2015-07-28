using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLaB.Xrm.LocalCrm.FetchXml
{
    public class FetchAttributeInfo
    {
        public FetchAttributeType Attribute { get; set; }
        public string EntityLogicalName { get; set; }

        public FetchAttributeInfo(string entityLogicalName, FetchAttributeType attribute)
        {
            Attribute = attribute;
            EntityLogicalName = entityLogicalName;
        }
    }
}
