using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// Abstract Entity builder for Creating Entities
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class EntityBuilder<TEntity> : IEntityBuilder<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        protected Dictionary<string, object> Attributes { get; }
        /// <summary>
        /// Gets or sets the Id of the Entity being created.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        protected Guid Id { get; set; }

        #region Abstract Methods

        /// <summary>
        /// Internal Build Method that gets called by the Build to creat ethe entity
        /// </summary>
        /// <returns></returns>
        protected abstract TEntity BuildInternal();

        #endregion Abstract Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBuilder{TEntity}"/> class.
        /// </summary>
        protected EntityBuilder()
        {
            Attributes = new Dictionary<string, object>();
        }

		#endregion Constructors

        #region IEntityBuilder Implementation

        Entity IEntityBuilder.Build()
        {
            return Build();
        }

        Entity IEntityBuilder.Create(IOrganizationService service)
        {
            return Create(service);
        }


        void IEntityBuilder.PostCreate(IOrganizationService service, Entity entity)
        {
            PostCreate(service, (TEntity)entity);
        }

        /// <summary>
        /// Defines that the entity should be built with the given attribute value
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IEntityBuilder WithAttributeValue(string attributeName, object value)
        {
            Attributes[attributeName] = value;
            return this;
        }

        #endregion IEntityBuilder Implementation

        /// <summary>
        /// Builds this instance, defaulting the Id if not populated.  Does not create it in CRM.
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
            var entity = BuildInternal().Clone();
            foreach (var att in Attributes)
            {
                entity[att.Key] = att.Value;
            }
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
        /// Helpful when a plugin may already be creating the entity, and the auto-created version needs to get cleaned up.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="entity">The entity.</param>
        public virtual void PostCreate(IOrganizationService service, TEntity entity)
        {
            // Do Nothing
        }

        /// <summary>
        /// It's very easy to attempt to create an entity using the wrong builder.  Call this method to ensure the type of the Id matches the type of the builder...
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected void AssertCorrectIdType(Id id)
        {
            var builderEntityLogicalName = EntityHelper.GetEntityLogicalName<TEntity>();
            TestSettings.TestFrameworkProvider.AssertAreEqual(builderEntityLogicalName, id.LogicalName, $"Wrong Builder Specified.  Builder is of type {builderEntityLogicalName} but id of type {id.LogicalName} ");
        }
    }
}
