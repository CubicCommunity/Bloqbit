namespace Bloqbit.Include
{
    public static class Uptime
    {
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public static TimeSpan Get()
        {
            return DateTime.UtcNow - _startTime;
        }

        public static string GetFormatted()
        {
            var uptime = Get();
            return $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds";
        }
    }
}