using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public interface IEntityBuilder<T> : IEntityBuilder where T : Entity
    {
    }

    public interface IEntityBuilder
    {
    }
}
