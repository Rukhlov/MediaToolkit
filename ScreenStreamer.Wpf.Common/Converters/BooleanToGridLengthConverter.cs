using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Common.Converters
{
    public class BooleanToGridLengthConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = 0d;
            var boolValue = (bool)value;
            
            if (boolValue)
            {
                result = double.Parse(parameter.ToString());
            }
            return new GridLength(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
