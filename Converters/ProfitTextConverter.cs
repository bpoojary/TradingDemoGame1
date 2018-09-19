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
    public class ProfitTextConverter : System.Windows.Data.IValueConverter
    {
      
        public ProfitTextConverter()
        {
           
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Your Profit:";
            double change = 0;
            Double.TryParse(System.Convert.ToString(value), out change);
            return change < 0 ? "Your Loss:" : "Your Profit:";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
