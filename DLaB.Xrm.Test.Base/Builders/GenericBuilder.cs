using Microsoft.Xrm.Sdk;
using System;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// The Generic Entity Builder class.  If no Entity Builder exists, this class will be used by the Environment builder to create the Entity
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class GenericEntityBuilder<TEntity> : EntityBuilder<TEntity> where TEntity : Entity
    {
        #region Constructors

        public GenericEntityBuilder()
        {

        }

        public GenericEntityBuilder(Guid id)
            : this()
        {
            Id = id;
        }

        public GenericEntityBuilder(Id id)
            : this(id.EntityId)
        {
            //AssertCorrectIdType(id);
        }

        #endregion Constructors

        protected override TEntity BuildInternal()
        {
            return Activator.CreateInstance<TEntity>();
        }
    }
}
