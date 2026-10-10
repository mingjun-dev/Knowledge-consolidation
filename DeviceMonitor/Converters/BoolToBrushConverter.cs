using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceMonitor
{
    public class BoolToBrushConverter : IValueConverter
    {
        private static readonly Color green = Colors.Green;
        private static readonly Color red = Colors.Red;
        private static readonly Color gray = Colors.DimGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool IsConnected) 
            {
                return IsConnected ? new SolidColorBrush(green): new  SolidColorBrush(red);
            }
            return new SolidColorBrush(gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
