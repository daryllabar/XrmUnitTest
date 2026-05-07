using System;

namespace DLaB.Xrm.LocalCrm
{
    /// <summary>
    /// Time Provider Interface
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current date and time in Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>A <see cref="DateTime"/> value that represents the current UTC date and time.</returns>
        DateTime GetUtcNow();
    }

    /// <summary>
    /// Provides time-related functionality with support for overriding the current time for testing purposes.
    /// </summary>
    public class TimeProvider : ITimeProvider
    {
        /// <summary>
        /// Gets or sets an optional override value for the current UTC time.
        /// When set, this value will be returned by <see cref="GetUtcNow"/> instead of the actual current time.
        /// This is useful for testing scenarios where a specific time needs to be simulated.
        /// </summary>
        public DateTime? UtcNow { get; set; }

        /// <summary>
        /// Gets or sets the function used to retrieve the default UTC time when <see cref="UtcNow"/> is null.
        /// Defaults to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public Func<DateTime> DefaultGetUtc { get; set; } = () => DateTime.UtcNow;

        /// <summary>
        /// Gets the current date and time in Coordinated Universal Time (UTC).
        /// Returns the value of <see cref="UtcNow"/> if set; otherwise, calls <see cref="DefaultGetUtc"/>.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> value that represents the current UTC date and time.</returns>
        public DateTime GetUtcNow()
        {
            return UtcNow ?? DefaultGetUtc();
        }
    }
}
