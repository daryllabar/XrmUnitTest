using System;

namespace DLaB.Xrm.Client
{
    [Serializable]
    public class CrmServiceInfo
    {

        #region Public Properties

        public string CrmServerUrl { get; private set; }
               
        public string CrmDiscoveryServerUrl { get; private set; }
               
        public string CrmOrganization { get; set; }
               
        public Guid ImpersonationUserId { get; set; }
               
        public bool EnableProxyTypes { get; set; }
               
        public string UserDomainName { get; set; }
               
        public string UserName { get; set; }
               
        public string UserPassword { get; set; }
               
        public string ProxyTypeAssembly { get; set; }
               
        public TimeSpan Timeout { get; set; }

        #endregion

        public CrmServiceInfo(string crmServerUrl, string crmDiscoveryServerUrl, string crmOrganization)
        {
            CrmServerUrl = crmServerUrl;
            CrmDiscoveryServerUrl = String.IsNullOrWhiteSpace(crmDiscoveryServerUrl) ? crmServerUrl : crmDiscoveryServerUrl;
            CrmOrganization = crmOrganization;  
            EnableProxyTypes = true;
            Timeout = new TimeSpan();
        }

        /// <summary>
        /// Defaults to using the CrmServerUrl and CrmDiscoveryServerUrl stored in the App.Config App Settings
        /// </summary>
        /// <param name="crmOrganization"></param>
        public CrmServiceInfo(string crmOrganization)
            : this(crmOrganization, Guid.Empty)
        {

        }

        /// <summary>
        /// Defaults to using the CrmServerUrl and CrmDiscoveryServerUrl stored in the App.Config App Settings
        /// </summary>
        /// <param name="crmOrganization"></param>
        /// <param name="impersonationUserId"></param>
        public CrmServiceInfo(string crmOrganization, Guid impersonationUserId)
            : this(
                System.Configuration.ConfigurationManager.AppSettings["CrmServerUrl"],
                System.Configuration.ConfigurationManager.AppSettings["CrmDiscoveryServerUrl"],
                crmOrganization,
                impersonationUserId)
        {

        }

        public CrmServiceInfo(string crmServerUrl,
                                string crmDiscoveryServerUrl,
                                string crmOrganization,
                                Guid impersonationUserId)
            : this(crmServerUrl, crmDiscoveryServerUrl, crmOrganization)
        {
            ImpersonationUserId = impersonationUserId;
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // return true only if parameter can be cast to CrmServiceInfo and is equal.
            var p = obj as CrmServiceInfo;
            return p != null && Equals(p);
        }

        public bool Equals(CrmServiceInfo p)
        {
            return 
                p != null &&
                CrmServerUrl == p.CrmServerUrl && 
                CrmDiscoveryServerUrl == p.CrmDiscoveryServerUrl &&
                CrmOrganization == p.CrmOrganization &&
                ImpersonationUserId == p.ImpersonationUserId &&
                EnableProxyTypes == p.EnableProxyTypes &&
                Timeout == p.Timeout;
        }

        public override int GetHashCode()
        {
            return (CrmServerUrl + CrmDiscoveryServerUrl).GetHashCode();
        }

        public string GetOrganizationKey()
        {
            int start = CrmServerUrl.IndexOf(@"//", StringComparison.Ordinal) + 2; // @"//".Length
            int end = CrmServerUrl.LastIndexOf('/');
            if (end < start)
            {
                end = CrmServerUrl.Length;
            }

            return String.Format("{0}|{1}|",
                CrmServerUrl.Substring(start, end - start).ToLower(),
                CrmOrganization.ToLower());
        }
    }
}
