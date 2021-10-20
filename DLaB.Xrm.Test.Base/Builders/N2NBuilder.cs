using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else
namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Builder for Associate only Entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class N2NBuilder<TEntity> : DLaBEntityBuilder<TEntity, N2NBuilder<TEntity>> where TEntity : Entity
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public N2NBuilder()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        public N2NBuilder(Guid id)
            : this()
        {
            Id = id;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        public N2NBuilder(Id id)
            : this(id.EntityId)
        {

        }

        #endregion Constructors

        /// <summary>
        /// Builds the Entity
        /// </summary>
        /// <returns></returns>
        protected override TEntity BuildInternal()
        {
            return Activator.CreateInstance<TEntity>();
        }

        /// <summary>
        /// Creates the entity via associate request
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override Guid CreateInternal(IOrganizationService service, TEntity entity)
        {
            var provider = TestBase.GetConfiguredLocalDatabaseInfo(nameof(N2NBuilder<TEntity>), Guid.Empty).ManyToManyAssociationProvider;
            if (!provider.IsManyToManyJoinEntity(entity.LogicalName))
            {
                throw new Exception($"{entity.LogicalName} was defined to use an N2NBuilder, but was not defined in the ManyToManyAssociationProvider!");
            }
            var response = (AssociateResponse)service.Execute(provider.CreateAssociateRequest(entity));
            var results = response.Results.GetParameterValue<Guid[]>("CreatedIds");
            return results == null || results.Length == 0
                ? Guid.Empty
                : results[0];
        }
    }
}
