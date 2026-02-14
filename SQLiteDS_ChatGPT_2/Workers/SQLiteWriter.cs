using SQLiteDS_ChatGPT_2.Core;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class SQLiteWriter 
    {
        private readonly Channel<WriteItem> _channel;
        private readonly DbWorker _worker;

        public SQLiteWriter(string dbPath)
        {
            _channel = Channel.CreateBounded<WriteItem>(
                new BoundedChannelOptions(200_000)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                });
            _worker = new DbWorker(dbPath, _channel.Reader);
        }
        public void Start()
            => _ = Task.Run(() => _worker.Run());
        public void Enqueue(WorkType type, Basic model)
            => _channel.Writer.TryWrite(new WriteItem(type, model));
    }
    public readonly record struct WriteItem(WorkType Type, Basic Model);
}
