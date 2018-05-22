using Microsoft.Xrm.Sdk;

namespace Xyz.MsTestBase.Builders
{
    public abstract class EntityBuilder<TEntity> : DLaB.Xrm.Test.Builders.EntityBuilder<TEntity> where TEntity : Entity
    {

    }
}
