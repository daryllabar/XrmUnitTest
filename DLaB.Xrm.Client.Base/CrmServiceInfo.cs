//using System;

//namespace DLaB.Xrm.Client
//{
//    /// <summary>
//    /// Info Class Contianing the information required to connect to CRM
//    /// </summary>
//    [Serializable]
//    public class CrmServiceInfo
//    {

//        #region Public Properties

//        /// <summary>
//        /// Gets the CRM server URL.
//        /// </summary>
//        /// <value>
//        /// The CRM server URL.
//        /// </value>
//        public string CrmServerUrl { get; }

//        /// <summary>
//        /// Gets the CRM discovery server URL.
//        /// </summary>
//        /// <value>
//        /// The CRM discovery server URL.
//        /// </value>
//        public string CrmDiscoveryServerUrl { get; }

//        /// <summary>
//        /// Gets or sets the CRM organization.
//        /// </summary>
//        /// <value>
//        /// The CRM organization.
//        /// </value>
//        public string CrmOrganization { get; set; }

//        /// <summary>
//        /// Gets or sets the impersonation user identifier.
//        /// </summary>
//        /// <value>
//        /// The impersonation user identifier.
//        /// </value>
//        public Guid ImpersonationUserId { get; set; }

//        /// <summary>
//        /// Gets or sets a value indicating whether [enable proxy types].
//        /// </summary>
//        /// <value>
//        ///   <c>true</c> if [enable proxy types]; otherwise, <c>false</c>.
//        /// </value>
//        public bool EnableProxyTypes { get; set; }

//        /// <summary>
//        /// Gets or sets the name of the user domain.
//        /// </summary>
//        /// <value>
//        /// The name of the user domain.
//        /// </value>
//        public string UserDomainName { get; set; }

//        /// <summary>
//        /// Gets or sets the name of the user.
//        /// </summary>
//        /// <value>
//        /// The name of the user.
//        /// </value>
//        public string UserName { get; set; }

//        /// <summary>
//        /// Gets or sets the user password.
//        /// </summary>
//        /// <value>
//        /// The user password.
//        /// </value>
//        public string UserPassword { get; set; }

//        /// <summary>
//        /// Gets or sets the proxy type assembly.
//        /// </summary>
//        /// <value>
//        /// The proxy type assembly.
//        /// </value>
//        public string ProxyTypeAssembly { get; set; }

//        /// <summary>
//        /// Gets or sets the timeout.
//        /// </summary>
//        /// <value>
//        /// The timeout.
//        /// </value>
//        public TimeSpan Timeout { get; set; }

//        #endregion

//        /// <summary>
//        /// Initializes a new instance of the <see cref="CrmServiceInfo"/> class.
//        /// </summary>
//        /// <param name="crmServerUrl">The CRM server URL.</param>
//        /// <param name="crmDiscoveryServerUrl">The CRM discovery server URL.</param>
//        /// <param name="crmOrganization">The CRM organization.</param>
//        public CrmServiceInfo(string crmServerUrl, string crmDiscoveryServerUrl, string crmOrganization)
//        {
//            CrmServerUrl = crmServerUrl;
//            CrmDiscoveryServerUrl = String.IsNullOrWhiteSpace(crmDiscoveryServerUrl) ? crmServerUrl : crmDiscoveryServerUrl;
//            CrmOrganization = crmOrganization;  
//            EnableProxyTypes = true;
//            Timeout = new TimeSpan();
//        }

//        /// <summary>
//        /// Defaults to using the CrmServerUrl and CrmDiscoveryServerUrl stored in the App.Config App Settings
//        /// </summary>
//        /// <param name="crmOrganization"></param>
//        public CrmServiceInfo(string crmOrganization)
//            : this(crmOrganization, Guid.Empty)
//        {

//        }

//        /// <summary>
//        /// Defaults to using the CrmServerUrl and CrmDiscoveryServerUrl stored in the App.Config App Settings
//        /// </summary>
//        /// <param name="crmOrganization"></param>
//        /// <param name="impersonationUserId"></param>
//        public CrmServiceInfo(string crmOrganization, Guid impersonationUserId)
//            : this(
//                System.Configuration.ConfigurationManager.AppSettings["CrmServerUrl"],
//                System.Configuration.ConfigurationManager.AppSettings["CrmDiscoveryServerUrl"],
//                crmOrganization,
//                impersonationUserId)
//        {

//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="CrmServiceInfo"/> class.
//        /// </summary>
//        /// <param name="crmServerUrl">The CRM server URL.</param>
//        /// <param name="crmDiscoveryServerUrl">The CRM discovery server URL.</param>
//        /// <param name="crmOrganization">The CRM organization.</param>
//        /// <param name="impersonationUserId">The impersonation user identifier.</param>
//        public CrmServiceInfo(string crmServerUrl,
//                                string crmDiscoveryServerUrl,
//                                string crmOrganization,
//                                Guid impersonationUserId)
//            : this(crmServerUrl, crmDiscoveryServerUrl, crmOrganization)
//        {
//            ImpersonationUserId = impersonationUserId;
//        }

//        /// <summary>
//        /// Determines whether the specified object is equal to the current object.
//        /// </summary>
//        /// <param name="obj">The object to compare with the current object.</param>
//        /// <returns>
//        /// true if the specified object  is equal to the current object; otherwise, false.
//        /// </returns>
//        public override bool Equals(Object obj)
//        {
//            // If parameter is null return false.
//            if (obj == null)
//            {
//                return false;
//            }

//            // return true only if parameter can be cast to CrmServiceInfo and is equal.
//            return obj is CrmServiceInfo p && Equals(p);
//        }

//        /// <summary>
//        /// Determines whether the specified object is equal to the current object.
//        /// </summary>
//        /// <param name="p">The p.</param>
//        /// <returns></returns>
//        public bool Equals(CrmServiceInfo p)
//        {
//            return 
//                p != null &&
//                CrmServerUrl == p.CrmServerUrl && 
//                CrmDiscoveryServerUrl == p.CrmDiscoveryServerUrl &&
//                CrmOrganization == p.CrmOrganization &&
//                ImpersonationUserId == p.ImpersonationUserId &&
//                EnableProxyTypes == p.EnableProxyTypes &&
//                Timeout == p.Timeout;
//        }

//        /// <summary>
//        /// Returns a hash code for this instance.
//        /// </summary>
//        /// <returns>
//        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
//        /// </returns>
//        public override int GetHashCode()
//        {
//            return (CrmServerUrl + CrmDiscoveryServerUrl).GetHashCode();
//        }

//        /// <summary>
//        /// Gets the organization key.
//        /// </summary>
//        /// <returns></returns>
//        public string GetOrganizationKey()
//        {
//            int start = CrmServerUrl.IndexOf(@"//", StringComparison.Ordinal) + 2; // @"//".Length
//            int end = CrmServerUrl.LastIndexOf('/');
//            if (end < start)
//            {
//                end = CrmServerUrl.Length;
//            }

//            return $"{CrmServerUrl.Substring(start, end - start).ToLower()}|{CrmOrganization.ToLower()}|";
//        }
//    }
//}
