using Microsoft.Xrm.Sdk;
using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Plugin
#else
namespace Source.DLaB.Xrm.Plugin
#endif
{
    /// <summary>
    /// Wraps Lazy creation of different Organization Services
    /// </summary>
    public class OrganizationServicesWrapper
    {
        /// <summary>
        /// A service that will cache the retrieve/retrieve multiple results and reuse them.  Uses the System Organization Service to prevent different users from retrieving different results.
        /// </summary>
        public Lazy<IOrganizationService> Cached { get; }
        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that triggered the services using the PluginExecutionContext.InitiatingUserId.
        /// </summary>
        public Lazy<IOrganizationService> InitiatingUser { get; }
        /// <summary>
        /// The IOrganizationService of the plugin, Impersonated as the user that plugin is registered to run as, using the PluginExecutionContext.UserId.
        /// </summary>
        public Lazy<IOrganizationService> Organization { get; }
        /// <summary>
        /// The IOrganizationService of the plugin, using the System User by not specifying a UserId.
        /// </summary>
        public Lazy<IOrganizationService> System { get; }

        /// <summary>
        /// Initializes a new instance of the OrganizationServicesWrapper class.
        /// </summary>
        /// <param name="organizationService">The IOrganizationService to be used for the Organization context.</param>
        /// <param name="systemService">The IOrganizationService to be used for the System context.</param>
        /// <param name="initiatingUserService">The IOrganizationService to be used for the Initiating User context.</param>
        /// <param name="cachedService">The IOrganizationService to be used for caching.</param>
        public OrganizationServicesWrapper(Lazy<IOrganizationService> organizationService,
            Lazy<IOrganizationService> systemService,
            Lazy<IOrganizationService> initiatingUserService,
            Lazy<IOrganizationService> cachedService)
        {
            Cached = cachedService;
            InitiatingUser = initiatingUserService;
            Organization = organizationService;
            System = systemService;
        }

        /// <summary>
        /// Initializes a new instance of the OrganizationServicesWrapper class.
        /// </summary>
        /// <param name="organizationService">The IOrganizationService to be used for the Organization context.</param>
        /// <param name="systemService">The IOrganizationService to be used for the System context.</param>
        /// <param name="initiatingUserService">The IOrganizationService to be used for the Initiating User context.</param>
        /// <param name="cachedService">The IOrganizationService to be used for caching.</param>
        public OrganizationServicesWrapper(IOrganizationService organizationService,
            IOrganizationService systemService,
            IOrganizationService initiatingUserService,
            IOrganizationService cachedService) : this(
                new Lazy<IOrganizationService>(() => organizationService),
                new Lazy<IOrganizationService>(() => systemService),
                new Lazy<IOrganizationService>(() => initiatingUserService), new Lazy<IOrganizationService>(() => cachedService))
        {
        }
    }
}
