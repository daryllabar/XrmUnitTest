#if !PRE_MULTISELECT
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Fake implementation of IManagedIdentityService for unit testing.
    /// </summary>
    public class FakeManagedIdentityService : IManagedIdentityService, IServiceFaked<IManagedIdentityService>, IFakeService, ICloneable
    {
        /// <summary>
        /// Gets or sets the token to return from AcquireToken.
        /// </summary>
        public string TokenToReturn { get; set; } = "fake-token";

        /// <summary>
        /// Gets the list of scopes passed to AcquireToken.
        /// </summary>
        public List<IEnumerable<string>> AcquiredScopes => _acquiredScopes;
        private List<IEnumerable<string>> _acquiredScopes = [];

        /// <summary>
        /// Optional custom function to control AcquireToken behavior.
        /// </summary>
        public Func<IEnumerable<string>, string>? AcquireTokenFunc { get; set; }

        /// <inheritdoc />
        public string AcquireToken(IEnumerable<string> scopes)
        {
            var localScopes = scopes as List<string> ?? scopes.ToList();
            _acquiredScopes.Add(localScopes);
            return AcquireTokenFunc == null
                ? TokenToReturn
                : AcquireTokenFunc(localScopes);
        }

        /// <inheritdoc />
        public string AcquireToken(Guid managedIdentityId, IEnumerable<string> scopes)
        {
            return AcquireToken(scopes);
        }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new FakeManagedIdentityService with copied state.</returns>
        public FakeManagedIdentityService Clone()
        {
            var clone = new FakeManagedIdentityService
            {
                TokenToReturn = this.TokenToReturn,
                AcquireTokenFunc = this.AcquireTokenFunc,
                _acquiredScopes = new List<IEnumerable<string>>(this._acquiredScopes)
            };
            return clone;
        }

        /// <summary>
        /// Creates a shallow copy of the current object.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}
#endif
