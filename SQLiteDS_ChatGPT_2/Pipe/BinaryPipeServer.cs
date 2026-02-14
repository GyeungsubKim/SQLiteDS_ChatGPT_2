using SQLiteDS_ChatGPT_2.Core;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Pipe
{
    public sealed class BinaryPipeServer
    {
        private readonly Channel<(WorkType, Basic)> _channel;

        private CancellationTokenSource? _cts;
        private Task? _task;

        public BinaryPipeServer()
        {
            _channel = Channel.CreateUnbounded<(WorkType, Basic)> (
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Run(() => Run(_cts.Token));
        }
        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _task?.Wait(2000);
            }
            catch { }
        }
        public void Send(WorkType type, Basic model)
            => _channel.Writer.TryWrite((type, model));
        private async Task Run(CancellationToken token)
        {
            using var pipe = new NamedPipeServerStream(
                PipeConfig.PipeName,
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            await pipe.WaitForConnectionAsync(token);

            using var bw = new BinaryWriter(pipe);

            while (!token.IsCancellationRequested &&
                await _channel.Reader.WaitToReadAsync(token))
            { 
                while (_channel.Reader.TryRead(out var item))
                {
                    bw.Write((int)item.Item1);
                    item.Item2.WriteTo(bw);
                    bw.Flush();
                }
            }
        }
    }
}
