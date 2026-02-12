using SQLiteDS_ChatGPT_2.Core;
using System.Diagnostics;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class BatchWorker
    {
        public Task RunAsync() => Task.Run(async () =>
        {
            var reader = ChannelHub_1.ProcessChannel.Reader;
            var dbWriter = ChannelHub_1.WriteChannel.Writer;
            var pipeWriter = ChannelHub_1.PipeChannel.Writer;

            var buffer = new List<Tick>(EngineConfig.BatchSize);
            var sw = Stopwatch.StartNew();

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var tick))
                {
                    buffer.Add(tick);

                    bool flush =
                        buffer.Count >= EngineConfig.BatchSize ||
                        sw.ElapsedMilliseconds >= EngineConfig.BatchFlushMs;

                    if (flush)
                    {
                        var batch = buffer.ToArray();
                        buffer.Clear();
                        sw.Restart();

                        await dbWriter.WriteAsync(batch);
                        await pipeWriter.WriteAsync(batch);
                    }
                }
            }
        });
    }
}
