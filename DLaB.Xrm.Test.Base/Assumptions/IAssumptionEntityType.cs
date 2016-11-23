using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Assumptions
{
    /// <summary>
    /// Interface to allow for tying a non-generic attribute to a Entity
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IAssumptionEntityType<TAttribute, TEntity>
        where TAttribute: EntityDataAssumptionBaseAttribute
        where TEntity : Entity
    {
    }
}
