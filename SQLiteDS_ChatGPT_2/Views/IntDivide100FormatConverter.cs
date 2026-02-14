using System.Globalization;
using System.Windows.Data;

namespace SQLiteDS_ChatGPT_2.Views
{
    /// <summary>
    /// XAML Binding  적용
    /// <local:StatusValue Grid.Row="0" Grid.Column="1"
    ///     Value="{Binding TotalCount, Converter={StaticResource Div100Fmt}}"/>
    /// <local:StatusValue Grid.Row="1" Grid.Column= "1"
    ///     Value= "{Binding ExpectFut, Converter={StaticResource Div100Fmt}}" />
    /// </summary>
    public class IntDivide100FormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0.00";
            if (!double.TryParse(value.ToString(), out double v)) return "0.00";

            double result = v / 100.0;

            return result.ToString("N2"); // 1,234,567.00
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
