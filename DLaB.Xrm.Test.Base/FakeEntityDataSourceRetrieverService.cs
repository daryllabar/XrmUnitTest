using System;
using Microsoft.Xrm.Sdk;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Fake implementation of IEntityDataSourceRetrieverService for unit testing.
    /// </summary>
    public class FakeEntityDataSourceRetrieverService : IEntityDataSourceRetrieverService, IServiceFaked<IEntityDataSourceRetrieverService>, IFakeService, ICloneable
    {
        /// <summary>
        /// Gets or sets the entity that serves as the data source for operations.
        /// </summary>
        public Entity EntityDataSource { get; set; } = new("Entity Data Source Not Set!");

        /// <summary>
        /// Retrieves the current entity data source associated with this instance.
        /// </summary>
        /// <returns>The <see cref="Entity"/> object representing the current data source.</returns>
        public Entity RetrieveEntityDataSource()
        {
            return EntityDataSource;
        }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new FakeEntityDataSourceRetrieverService with copied state.</returns>
        public FakeEntityDataSourceRetrieverService Clone()
        {
            var clone = (FakeEntityDataSourceRetrieverService)MemberwiseClone();
            clone.EntityDataSource = EntityDataSource;           
            return clone;
        }


        /// <summary>
        /// Creates a copy of the current object.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}