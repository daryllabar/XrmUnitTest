using Microsoft.Xrm.Sdk;

namespace Xyz.Xrm.Test.Builders
{
    public abstract class EntityBuilder<TEntity> : DLaB.Xrm.Test.Builders.EntityBuilder<TEntity> where TEntity : Entity
    {

    }
}
