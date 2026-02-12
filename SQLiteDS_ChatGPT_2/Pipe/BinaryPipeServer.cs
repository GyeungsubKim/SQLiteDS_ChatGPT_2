using System.Collections.Concurrent;
using System.IO.Pipes;

namespace SQLiteDS_ChatGPT_2.Pipe
{
    public sealed class BinaryPipeServer
    {
        private readonly ConcurrentDictionary<int, PipeSession> _clients = new();
        private int _idGen;

        public ConcurrentDictionary<int, PipeSession> Clients => _clients;

        public void Start()
        {
            _ = AcceptLoop();
        }

        private async Task AcceptLoop()
        {
            while (true)
            {
                var pipe = new NamedPipeServerStream(
                    PipeConfig.PipeName,
                    PipeDirection.Out,
                    PipeConfig.MaxClients,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    PipeConfig.PipeBufferSize,
                    PipeConfig.PipeBufferSize);

                await pipe.WaitForConnectionAsync();

                var id = Interlocked.Increment(ref _idGen);
                _clients[id] = new PipeSession(pipe);
            }
        }
    }
}
