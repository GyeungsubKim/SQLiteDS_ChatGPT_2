using System.IO.Pipes;

namespace SQLiteDS_ChatGPT_2.Pipe
{
    public sealed class PipeSession
    {
        public NamedPipeServerStream Stream { get; }

        public PipeSession(NamedPipeServerStream stream)
        {
            Stream = stream;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> data)
        {
            if (!Stream.IsConnected) return;

            try
            {
                await Stream.WriteAsync(data);
            }
            catch
            {
                try { Stream.Dispose(); } catch { }
            }
        }
    }
}
