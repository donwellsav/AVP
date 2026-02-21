using System;

namespace AVPlayer.Helpers
{
    public static class TimecodeHelper
    {
        // Simple helper for now, can be expanded for Drop-Frame later
        public static string ToSmpte(this TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\:ff");
        }

        public static TimeSpan FromSmpte(string timecode)
        {
            if (TimeSpan.TryParseExact(timecode, @"hh\:mm\:ss\:ff", null, out var result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }
    }
}
