using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Common.Converters
{
    public class DoubleShiftConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int &&
                double.TryParse(parameter?.ToString(), out var doubleParam))
            {
                return (int)value + doubleParam;
            }
            return (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double &&
                double.TryParse(parameter?.ToString(), out var doubleParam))
            {
                var rounded = (int)((double)value - doubleParam);
                return rounded;
            }
            return (int)value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
