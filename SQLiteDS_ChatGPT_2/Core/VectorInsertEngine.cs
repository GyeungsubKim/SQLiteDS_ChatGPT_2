using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_2.VectorInsertEngine;

namespace SQLiteDS_ChatGPT_2.Core
{
    public sealed class VectorInsertEngine
    {
        private readonly SqliteConnection _con;
        private readonly VectorInsertCache _cache;

        public VectorInsertEngine(SqliteConnection con)
        {
            _con = con;
            _cache = new VectorInsertCache(con);
        }
        public void InsertBatch<T>(IReadOnlyList<T> models, string table)
        {
            if (models == null || models.Count == 0) return;

            var (cmd, binder) = _cache.Get(typeof(T), table);

            using var tx = _con.BeginTransaction();
            cmd.Transaction = tx;

            foreach (var m in models)
            {
                binder(cmd, m!);                
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
    }
}
