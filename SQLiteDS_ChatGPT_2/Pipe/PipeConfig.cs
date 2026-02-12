namespace SQLiteDS_ChatGPT_2.Pipe
{
    public static class PipeConfig
    {
        public const string PipeName = "SQLiteDS_TickPipe";
        public const int MaxClients = 32;
        public const int PipeBufferSize = 1024 * 1024 * 4;
    }
}
