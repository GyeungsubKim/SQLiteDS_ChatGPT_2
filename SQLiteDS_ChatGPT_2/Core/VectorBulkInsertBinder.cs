using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace SQLiteDS_ChatGPT_2.Core
{
    public class VectorBulkInsertBinder
    {
        private static readonly ConcurrentDictionary<WorkType, string> _sqlCache = new();
        private static readonly ConcurrentDictionary<WorkType,
            Action<SqliteCommand, Basic>> _binderCache = new();
        private static readonly ConcurrentDictionary<WorkType, int> _paramCountCache = new();
        public static void Insert(SqliteConnection conn,
                                  SqliteTransaction tx,
                                  WorkType type,
                                  List<Basic> models)
        {
            if (models == null || models.Count == 0) return;

            var sql = _sqlCache.GetOrAdd(type,
                t => ModelSqlFactory.GetInsertSql(t));

            var binder = _binderCache.GetOrAdd(type,
                t => ModelBinderCompile.Get(t));

            var paramCount = _paramCountCache.GetOrAdd(type,
                t => ModelBinderCompile.GetParamCount(t));

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            foreach (var model in models)
            {
                cmd.Parameters.Clear();
                binder(cmd, model);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }        
    }
}
