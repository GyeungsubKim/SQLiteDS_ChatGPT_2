using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class GlobalCommitQueue : IDisposable
    {
        private readonly BlockingCollection<SqlWriteJob> _queue;
        private readonly Thread _writerThread;
        private readonly SqliteConnection _conn;

        private volatile bool _running = true;

        private const int BATCH_SIZE = 256;

        public GlobalCommitQueue(SqliteConnection conn, int capacity = 100_000)
        {
            _conn = conn;
            _queue = new BlockingCollection<SqlWriteJob>(
                new ConcurrentQueue<SqlWriteJob>(), capacity);
            _writerThread = new Thread(WriteLoop)
            {
                IsBackground = true,
                Name = "SQLite Single Writer"
            };
            _writerThread.Start();
        }
        public void Enqueue(SqlWriteJob job)
        {
            if (!_running)
                throw new InvalidOperationException("Queue stopped");

            _queue.Add(job);
        }
        private void WriteLoop()
        {
            List<SqlWriteJob> batch = new(BATCH_SIZE);

            while (_running || _queue.Count > 0)
            {
                try
                {
                    if (!_queue.TryTake(out SqlWriteJob? first, 50)) continue;
                    batch.Add(first!);

                    while(batch.Count < BATCH_SIZE &&
                        _queue.TryTake(out var j))
                        batch.Add(j);

                    ExecuteBatch(batch);
                    batch.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Writer Error] {ex}");
                }
            }
        }
        private void ExecuteBatch(List<SqlWriteJob> batch)
        {
            using var tx = _conn.BeginTransaction();

            foreach (var job in batch)
            {
                using var cmd = _conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = job.Sql;
                cmd.Parameters.AddRange(job.Parameters);

                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
        public void Dispose()
        {
            _running = false;
            _queue.CompleteAdding();
            _writerThread.Join();
        }
    }
}
