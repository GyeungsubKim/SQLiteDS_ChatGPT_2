using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_2.Workers;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Core
{
    public class DbWorker
    {
        private readonly string _dbPath;
        private readonly ChannelReader<WriteItem> _reader;

        private readonly Dictionary<WorkType, List<Basic>> _batch = new();

        private const int BatchSize = 256;

        public DbWorker(string dbPath, ChannelReader<WriteItem> reader)
        {
            _dbPath = dbPath;
            _reader = reader;
        }
        public async Task Run()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();

            SqliteUltraPragma.Apply(conn);

            while (await _reader.WaitToReadAsync())
            {
                while (_reader.TryRead(out var item))
                {
                    if (!_batch.TryGetValue(item.Type, out var list))
                    {
                        list = new List<Basic>(BatchSize);
                        _batch[item.Type] = list;
                    }
                    list.Add(item.Model);

                    if (list.Count >= BatchSize)
                        Flush(conn, item.Type, list);
                }

                FlushAll(conn);
            }
        }
        private void FlushAll(SqliteConnection conn)
        {
            foreach (var kv in _batch)
            {
                if (kv.Value.Count == 0) continue;
                Flush(conn, kv.Key, kv.Value);
            }
        }
        private void Flush(SqliteConnection conn,
                            WorkType type,
                            List<Basic> list)
        {
            using var tx = conn.BeginTransaction();
            VectorBulkInsertBinder.Insert(conn, tx, type, list);

            tx.Commit();
            list.Clear();
        }
    }
}
