using System.IO;
using System.Windows;
using System.Windows.Shapes;

namespace SQLiteDS_ChatGPT_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
            //private ViewModel? _vm;
            //private EngineBootstrap? _engine;
            //private FeedBus? _bus;
            //private Works? _work;

            public static string CurPath = Directory.GetCurrentDirectory();
            public static string CurDate = DateTime.Now.ToString("yyyyMMdd");
            public MainWindow()
            {
                InitializeComponent();

                // 생성자에서 이벤트 등록을 해야된다.
                if (GetPoint() == null)
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                else
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Left = _point!.Value.X;
                    this.Top = _point.Value.Y;
                }

                LocationChanged += MainWindow_LocationChanged;
                Loaded += MainWindow_Loaded;
            }
            string dataPath = System.IO.Path.Combine(CurPath, "Datas");
            private void MainWindow_Loaded(object sender, RoutedEventArgs e)
            {
                //ViewModel.LogText.CollectionChanged += LogText_CollectionChanged;
                //if (!Directory.Exists(dataPath))
                //    Directory.CreateDirectory(dataPath);
                //ViewModel.AddLog($"데이터 베이스 저장 장소 : {dataPath}");

                //var dbPah = Path.Combine(dataPath, $"sql_{CurDate}.db3");
                //_engine = new EngineBootstrap(dbPah);
                //_engine.Start();

                //_bus = new FeedBus(_engine.Writer, _engine.Pipe);
                //_work = new Works(_bus);
                //_work.OnReceive += _bus.OnReceive;

                //_vm = new ViewModel(_bus);
                //DataContext = _vm;
            }
            private void LogText_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    return;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //if (LogList.Items.Count < 1) return;
                        //var lastIndex = LogList.Items.Count - 1;
                        //LogList.SelectedIndex = lastIndex;
                        //LogList.ScrollIntoView(LogList.Items[lastIndex]);
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }));
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
                if (_point == null)
                    _point = new Point(this.Left, this.Top);
                File.WriteAllText(PositionFile, $"{_point.Value.X}, {_point.Value.Y}");
            }
            Point? _point;
            private static string PositionFile { get => System.IO.Path.Combine(CurPath, "position.ini"); }
            private bool? GetPoint()
            {
                if (!File.Exists(PositionFile))
                    return null;
                string text = File.ReadAllText(PositionFile);
                string[] parts = text.Split(',');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out double x) &&
                    double.TryParse(parts[1], out double y))
                {
                    _point = new Point(x, y);
                    return true;
                }
                return null;
            }
            private void Button_Start_Click(object sender, RoutedEventArgs e)
            {
                //Task.Run(() => _work?.Run());
            }
            private void Button_Exit_Click(object sender, RoutedEventArgs e)
            {
                //_work = null;
                //_vm = null;

                base.Close();
            }
        }
}