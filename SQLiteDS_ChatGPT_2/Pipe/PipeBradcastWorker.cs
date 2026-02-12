using SQLiteDS_ChatGPT_2.Core;
using System.Runtime.InteropServices;

namespace SQLiteDS_ChatGPT_2.Pipe
{
    public sealed class PipeBroadcastWorker
    {
        private readonly BinaryPipeServer _server;

        public PipeBroadcastWorker(BinaryPipeServer server)
        {
            _server = server;
        }

        public Task RunAsync() => Task.Run(async () =>
        {
            var reader = ChannelHub.PipeChannel.Reader;
            byte[] header = new byte[4];

            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var batch))
                {
                    var payload = Serialize(batch);

                    PipePacket.WriteLength(header, payload.Length);

                    foreach (var kv in _server.Clients)
                    {
                        var session = kv.Value;

                        await session.SendAsync(header);
                        await session.SendAsync(payload);
                    }
                }
            }
        });

        private static byte[] Serialize(Tick[] ticks)
        {
            int size = ticks.Length * Marshal.SizeOf<Tick>();
            byte[] buffer = new byte[size];

            var span = MemoryMarshal.Cast<byte, Tick>(buffer);
            ticks.CopyTo(span);

            return buffer;
        }
    }
}
