using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SQLiteDS_ChatGPT_2.Controls
{
    /// <summary>
    /// StatusValue.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StatusValue : UserControl
    {
        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value), 
                typeof(string), 
                typeof(StatusValue),
                new PropertyMetadata(""));
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static readonly DependencyProperty ValueBackgroundProperty =
            DependencyProperty.Register(
                nameof(ValueBackground),
                typeof(Brush),
                typeof(StatusValue),
                new PropertyMetadata(Brushes.Transparent));
        public Brush ValueBackground
        {
            get { return (Brush)GetValue(ValueBackgroundProperty); }
            set { SetValue(ValueBackgroundProperty, value); }
        }
        public StatusValue()
        {
            InitializeComponent();
        }
    }
}
