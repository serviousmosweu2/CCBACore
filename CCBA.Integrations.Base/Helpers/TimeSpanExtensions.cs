using System;

namespace CCBA.Integrations.Base.Helpers
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan AddJitter(this TimeSpan timeSpan, int max = 1000)
        {
            var jitter = TimeSpan.FromMilliseconds(new Random((int)DateTime.UtcNow.Ticks).Next(1, max));
            return timeSpan.Add(jitter);
        }
    }
}