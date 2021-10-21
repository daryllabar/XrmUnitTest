using Microsoft.Xrm.Sdk;
#if NET
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace XrmUnitTest.Test.Builders
{
    public abstract class EntityBuilder<TEntity,TBuilder> : DLaBEntityBuilder<TEntity,TBuilder>
        where TBuilder : EntityBuilder<TEntity, TBuilder>
        where TEntity : Entity
    {

    }

}
