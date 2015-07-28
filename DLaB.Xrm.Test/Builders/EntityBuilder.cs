using System;
using DLaB.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    public abstract class EntityBuilder<TEntity> where TEntity : Entity
    {
        protected Guid Id { get; set; }

        #region Abstract Methods

        protected abstract TEntity BuildInternal();

        #endregion // Abstract Methods

        /// <summary>
        /// Builds this instance, defaulting the Id if not populated.  Does not creat it in CRM.
        /// </summary>
        /// <returns></returns>
        public TEntity Build()
        {
            return Build(true);
        }

        /// <summary>
        /// If creating in CRM, don't want to default the Id.
        /// </summary>
        /// <param name="defaultId">if set to <c>true</c> [default identifier].</param>
        /// <returns></returns>
        private TEntity Build(bool defaultId)
        {
            var entity = BuildInternal();
            if (defaultId && entity.Id == Guid.Empty && Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
            if (Id != Guid.Empty)
            {
                entity.Id = Id;
            }
            return entity;
        }

        /// <summary>
        /// Creates the specified entity, setting the Id the Id Property is populated.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public TEntity Create(IOrganizationService service)
        {
            var entity = Build(false);
            entity.Id = service.Create(entity);
            PostCreate(service, entity);
            return entity;
        }

        /// <summary>
        /// Allows child classes to be able to cleanup other entities after record is created.
        /// Helpful when a plugin may already be creating the class, and the auto-created version needs to get cleaned up.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        protected virtual void PostCreate(IOrganizationService service, TEntity entity)
        {
            // Do Nothing
        }

        /// <summary>
        /// It's very easy to attempt to create an entity using the wrong builder.  Call this method to ensure the type of the Id matches the type of the builder...
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected void AssertCorrectIdType(Id id)
        {
            Assert.AreEqual(EntityHelper.GetEntityLogicalName<TEntity>(), id.LogicalName, "Wrong Builder Specified");
        }
    }
}
