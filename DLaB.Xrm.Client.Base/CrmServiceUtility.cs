using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Description;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Creates connections to CRM
    /// </summary>
    public class CrmServiceUtility
    {
        #region Fields / Properties

        private static Assembly _crmEntitiesAssembly;

        #endregion Fields / Properties

        #region GetOrganizationService

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService(CrmServiceInfo info)
        {
            return CreateService(info);
        }

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <param name="crmOrganization">The CRM organization.</param>
        /// <param name="impersonationUserId">The impersonation user identifier.</param>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService(string crmOrganization,
            Guid impersonationUserId = new Guid())
        {
            return GetOrganizationService(new CrmServiceInfo(crmOrganization, impersonationUserId));
        }

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <param name="crmOrganizationUrl">The CRM organization URL.</param>
        /// <param name="crmDiscoveryUrl">The CRM discovery URL.</param>
        /// <param name="crmOrganization">The CRM organization.</param>
        /// <param name="impersonationUserId">The impersonation user identifier.</param>
        /// <param name="enableProxyTypes">if set to <c>true</c> [enable proxy types].</param>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService(string crmOrganizationUrl,
            string crmDiscoveryUrl, string crmOrganization, Guid impersonationUserId = new Guid(),
            bool enableProxyTypes = true)
        {
            return CreateService(new CrmServiceInfo(crmOrganizationUrl, crmDiscoveryUrl, crmOrganization,
                impersonationUserId) { EnableProxyTypes = enableProxyTypes });
        }

        /// <summary>
        /// Create the Organization proxy given the network user credentials
        /// </summary>
        public static IClientSideOrganizationService GetOrganizationService(string crmOrganizationUrl,
            string crmDiscoveryUrl, string crmOrganization, string domain, string userName, string password,
            bool enableProxyTypes = true)
        {
            return CreateService(new CrmServiceInfo(crmOrganizationUrl, crmDiscoveryUrl, crmOrganization)
            {
                UserDomainName = domain,
                UserName = userName,
                UserPassword = password,
                EnableProxyTypes = enableProxyTypes
            });
        }

        #endregion GetOrganizationService

        private static IClientSideOrganizationService CreateService(CrmServiceInfo info)
        {
            var url = info.CrmServerUrl + "/" + info.CrmOrganization;
            if(info.CrmServerUrl.Contains("crm.dynamics.com"))
            {
                const string onlinePrefix = "https://";
                url = onlinePrefix + info.CrmOrganization + "." + info.CrmServerUrl.Substring(onlinePrefix.Length);
            }

            var client = new CrmConnection
            {
                CallerId = info.ImpersonationUserId == Guid.Empty ? null : new Guid?(info.ImpersonationUserId),
                ClientCredentials = GetCredentials(info),
                ProxyTypesAssembly = info.EnableProxyTypes ? GetEarlyBoundProxyAssembly(info) : null,
                ProxyTypesEnabled = info.EnableProxyTypes,
                ServiceUri = new Uri(url),
                Timeout =  info.Timeout.Ticks == 0 ? null : new TimeSpan?(info.Timeout),
            };

            return new ClientSideOrganizationService(new OrganizationService(client));

        }

        private static ClientCredentials GetCredentials(CrmServiceInfo info)
        {
            var cred = new ClientCredentials();
            var userName = String.Empty;
            var password = String.Empty;
            var domain = String.Empty;

            if (info == null)
            {
                if (!Debugger.IsAttached)
                {
                    return cred;
                }
            }
            else
            {
                userName = info.UserName;
                password = info.UserPassword;
                domain = info.UserDomainName;
            }

            // If the caller hasn't explicitly set the user name and password, user the Debug credentials
            if (Debugger.IsAttached && string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
            {
                userName = AppConfig.DebugUserAccountName;
                password = AppConfig.DebugUserAccountPassword;
                domain = AppConfig.DebugUserAccountDomain;
            }
            
            // If UserName or Password is null, return standard Client Credentials
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return cred;
            }

            // If there is a domain, use Network Credentials.  If there is not a domain just set client credentials
            if (String.IsNullOrWhiteSpace(domain))
            {
                cred.UserName.UserName = userName;
                cred.UserName.Password = password;
            }
            else
            {
                cred.Windows.ClientCredential = new System.Net.NetworkCredential(userName, password, domain);
            }

            return cred;
        }

        #region EarlyBoundProxy

        /// <summary>
        /// Gets the early bound proxy assembly.
        /// </summary>
        /// <param name="defaultAssembly">The default assembly.</param>
        /// <returns></returns>
        public static Assembly GetEarlyBoundProxyAssembly(Assembly defaultAssembly = null)
        {
            if (_crmEntitiesAssembly != null)
            {
                return _crmEntitiesAssembly;
            }

            if (defaultAssembly == null)
            {
                return GetEarlyBoundProxyAssembly(AppConfig.CrmEntities.ContextType);
            }

            _crmEntitiesAssembly = defaultAssembly;
            return _crmEntitiesAssembly;
        }

        private static Assembly GetEarlyBoundProxyAssembly(string assemblyName)
        {
            var type = Type.GetType(assemblyName);
            if (type == null)
            {
                throw new Exception("Unable to load EarlyBoundProxy Assembly " + assemblyName + ".  Populate the EarlyBound Crm Context name in the CrmEntities.ContextType App Settings Config value.");
            }
            _crmEntitiesAssembly = type.Assembly;
            return _crmEntitiesAssembly;
        }

        private static Assembly GetEarlyBoundProxyAssembly(CrmServiceInfo info)
        {
            if (String.IsNullOrWhiteSpace(info.ProxyTypeAssembly))
            {
                return GetEarlyBoundProxyAssembly();
            }

            return GetEarlyBoundProxyAssembly(info.ProxyTypeAssembly);
        }

        #endregion EarlyBoundProxy

        #region Url Formatting

        /// <summary>
        /// Gets the discovery service URI.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public static Uri GetDiscoveryServiceUri(string serverName)
        {
            return new Uri($@"{serverName}/XRMServices/2011/Discovery.svc");
        }

        /// <summary>
        /// Gets the organization service URI.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static Uri GetOrganizationServiceUri(CrmServiceInfo info)
        {
            return new Uri($@"{info.CrmServerUrl}/{info.CrmOrganization}/XRMServices/2011/Organization.svc");
        }

        #endregion Url Formatting

    }
}
