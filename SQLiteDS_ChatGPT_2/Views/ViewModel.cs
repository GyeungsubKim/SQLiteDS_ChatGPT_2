using SQLiteDS_ChatGPT_2.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;

namespace SQLiteDS_ChatGPT_2.Views
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly FeedBus _bus;
        private readonly Dispatcher _ui;

        public ObservableCollection<string> LogText { get; } = [];

        public ViewModel(FeedBus bus, Dispatcher ui)
        {
            _bus = bus;
            _ui = ui;

            _bus.OnUi += OnUiEvent;
        }
        private void OnUiEvent(UiPacket pkt)
        {
            _ui.BeginInvoke(() =>
            {
                switch (pkt.Type)
                {
                    case WorkType.OptionCur:
                        
                    case WorkType.Ksp:
                        switch (pkt.Index![0])
                        {
                            case 'U':
                                Increase(nameof(Kospi)); break;
                            case '3':
                                Increase(nameof(Kos200)); break;
                            case '4':
                                Increase(nameof(ExpectFut)); break;
                        }
                        break;
                }
            });
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void AddLog(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LogText.Add(msg);
                if (LogText.Count > 5000) 
                    LogText.RemoveAt(0);
            });
        }
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                int total = ExpectFut + CurrentFut + BidNask +
                    CurrentOpt + Trading + Kospi + Kos200;
                Set(ref _totalCount, total);
            } 
        }
        private int _expectFut;
        public int ExpectFut
        {
            get => _expectFut;
            set => Set(ref _expectFut, value, nameof(ExpectFut));
        }
        private Brush _expectFutBg = Brushes.Transparent;
        public Brush ExpectFutBg
        {
            get => _expectFutBg;
            set => Set(ref _expectFutBg, Brushes.Cyan, nameof(ExpectFutBg));
        } 
        private int _currentFut;
        public int CurrentFut
        {
            get => _currentFut;
            set => Set(ref _currentFut, value, nameof(CurrentFut)); 
        }
        private Brush _currentFutBg = Brushes.Transparent;
        public Brush CurrentFutBg
        {
            get => _currentFutBg;
            set => Set(ref _currentFutBg, Brushes.Cyan, nameof(CurrentFutBg));
        } 
        private int _bidNask;
        public int BidNask
        {
            get => _bidNask;
            set => Set(ref _bidNask, value, nameof(BidNask));
        }
        private Brush _bidNaskBg = Brushes.Transparent;
        public Brush BidNaskBg
        {
            get => _bidNaskBg;
            set => Set(ref _bidNaskBg, Brushes.Cyan, nameof(BidNaskBg));
        }
        private int _currentOpt;
        public int CurrentOpt
        {
            get => _currentOpt;
            set => Set(ref _currentOpt, value, nameof(CurrentOpt));
        }
        private Brush _currentOptBg = Brushes.Transparent;
        public Brush CurrentOptBg
        {
            get => _currentOptBg;
            set => Set(ref _currentOptBg, Brushes.Cyan, nameof(CurrentOptBg));
        }
        private int _trading;
        public int Trading
        {
            get => _trading;
            set => Set(ref _trading, value, nameof(Trading));
        }
        private Brush _tradingBg = Brushes.Transparent;
        public Brush TradingBg
        {
            get => _tradingBg;
            set => Set(ref _tradingBg, Brushes.Cyan, nameof(TradingBg));
        }
        private int _kospi;
        public int Kospi
        {
            get => _kospi;
            set => Set(ref _kospi, value, nameof(Kospi));
        }
        private Brush _kospiBg = Brushes.Transparent;
        public Brush KospiBg
        {
            get => _kospiBg;
            set => Set(ref _kospiBg, Brushes.Cyan, nameof(KospiBg));
        }
        private int _kos200;
        public int Kos200
        {
            get => _kos200;
            set => Set(ref _kos200, value, nameof(Kos200));
        }
        private Brush _kos200Bg = Brushes.Transparent;
        public Brush Kos200Bg
        {
            get => _kos200Bg;
            set => Set(ref _kos200Bg, Brushes.Cyan, nameof(Kos200Bg));
        }
        public void Increase(string fieldName)
        {
            ResetAllBackgounds();
            var propInfo = this.GetType().GetProperty(fieldName);

            if (propInfo != null)
            {
                int current = (int)propInfo.GetValue(this)!;
                propInfo.SetValue(this, current);

                var bgInfo = this.GetType().GetProperty(fieldName + "Bg");
                bgInfo?.SetValue(this, Brushes.Cyan);

                TotalCount = 1;
            }
        }
        private void ResetAllBackgounds()
        {
            _expectFutBg = Brushes.Transparent;
            _currentFutBg = Brushes.Transparent;
        }
    }
}
