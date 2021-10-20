using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else

namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Class to simplify the simplest cases of creating entities without changing the defaults.  Use the CrmEnvironmentBuilderBase to provide application specific logic
    /// </summary>
    public sealed class DLaBCrmEnvironmentBuilder : CrmEnvironmentBuilderBase<DLaBCrmEnvironmentBuilder>
    {
        /// <summary>
        /// Returns the Instance
        /// </summary>
        protected override DLaBCrmEnvironmentBuilder This => this;
    }

    /// <summary>
    /// Class to simplify the simplest cases of creating entities without changing the defaults.
    /// </summary>
    public abstract class CrmEnvironmentBuilderBase<TDerived> where TDerived : CrmEnvironmentBuilderBase<TDerived>
    {
        /// <summary>
        /// Gets the Instance.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected abstract TDerived This { get; }
        /// <summary>
        /// Gets or sets the entity builders.
        /// </summary>
        /// <value>
        /// The entity builders.
        /// </value>
        internal EntityBuilderManager EntityBuilders { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmEnvironmentBuilderBase{TDerived}" /> class.
        /// </summary>
        protected CrmEnvironmentBuilderBase()
        {
            try
            {
                EntityBuilders = new EntityBuilderManager();
            }
            catch (TypeInitializationException ex)
            {
                // Loading of the Early Bound Entities Could cause a major Exception
                if (ex.InnerException == null)
                {
                    throw;
                }
                throw ex.InnerException;
            }
        }

        #region Fluent Methods

        /// <summary>
        /// Primarily Used in Conjuction with WithEntities&lt;TIdsStruct&gt; to allow for the exclusion of some Ids from being Created
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived ExceptEntities(params Id[] ids)
        {
            foreach (var id in ids)
            {
                EntityBuilders.Remove(id);
            }
            return This;
        }

        /// <summary>
        /// Primarily Used in Conjuction with WithEntities&lt;TIdsStruct&gt; to allow for the exclusion of some Ids from being Created
        /// </summary>
        /// <typeparam name="TIdsStruct">The type of the ids structure.</typeparam>
        /// <returns></returns>
        public TDerived ExceptEntities<TIdsStruct>() where TIdsStruct : struct
        {
            return ExceptEntities(typeof(TIdsStruct).GetIds().ToArray());
        }

        /// <summary>
        /// Allows for the specification of any fluent methods to the builder for the given entity
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public TDerived WithBuilder<TBuilder>(Id id, Action<TBuilder> action) where TBuilder : class, IEntityBuilder
        {
            EntityBuilders.WithBuilderForEntity(id, action);
            return This;
        }

        /// <summary>
        /// Allows for the specification of any fluent methods to the builder for the given type of entity
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public TDerived WithBuilder<TBuilder>(Action<TBuilder> action) 
            where TBuilder : class, IEntityBuilder
        {
            EntityBuilders.WithBuilderForEntityType(action);
            return This;
        }

        /// <summary>
        /// Adds the child entities to the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithChildEntities(Id parent, params Id[] ids)
        {
            EntityBuilders.Get(parent);
            foreach (var id in ids)
            {
                var name = EntityHelper.GetParentEntityAttributeName(TestBase.GetType(id), TestBase.GetType(parent));
                EntityBuilders.Get(id).WithAttributeValue(name, parent.EntityReference);
                id.Entity[name] = parent.EntityReference;
            }

            return This;
        }

        /// <summary>
        /// Creates the Entities using the default Builder for the given entity type
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public TDerived WithEntities(params Id[] ids)
        {
            foreach (var id in ids)
            {
                EntityBuilders.Get(id);
            }
            return This;
        }

        /// <summary>
        /// Walks the Struct, Creating the Entities using the default Builder for each Id Defined in the Struct
        /// </summary>
        /// <returns></returns>
        public TDerived WithEntities<TIdsStruct>() where TIdsStruct : struct
        {
            return WithEntities(typeof (TIdsStruct).GetIds().ToArray());
        }

        #endregion Fluent Methods

        /// <summary>
        /// Creates all of the Ids defined by the builder, using the given Service
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        public Dictionary<Guid, Entity> Create(IOrganizationService service)
        {
            return EntityBuilders.Create(service);
        }
    }
}
