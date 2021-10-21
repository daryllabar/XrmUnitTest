#if NET
using DataverseUnitTest.Builders;
#else
using DLaB.Xrm.Test.Builders;
#endif

namespace XrmUnitTest.Test.Builders
{
    /// <summary>
    /// Class to simplify the simplest cases of creating entities without changing the defaults.
    /// </summary>
    public class CrmEnvironmentBuilder : CrmEnvironmentBuilderBase<CrmEnvironmentBuilder>
    {
        /// <summary>
        /// Gets the current instance.
        /// </summary>
        /// <value>
        /// The this.
        /// </value>
        protected override CrmEnvironmentBuilder This => this;

        #region Fluent Methods

        #endregion Fluent Methods
    }
}
