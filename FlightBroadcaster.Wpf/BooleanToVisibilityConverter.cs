using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlightBroadcaster.Wpf
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Reversed { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Reversed ^ (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
