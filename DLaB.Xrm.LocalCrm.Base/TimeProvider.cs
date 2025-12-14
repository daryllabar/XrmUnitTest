using System;

namespace DLaB.Xrm.LocalCrm
{
    public interface ITimeProvider
    {
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
