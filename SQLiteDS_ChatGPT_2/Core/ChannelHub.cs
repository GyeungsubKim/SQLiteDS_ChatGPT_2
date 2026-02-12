using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class ChannelHub
    {
        public static readonly Channel<Tick> FeedChannel =
                Channel.CreateBounded<Tick>(
                    new BoundedChannelOptions(EngineConfig.FeedChannelSize)
                    {
                        SingleReader = false,
                        SingleWriter = true,
                        FullMode = BoundedChannelFullMode.DropOldest
                    });

        public static readonly Channel<Tick> ProcessChannel =
            Channel.CreateBounded<Tick>(
                new BoundedChannelOptions(EngineConfig.ProcessChannelSize)
                {
                    SingleReader = false,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                });

        public static readonly Channel<Tick[]> WriteChannel =
            Channel.CreateBounded<Tick[]>(
                new BoundedChannelOptions(EngineConfig.WriteChannelSize)
                {
                    SingleReader = true,
                    SingleWriter = false
                });

        public static readonly Channel<Tick[]> PipeChannel =
            Channel.CreateBounded<Tick[]>(
                new BoundedChannelOptions(EngineConfig.PipeChannelSize)
                {
                    SingleReader = false,
                    SingleWriter = false
                });
    }
}
