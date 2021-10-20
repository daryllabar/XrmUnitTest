using System;
using System.Reflection;
#if XRM_2015 || PRE_KEYATTRIBUTE
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using DLaB.Xrm.Test;
#elif NET
using DataverseUnitTest;
using Microsoft.PowerPlatform.Dataverse.Client;
#else
using Microsoft.Xrm.Tooling.Connector;
using DLaB.Xrm.Test;
#endif

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
        /// Gets the organization service using the AppConfig
        /// </summary>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService()
        {
            return GetOrganizationService(AppConfig.ConnectionString);
        }

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <param name="connectionString">The ConnectionString to use.</param>
        /// <returns></returns>
        public static IClientSideOrganizationService GetOrganizationService(string connectionString)
        {
#if XRM_2015 || PRE_KEYATTRIBUTE
            CrmConnection crmConnection = CrmConnection.Parse(connectionString);
            OrganizationService service = new OrganizationService(crmConnection);
            return new ClientSideOrganizationService(service);
#elif NET
            var client = new ServiceClient(connectionString);
            if (!client.IsReady)
            {
                throw new Exception("Unable to connect to CRM: " + (client.LastError ?? client.LastException?.ToString()));
            }
            return new ClientSideOrganizationService(client);

#else 
            var client = new CrmServiceClient(connectionString);            
            if (!client.IsReady)
            {
                throw new Exception("Unable to connect to CRM: " + (client.LastCrmError ?? client.LastCrmException?.ToString()));
            }
            return new ClientSideOrganizationService(client);
#endif
        }

#endregion GetOrganizationService

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
                try
                {
                    _crmEntitiesAssembly = TestSettings.EarlyBound.Assembly;
                    
                }
                catch(Exception ex)
                {
                     /* If it's not configured, it will throw an error.  Would rather throw this error: */
                    throw new Exception("Unable to load EarlyBoundProxy Assembly " + assemblyName +
                                        ".  Populate the EarlyBound Crm Context name in the CrmEntities.ContextType App Settings Config value.", ex);
                }
            }
            else
            {
                _crmEntitiesAssembly = type.Assembly;
            }
            return _crmEntitiesAssembly;
        }

#endregion EarlyBoundProxy
    }
}
