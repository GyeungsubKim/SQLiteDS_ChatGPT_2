using Microsoft.Data.Sqlite;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class SqlWriteJob(string sql, SqliteParameter[] parameters)
    {
        public string Sql { get; } = sql;
        public SqliteParameter[] Parameters { get; } = parameters;
    }
}
