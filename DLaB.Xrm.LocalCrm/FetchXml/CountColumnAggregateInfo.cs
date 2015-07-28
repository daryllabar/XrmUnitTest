using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.FetchXml
{
    public class CountColumnAggregateInfo
    {
        public string Key { get; set; }
        public Entity Entity { get; set; }
        public int  Count { get; set; }
    }
}
