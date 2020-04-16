using Microsoft.Xrm.Sdk;

namespace XrmUnitTest.Test.Builders
{
    public abstract class EntityBuilder<TEntity,TBuilder> : DLaB.Xrm.Test.Builders.EntityBuilder<TEntity,TBuilder>
        where TBuilder : EntityBuilder<TEntity, TBuilder>
        where TEntity : Entity
    {

    }

    public abstract class EntityBuilder<TEntity> : EntityBuilder<TEntity, EntityBuilder<TEntity>>
        where TEntity : Entity
    {

    }
}
