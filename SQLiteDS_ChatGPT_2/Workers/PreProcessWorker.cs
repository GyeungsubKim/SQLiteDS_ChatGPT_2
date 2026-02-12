using SQLiteDS_ChatGPT_2.Core;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class PreProcessWorker
    {
        public Task RunAsync() => Task.Run(async () =>
        {
            var reader = ChannelHub.FeedChannel.Reader;
            var writer = ChannelHub.ProcessChannel.Writer;

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var tick))
                {
                    // 필요 시 보정 로직
                    // tick.Time = Normalize(...);

                    await writer.WriteAsync(tick);
                }
            }
        });
    }
}
