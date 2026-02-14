using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_2.Core;
using SQLiteDS_ChatGPT_2.Pipe;
using System.IO;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public class EngineBootstrap
    {
        public SQLiteWriter Writer { get; }
        public BinaryPipeServer Pipe { get; }
        public FeedBus Bus { get; }

        private readonly string _dbPath;

        public EngineBootstrap(string dbPath)
        {
            _dbPath = dbPath; 

            Writer = new SQLiteWriter(_dbPath);
            Pipe = new BinaryPipeServer();
            Bus = new FeedBus(Writer, Pipe);
        }
        public void Start()
        {
            var isNew = !File.Exists(_dbPath);
            var conn = new SqliteConnection(
                $"Data Source={_dbPath};Pooling=True;");
            conn.Open();

            if (isNew)
            {
                var sql = SqlCreateGenerator.CreateTablesSql();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                using var pragma = conn.CreateCommand();
                pragma.CommandText = @"
                    PRAGMA journal_mode=WAL;
                    PRAGMA synchronous=NORMAL;
                    PRAGMA temp_store=MEMORY;
                    PRAGMA cache_size=-200000;";
                pragma.ExecuteNonQuery();
            }

            Writer.Start();
            Pipe.Start();
        }
        public void Stop()
            => Pipe.Stop();
    }
}
