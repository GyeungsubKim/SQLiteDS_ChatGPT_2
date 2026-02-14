using SQLiteDS_ChatGPT_2.Pipe;
using SQLiteDS_ChatGPT_2.Workers;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_2.Core
{
    public class FeedBus
    {
        private readonly SQLiteWriter _writer;
        private readonly BinaryPipeServer _pipe;

        private long _globalSeq = 0;

        public event Action<UiPacket>? OnUi;

        public FeedBus(SQLiteWriter writer, BinaryPipeServer pipe)
        {
            _writer = writer;
            _pipe = pipe;
        }
        readonly WorkType[] Breaks = [WorkType.FutureCode, 
            WorkType.OptionCode, WorkType.HighLow, WorkType.Master];
        public void OnReceive(WorkType type, Basic model)
        {
            long seq = Interlocked.Increment(ref _globalSeq);
            model.Seq = seq;

            _writer.Enqueue(type,  model);
            _pipe.Send(type, model);

            if (Breaks.Contains(type)) return;

            long time = DateTime.Now.Ticks;

            var ui = UiPacketFactory.Create(type, model, time, seq);
            if (ui != null)
                OnUi?.Invoke(ui);
        }
    }
}
