using System;
using DLaB.Xrm.LocalCrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DLaB.Xrm.Test.Builders
{
    public class N2NBuilder<TEntity> : DLaBEntityBuilder<TEntity, N2NBuilder<TEntity>> where TEntity : Entity
    {
        #region Constructors

        public N2NBuilder()
        {

        }

        public N2NBuilder(Guid id)
            : this()
        {
            Id = id;
        }

        public N2NBuilder(Id id)
            : this(id.EntityId)
        {

        }

        #endregion Constructors

        protected override TEntity BuildInternal()
        {
            return Activator.CreateInstance<TEntity>();
        }

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
