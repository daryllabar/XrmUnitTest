using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Collections.Concurrent;
using DLaB.Common;
using DLaB.Xrm.Entities;

namespace DLaB.Xrm.Client
{
    public class CrmServiceUtility
    {
        #region Fields / Properties

        private static System.Reflection.Assembly _crmEntitiesAssembly;

        #endregion // Fields / Properties

        #region GetOrganizationService

        public static IClientSideOrganizationService GetOrganizationService(CrmServiceInfo info)
        {
            return CreateService(info);
        }

        public static IClientSideOrganizationService GetOrganizationService(string crmOrganization,
            Guid impersonationUserId = new Guid())
        {
            return GetOrganizationService(new CrmServiceInfo(crmOrganization, impersonationUserId));
        }

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

        #endregion // GetOrganizationService

        private static IClientSideOrganizationService CreateService(CrmServiceInfo info)
        {
            var url = info.CrmServerUrl + "/" + info.CrmOrganization;
            if(info.CrmServerUrl.Contains("crm.dynamics.com"))
            {
                const string onlinePrefix = "https://";
                url = onlinePrefix + info.CrmOrganization + "." + info.CrmServerUrl.Substring(onlinePrefix.Length - 1);
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
            if (Debugger.IsAttached && String.IsNullOrWhiteSpace(userName) && String.IsNullOrWhiteSpace(password))
            {
                userName = ConfigurationManager.AppSettings["DebugUserAccountName"];
                password = ConfigurationManager.AppSettings["DebugUserAccountPassword"];
                domain = ConfigurationManager.AppSettings["DebugUserAccountDomain"];
            }
            
            // If UserName or Password is null, return standard Client Credentials
            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
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

        public static Assembly GetEarlyBoundProxyAssembly()
        {
            if (_crmEntitiesAssembly != null) 
            {
                return _crmEntitiesAssembly;
            }
            return GetEarlyBoundProxyAssembly(Config.GetAppSettingOrDefault("CrmEntities.ContextType", () => typeof (CrmContext).AssemblyQualifiedName));
        }

        private static Assembly GetEarlyBoundProxyAssembly(string assemblyName)
        {
            var type = Type.GetType(assemblyName);
            if (type == null)
            {
                throw new Exception("Unable to load EarlyBoundProxy Assembly " + assemblyName);
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

        #region Url Formatting

        public static Uri GetDiscoveryServiceUri(string serverName)
        {
            return new Uri(string.Format(@"{0}/XRMServices/2011/Discovery.svc", serverName));
        }

        public static Uri GetOrganizationServiceUri(CrmServiceInfo info)
        {
            return new Uri(String.Format(@"{0}/{1}/XRMServices/2011/Organization.svc",
                info.CrmServerUrl,
                info.CrmOrganization));
        }

        #endregion // Url Formatting

    }
}
