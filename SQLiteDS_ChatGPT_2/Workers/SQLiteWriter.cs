using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_2.Core;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class SQLiteWriter : IDisposable
    {
        private readonly SqliteConnection _conn;

        public SQLiteWriter(string connectionString)
        {
            _conn = new SqliteConnection(connectionString);
            _conn.Open();

            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA temp_store=MEMORY;
                PRAGMA cache_size=-200000;
                PRAGMA mmap_size=30000000000;
                PRAGMA locking_mode=EXCLUSIVE;
                PRAGMA wal_autocheckpoint=0;";
            cmd.ExecuteNonQuery();
        }

        public Task RunAsync() => Task.Run(async () =>
        {
            var reader = ChannelHub.WriteChannel.Reader;

            using var txCmd = _conn.CreateCommand();
            txCmd.CommandText = "BEGIN IMMEDIATE;";

            using var commitCmd = _conn.CreateCommand();
            commitCmd.CommandText = "COMMIT;";

            using var insert = _conn.CreateCommand();
            insert.CommandText = @"
                INSERT INTO ticks
                (SymbolId, Time, Price, Volume, Ask, Bid, Type)
                VALUES ($sid,$t,$p,$v,$a,$b,$ty);";

            var p_sid = insert.CreateParameter(); p_sid.ParameterName = "$sid"; insert.Parameters.Add(p_sid);
            var p_t = insert.CreateParameter(); p_t.ParameterName = "$t"; insert.Parameters.Add(p_t);
            var p_p = insert.CreateParameter(); p_p.ParameterName = "$p"; insert.Parameters.Add(p_p);
            var p_v = insert.CreateParameter(); p_v.ParameterName = "$v"; insert.Parameters.Add(p_v);
            var p_a = insert.CreateParameter(); p_a.ParameterName = "$a"; insert.Parameters.Add(p_a);
            var p_b = insert.CreateParameter(); p_b.ParameterName = "$b"; insert.Parameters.Add(p_b);
            var p_ty = insert.CreateParameter(); p_ty.ParameterName = "$ty"; insert.Parameters.Add(p_ty);

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var batch))
                {
                    txCmd.ExecuteNonQuery();

                    //foreach (ref readonly var t in batch.AsSpan())
                    foreach (var t in batch)
                    {
                        p_sid.Value = t.SymbolId;
                        p_t.Value = t.Time;
                        p_p.Value = t.Price;
                        p_v.Value = t.Volume;
                        p_a.Value = t.Ask;
                        p_b.Value = t.Bid;
                        p_ty.Value = t.Type;

                        insert.ExecuteNonQuery();
                    }

                    commitCmd.ExecuteNonQuery();
                }
            }
        });

        public void Dispose() => _conn.Dispose();
    }
}
