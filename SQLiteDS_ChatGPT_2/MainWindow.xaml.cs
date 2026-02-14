using System.IO;
using System.Windows;
using SQLiteDS_ChatGPT_2.Daishins;
using SQLiteDS_ChatGPT_2.Views;
using SQLiteDS_ChatGPT_2.Workers;

namespace SQLiteDS_ChatGPT_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel? _vm;
        private EngineBootstrap? _engine;
        private Works? _work;
        private CancellationTokenSource? _cts;

        public string CurPath = Directory.GetCurrentDirectory();
        public string CurDate = DateTime.Now.ToString("yyyyMMdd");
        Point? _point;
        private string PositionFile { get => System.IO.Path.Combine(CurPath, "position.ini"); }
        public MainWindow()
        {
            InitializeComponent();

            // 생성자에서 이벤트 등록을 해야된다.
            if (GetPoint())
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = _point!.Value.X;
                this.Top = _point.Value.Y;
            }
            else
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            LocationChanged += MainWindow_LocationChanged;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string dataPath = System.IO.Path.Combine(CurPath, "Datas");
            Directory.CreateDirectory(dataPath);

            var dbPah = System.IO.Path.Combine(dataPath, $"sql_{CurDate}.db3");

            _engine = new EngineBootstrap(dbPah);
            _engine.Start();

            _vm = new ViewModel(_engine.Bus, Dispatcher);
            DataContext = _vm;

            _vm.LogText.CollectionChanged += LogText_CollectionChanged;

            _work = new Works(_vm);
            _work.OnReceive += _engine.Bus.OnReceive;

            _vm.AddLog($"데이터 베이스 저장 장소 : {dataPath}");

        }
        private void LogText_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;

            this.Dispatcher.BeginInvoke(() =>
            {
                if (LogList.Items.Count < 1) return;
                var lastIndex = LogList.Items.Count - 1;
                LogList.SelectedIndex = lastIndex;
                LogList.ScrollIntoView(LogList.Items[lastIndex]);
            });
        }
        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            if (_point == null)
                _point ??= new Point(this.Left, this.Top);
            else
                _point = new Point(this.Left, this.Top);
            WritePoint();
        }
        private void WritePoint()
        {            
            _point ??= new Point(this.Left, this.Top);
            File.WriteAllText(PositionFile, 
                $"{_point.Value.X}, {_point.Value.Y}");
        }
        private bool GetPoint()
        {
            if (!File.Exists(PositionFile))
                return false;
            string text = File.ReadAllText(PositionFile);
            string[] parts = text.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out double x) &&
                double.TryParse(parts[1], out double y))
            {
                _point = new Point(x, y);
                return true;
            }
            return false;
        }
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            if (_work == null) return;
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await _work.Run();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    _vm?.AddLog($"Button Start Click Error: {ex.Message}"));
                }
            });
        }
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            base.Close();
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _cts?.Cancel();
            _engine?.Stop();
            _engine = null;
        }
    }
}