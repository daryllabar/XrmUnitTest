using Microsoft.Xrm.Sdk;

namespace XrmUnitTest.Test.Builders
{
    public abstract class EntityBuilder<TEntity> : DLaB.Xrm.Test.Builders.EntityBuilder<TEntity> where TEntity : Entity
    {

    }
}
