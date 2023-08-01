using System;

namespace HMS.HealthTrack.Inventory.Common
{
    public interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }
}
