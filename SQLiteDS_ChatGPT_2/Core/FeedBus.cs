using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class FeedBus
    {
        private static ChannelWriter<Tick> Writer =>
                    ChannelHub_1.FeedChannel.Writer;

        public static bool TryPublish(in Tick tick)
        {
            return Writer.TryWrite(tick);
        }
    }
}
