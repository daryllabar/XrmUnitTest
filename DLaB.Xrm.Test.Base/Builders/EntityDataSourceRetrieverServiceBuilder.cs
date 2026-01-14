using Microsoft.Xrm.Sdk;
#if NET
using DLaB.Xrm;

namespace DataverseUnitTest.Builders
#else

namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Derived Version of the EntityDataSourceRetrieverServiceBuilderBase
    /// </summary>
    public sealed class EntityDataSourceRetrieverServiceBuilder : EntityDataSourceRetrieverServiceBuilderBase<EntityDataSourceRetrieverServiceBuilder>
    {
        /// <summary>
        /// Gets the Entity Data Source Retriever Service of the derived Class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override EntityDataSourceRetrieverServiceBuilder This => this;
    }

    /// <summary>
    /// Abstract Builder to allow for Derived Types to created
    /// </summary>
    /// <typeparam name="TDerived">The type of the derived.</typeparam>
    public abstract class EntityDataSourceRetrieverServiceBuilderBase<TDerived> where TDerived : EntityDataSourceRetrieverServiceBuilderBase<TDerived>
    {
        /// <summary>
        /// Gets the derived version of the class.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected abstract TDerived This { get; }

        /// <summary>
        /// Gets or sets the retriever Service.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected FakeEntityDataSourceRetrieverService RetrieverService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataSourceRetrieverServiceBuilderBase{TDerived}" /> class.
        /// </summary>
        protected EntityDataSourceRetrieverServiceBuilderBase()
        {
            RetrieverService = new FakeEntityDataSourceRetrieverService();
        }


        #region Fluent Methods

        /// <summary>
        /// Sets the Entity Data Source of the Entity Data Source Retriever Service
        /// </summary>
        /// <param name="entityDataSource">The entity data source.</param>
        /// <returns></returns>
        public TDerived WithEntityDataSource(Entity entityDataSource)
        {
            RetrieverService.EntityDataSource = entityDataSource;
            return This;
        }


        #endregion Fluent Methods

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public IEntityDataSourceRetrieverService Build()
        {
            return RetrieverService.Clone();
        }
    }
}
