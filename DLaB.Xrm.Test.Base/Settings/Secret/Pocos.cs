using System.Collections.Generic;

namespace DLaB.Xrm.Test.Settings.Secret
{
    /* Generated From:
{
  "DataverseUnitTestSettings": { 
    "UseLocalCrmDatabase": true,
    "Connection": "Dev",
    "Connections": [
      { "Name": "Dev",   "Value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://dev.contoso.crm.dynamics.com" },
      { "Name": "Qa",    "Value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://qa.contoso.crm.dynamics.com" },
      { "Name": "Stage", "Value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://stage.contoso.crm.dynamics.com" },
      { "Name": "Prod",  "Value": "AuthType=Office365;Username=jsmith@contoso.onmicrosoft.com; Url=https://prod.contoso.crm.dynamics.com" }
    ],
    "CrmSystemSettings": {
      "FullNameFormat": "F I L"
    },
    "AppSettings": [
      {
        "Key": "YourKey",
        "Value": "YourValue"
      }
    ],
    "Password": "MyPassword",
    "Passwords": [ 
      { "Name": "Dev",   "Value": "MyPassword" },
      { "Name": "Qa",    "Value": "MyPassword" },
      { "Name": "Stage", "Value": "MyPassword" },
      { "Name": "Prod",  "Value": "MyPassword" }
    ]
    }
  }
     */
    public class AppSetting
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Connection
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class CrmSystemSettings
    {
        public string FullNameFormat { get; set; }
    }

    public class DataverseUnitTestSettings
    {
        public bool UseLocalCrmDatabase { get; set; }
        public string Connection { get; set; }
        public List<Connection> Connections { get; set; }
        public CrmSystemSettings CrmSystemSettings { get; set; }
        public List<AppSetting> AppSettings { get; set; }
        public string Password { get; set; }
        public List<Password> Passwords { get; set; }
    }

    public class Password
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
