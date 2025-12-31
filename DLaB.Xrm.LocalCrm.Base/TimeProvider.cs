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

    internal class TimeProvider :ITimeProvider
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
