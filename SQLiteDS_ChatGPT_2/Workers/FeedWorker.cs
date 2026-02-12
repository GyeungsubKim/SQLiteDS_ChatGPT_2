using SQLiteDS_ChatGPT_2.Core;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public sealed class FeedWorker
    {
        private Thread? _thread;
        private volatile bool _running;

        public void Start()
        {
            _running = true;
            _thread = new Thread(Run) { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA); // Cybos COM
            _thread.Start();
        }

        public void Stop() => _running = false;

        private void Run()
        {
            // TODO: CybosPlus 로그인/구독 초기화
            while (_running)
            {
                // TODO: Cybos 이벤트에서 데이터 수신
                // 아래는 예시 Tick 생성
                Tick tick = new Tick
                {
                    SymbolId = 1,
                    Time = EngineClock.NowMicro(),
                    Price = 123450, // price * 100
                    Volume = 1,
                    Ask = 123460,
                    Bid = 123440,
                    Type = 0
                };

                FeedBus.TryPublish(in tick);
                Thread.SpinWait(50); // 실제는 이벤트 기반
            }
        }
    }
}
