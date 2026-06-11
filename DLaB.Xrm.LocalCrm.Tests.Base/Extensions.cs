using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.Tests
{
    public static class Extensions
    {
        /// <summary>
        /// Creates the specified entity and returns the entity with the Id populated.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="service">The organization service.</param>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The created entity with the Id populated.</returns>
        public static TEntity CreateEntity<TEntity>(this IOrganizationService service, TEntity entity) where TEntity : Entity
        {
            entity.Id = service.Create(entity);
            return entity;
        }
    }
}
