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
        /// Combines the Building, Creating, and Post Creation of the Entity.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        Entity Create(IOrganizationService service);

        /// <summary>
        /// Allows child classes to be able to cleanup other entities after record is created.
        /// Helpful when a plugin may already be creating the entity, and the auto-created version needs to get cleaned up.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        void PostCreate(IOrganizationService service, Entity entity);

        /// <summary>
        /// Allows for setting the given attribute of the entity when built
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        IEntityBuilder WithAttributeValue(string attributeName, object value);
    }
}
