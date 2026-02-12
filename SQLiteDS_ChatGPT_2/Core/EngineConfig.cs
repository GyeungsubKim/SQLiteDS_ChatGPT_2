namespace SQLiteDS_ChatGPT_2.Core
{
    public static class EngineConfig
    {
        public const int FeedChannelSize = 200_000;
        public const int ProcessChannelSize = 200_000;
        public const int WriteChannelSize = 500_000;
        public const int PipeChannelSize = 200_000;

        public const int BatchSize = 1000;
        public const int BatchFlushMs = 30;
    }
}
