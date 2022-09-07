using System.Collections.Generic;
// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles

namespace DLaB.Xrm.Test.Settings.Secret
{
    /* Generated From:
{
  "dataverseUnitTestSettings": { 
    "useLocalCrmDatabase": true,
    "connection": "Dev",
    "connections": [
      { "name": "Dev",   "value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://dev.contoso.crm.dynamics.com" },
      { "name": "Qa",    "value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://qa.contoso.crm.dynamics.com" },
      { "name": "Stage", "value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://stage.contoso.crm.dynamics.com" },
      { "name": "Prod",  "value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://prod.contoso.crm.dynamics.com" }
    ],
    "crmSystemSettings": {
      "fullNameFormat": "F I L"
    },
    "appSettings": [
      {
        "key": "YourKey",
        "value": "YourValue"
      }
    ],
    "password": "MyPassword",
    "passwords": [ 
      { "name": "Dev",   "value": "MyPassword" },
      { "name": "Qa",    "value": "MyPassword" },
      { "name": "Stage", "value": "MyPassword" },
      { "name": "Prod",  "value": "MyPassword" }
    ]
    }
  }
     */
    public class AppSetting
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Connection
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class CrmSystemSettings
    {
        public string fullNameFormat { get; set; }
    }

    public class DataverseUnitTestSettings
    {
        public bool useLocalCrmDatabase { get; set; }
        public string connection { get; set; }
        public List<Connection> connections { get; set; }
        public CrmSystemSettings crmSystemSettings { get; set; }
        public List<AppSetting> appSettings { get; set; }
        public string password { get; set; }
        public List<Password> passwords { get; set; }
    }

    public class Password
    {
        public string name { get; set; }

        public string value { get; set; }
    }
}
#pragma warning restore IDE1006 // Naming Styles
