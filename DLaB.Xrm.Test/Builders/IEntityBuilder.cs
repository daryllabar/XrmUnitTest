using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public interface IEntityBuilder<out T> : IEntityBuilder where T : Entity
    {
        new T Build();
        new T Create(IOrganizationService service);
    }

    public interface IEntityBuilder
    {
        Entity Build();
        Entity Create(IOrganizationService service);
        void WithAttributeValue(string attributeName, object value);
    }
}
