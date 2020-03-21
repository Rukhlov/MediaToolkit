using System;
using System.Globalization;
using System.Windows;

namespace ScreenStreamer.Wpf.Common.Converters
{
    public class InvertedBooleanToVisibilityConverter : BooleanToVisibilityConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)base.Convert(value, targetType, parameter, culture) == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)base.ConvertBack(value, targetType, parameter, culture);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
