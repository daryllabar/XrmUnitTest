using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test.Builders
{
    /// <summary>
    /// Abstract Entity builder for Creating Entities
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TBuilder">The Derived Type</typeparam>
    public abstract class EntityBuilder<TEntity, TBuilder> : IEntityBuilder<TEntity>
        where TBuilder: EntityBuilder<TEntity, TBuilder>
        where TEntity : Entity
    {
        /// <summary>
        /// Gets the Entity Builder of the derived Class.
        /// </summary>
        protected abstract TBuilder This { get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        protected Dictionary<string, object> Attributes { get; }
        /// <summary>
        /// Gets the attributes set post create.
        /// </summary>
        protected Dictionary<string, object> PostCreateAttributes { get; }
        /// <summary>
        /// Gets or sets the Id of the Entity being created.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        protected Guid Id { get; set; }

        #region Abstract Methods

        /// <summary>
        /// Internal Build Method that gets called by the Build to create the entity
        /// </summary>
        /// <returns></returns>
        protected abstract TEntity BuildInternal();

        #endregion Abstract Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected EntityBuilder()
        {
            Attributes = new Dictionary<string, object>();
            PostCreateAttributes = new Dictionary<string, object>();
        }

		#endregion Constructors

        #region IEntityBuilder Implementation

        Entity IEntityBuilder.Build() { return Build(); }

        Entity IEntityBuilder.Create(IOrganizationService service, bool runPostCreate) { return Create(service, runPostCreate); }

        void IEntityBuilder.PostCreate(IOrganizationService service, Entity entity) { PostCreate(service, (TEntity) entity); }

        IEntityBuilder IEntityBuilder.WithAttributeValue(string attributeName, object value) { return WithAttributeValue(attributeName, value); }

        #endregion IEntityBuilder Implementation

        #region Fluent Methods

        /// <summary>
        /// It's very easy to attempt to create an entity using the wrong builder.  Call this method to ensure the type of the Id matches the type of the builder...
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected TBuilder AssertCorrectIdType(Id id)
        {
            var builderEntityLogicalName = EntityHelper.GetEntityLogicalName<TEntity>();
            TestSettings.TestFrameworkProvider.AssertAreEqual(builderEntityLogicalName, id.LogicalName, $"Wrong Builder Specified.  Builder is of type {builderEntityLogicalName} but id of type {id.LogicalName} ");
            return This;
        }

        /// <summary>
        /// Defines that the entity should be built with the given attribute value
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        public TBuilder WithAttributeValue(string attributeName, object value)
        {
            Attributes[attributeName] = value;
            return This;
        }

        /// <summary>
        /// Defines that the entity should be set post creation
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">The value.</param>
        public TBuilder WithPostCreateAttributeValue(string attributeName, object value)
        {
            PostCreateAttributes[attributeName] = value;
            return This;
        }

        /// <summary>
        /// Defines that the entity should be set post creation
        /// </summary>
        /// <param name="entity">The Attributes of the entity will be used to update the entity post create.</param>
        public TBuilder WithPostCreate(TEntity entity)
        {
            foreach (var att in entity.Attributes)
            {
                WithPostCreateAttributeValue(att.Key, att.Value);
            }
            return This;
        }

        #endregion Fluent Methods

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
        /// Creates the specified entity, setting populating the Id Property.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="runPostCreate">If true, PostCreate will be called directly after the create.</param>
        /// <returns></returns>
        public TEntity Create(IOrganizationService service, bool runPostCreate)
        {
            var entity = Build(false);
            entity.Id = service.Create(entity);
            if (runPostCreate)
            {
                PostCreate(service, entity);
            }
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
            if (PostCreateAttributes.Count == 0)
            {
                return;
            }

            var entityToUpdate = new Entity
            {
                Id = entity.Id,
                LogicalName = entity.LogicalName
            };
            foreach (var att in PostCreateAttributes)
            {
                entityToUpdate[att.Key] = att.Value;
            }
            service.Update(entityToUpdate);
        }
    }
}
