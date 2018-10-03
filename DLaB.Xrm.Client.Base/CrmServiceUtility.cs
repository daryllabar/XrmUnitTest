using System;
using System.Reflection;
#if XRM_2015 || PRE_KEYATTRIBUTE
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
#else
using Microsoft.Xrm.Tooling.Connector;
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
#else
            var client = new CrmServiceClient(connectionString);
            return new ClientSideOrganizationService(client.OrganizationServiceProxy);
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
                throw new Exception("Unable to load EarlyBoundProxy Assembly " + assemblyName + ".  Populate the EarlyBound Crm Context name in the CrmEntities.ContextType App Settings Config value.");
            }
            _crmEntitiesAssembly = type.Assembly;
            return _crmEntitiesAssembly;
        }

#endregion EarlyBoundProxy
    }
}
