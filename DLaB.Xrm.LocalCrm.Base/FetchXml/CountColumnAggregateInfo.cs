using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.LocalCrm.FetchXml
{
    /// <summary>
    /// Fetch Xml Class
    /// </summary>
    public class CountColumnAggregateInfo
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string? Key { get; set; }
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Entity? Entity { get; set; }
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }
    }
}
