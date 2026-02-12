using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_2.Core;
using System.Collections.Concurrent;

namespace SQLiteDS_ChatGPT_2.VectorInsertEngine
{
    public sealed class VectorInsertCache(SqliteConnection con)
    {
        private readonly SqliteConnection _con = con;
        private readonly ConcurrentDictionary<Type,
            (SqliteCommand Cmd, Action<SqliteCommand, object> Binder)>
            _cache = new();
        public (SqliteCommand, Action<SqliteCommand, object>) Get(Type t, string table)
        {
            return _cache.GetOrAdd(t, type =>
            {
                string sql = ModelSqlFactory.BuildInsertSql(type, table);

                var cmd = _con.CreateCommand();
                cmd.CommandText = sql;

                foreach (var p in type.GetProperties())
                {
                    if (p.Name == "Idx") continue;
                    cmd.Parameters.AddWithValue($"@{p.Name}", DBNull.Value);
                }

                var binder = ModelBinderCompiler.Compile(type);

                return (cmd, binder);
            });
        }
    }
}
