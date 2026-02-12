using System.Diagnostics;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class EngineClock
    {
        private static readonly double TickToMicro =
                    1_000_000.0 / Stopwatch.Frequency;

        public static long NowMicro()
        {
            return (long)(Stopwatch.GetTimestamp() * TickToMicro);
        }
    }
}
