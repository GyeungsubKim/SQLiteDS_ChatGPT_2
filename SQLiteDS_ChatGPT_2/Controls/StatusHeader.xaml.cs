using System.Windows;
using System.Windows.Controls;

namespace SQLiteDS_ChatGPT_2.Controls
{
    /// <summary>
    /// StatusHeader.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StatusHeader : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(StatusHeader));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public StatusHeader()
        {
            InitializeComponent();
        }
    }
}
