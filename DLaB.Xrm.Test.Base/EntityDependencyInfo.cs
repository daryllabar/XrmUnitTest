using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Contains the logical name of an entity and it's list of CyclicAttributes 
    /// </summary>
    [DebuggerDisplay("{LogicalName}")]
    public class EntityDependencyInfo
    {
        /// <summary>
        /// Gets the name of the logical.
        /// </summary>
        /// <value>
        /// The name of the logical.
        /// </value>
        public string LogicalName { get; }
        /// <summary>
        /// Gets the cyclic attributes.
        /// </summary>
        /// <value>
        /// The cyclic attributes.
        /// </value>
        public IReadOnlyCollection<string> CyclicAttributes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDependencyInfo" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="cyclicAttributes">The cyclic attributes.</param>
        public EntityDependencyInfo(string name, IEnumerable<string> cyclicAttributes )
        {
            LogicalName = name;
            CyclicAttributes = new ReadOnlyCollection<string>(cyclicAttributes.Distinct().ToList());
        }
    }
}
