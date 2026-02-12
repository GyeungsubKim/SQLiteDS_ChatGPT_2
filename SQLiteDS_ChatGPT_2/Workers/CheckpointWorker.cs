using Microsoft.Data.Sqlite;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class CheckpointWorker
    {
        private readonly string _connStr;
        private readonly int _intervalMs;

        public CheckpointWorker(string connStr, int intervalMs = 5000)
        {
            _connStr = connStr;
            _intervalMs = intervalMs;
        }

        public Task RunAsync(CancellationToken ct) => Task.Run(async () =>
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";

            while (!ct.IsCancellationRequested)
            {
                try { cmd.ExecuteNonQuery(); }
                catch { /* 로깅 */ }

                await Task.Delay(_intervalMs, ct);
            }
        }, ct);
    }
}
