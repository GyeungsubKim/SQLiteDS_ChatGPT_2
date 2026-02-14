using Microsoft.Data.Sqlite;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public static class SqliteUltraPragma
    {
        public static void Apply(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();

            cmd.CommandText =
            @"
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=OFF;
            PRAGMA temp_store=MEMORY;
            PRAGMA cache_size=-200000;
            PRAGMA mmap_size=30000000000;
            ";

            cmd.ExecuteNonQuery();
        }
    }
}
