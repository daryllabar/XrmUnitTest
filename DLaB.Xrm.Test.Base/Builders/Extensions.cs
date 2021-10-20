using System;
using System.Collections.Generic;

#if NET
namespace DataverseUnitTest.Builders
#else
namespace DLaB.Xrm.Test.Builders
#endif
{
    /// <summary>
    /// Extensions for Test Builders
    /// </summary>
    public static class Extensions
    {
        #region Dictionary<Guid,>

        /// <summary>
        /// Shortcut method to create the Builder and add it to the dictionary if not exists.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="createBuilder">The create builder.</param>
        public static void CreateAndAddIfNotExists<TValue>(this Dictionary<Guid, TValue> dictionary, Guid id, Func<TValue> createBuilder)
        {
            if (!dictionary.ContainsKey(id))
            {
                dictionary.Add(id, createBuilder());
            }
        }

        #endregion Dictionary<Guid,>
    }
}
