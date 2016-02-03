using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// A generic Interface for Fluent Building of Entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="DLaB.Xrm.Test.Builders.IEntityBuilder" />
    public interface IEntityBuilder<out T> : IEntityBuilder where T : Entity
    {
        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        new T Build();
        /// <summary>
        /// Creates the specified service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        new T Create(IOrganizationService service);
    }

    /// <summary>
    /// An Interface for Fluent Building of Entities
    /// </summary>
    /// <seealso cref="DLaB.Xrm.Test.Builders.IEntityBuilder" />
    public interface IEntityBuilder
    {
        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        Entity Build();
        /// <summary>
        /// Creates the specified service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        Entity Create(IOrganizationService service);
        /// <summary>
        /// Allows for setting the given attribute of the entity when built
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        void WithAttributeValue(string attributeName, object value);
    }
}
