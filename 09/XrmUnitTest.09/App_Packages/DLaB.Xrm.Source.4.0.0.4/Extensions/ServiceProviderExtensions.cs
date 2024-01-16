#if XRM_2013 || XRM_2015 || XRM_2016
using System;

namespace Microsoft.Xrm.Sdk.Extensions
{
    /// <summary>
    /// Extensions for Service Provider
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>Gets the service object of the specified type.</summary>
        /// <param name="serviceProvider">The Service Provider</param>
        /// <returns>A service object of type <paramref name="T" />.
        /// -or-
        /// <see langword="null" /> if there is no service object of type <paramref name="T" />.</returns>
        public static T Get<T>(this IServiceProvider serviceProvider) => (T)serviceProvider.GetService(typeof(T));

        /// <summary>
        /// Creates an instance of IOrganizationService initialized with a given user id.  Null will use the current user id.
        /// </summary>
        /// <param name="id">Id of user.</param>
        public static IOrganizationService GetOrganizationService(
            this IServiceProvider serviceProvider,
            Guid id)
        {
            return serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(id);
        }
    }
}
#endif