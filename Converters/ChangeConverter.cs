using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IndiaHacksGame.Converters
{
    public class ChangeConverter : System.Windows.Data.IValueConverter
    {
        Brush _redBrush;
        Brush _greenBrush;
        public ChangeConverter()
        {
            _redBrush = Application.Current.Resources["redBrush"] as LinearGradientBrush;
            _greenBrush = Application.Current.Resources["greenBrush"] as LinearGradientBrush;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return _greenBrush !=null ? _greenBrush : new SolidColorBrush(Colors.Green);
            double change = -1;
            Double.TryParse(System.Convert.ToString(value), out change);
            return change <0 ? _redBrush != null ? _redBrush : new SolidColorBrush(Colors.Red) : _greenBrush != null ? _greenBrush : new SolidColorBrush(Colors.Green); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
