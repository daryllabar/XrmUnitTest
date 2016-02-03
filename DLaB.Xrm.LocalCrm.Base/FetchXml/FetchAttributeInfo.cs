
namespace DLaB.Xrm.LocalCrm.FetchXml
{
    /// <summary>
    /// FetchXml Attribute Info
    /// </summary>
    public class FetchAttributeInfo
    {
        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public FetchAttributeType Attribute { get; set; }
        /// <summary>
        /// Gets or sets the name of the entity logical.
        /// </summary>
        /// <value>
        /// The name of the entity logical.
        /// </value>
        public string EntityLogicalName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchAttributeInfo"/> class.
        /// </summary>
        /// <param name="entityLogicalName">Name of the entity logical.</param>
        /// <param name="attribute">The attribute.</param>
        public FetchAttributeInfo(string entityLogicalName, FetchAttributeType attribute)
        {
            Attribute = attribute;
            EntityLogicalName = entityLogicalName;
        }
    }
}
