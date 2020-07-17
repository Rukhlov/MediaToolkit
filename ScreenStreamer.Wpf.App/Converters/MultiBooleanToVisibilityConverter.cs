using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Converters
{
    public class MultiBooleanToVisibilityConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var boolResult = (parameter as string)?.Contains("Any") == true ?
                                values.Any(v => (bool)v == true) :
                                values.All(v => (bool)v == true);
            return boolResult ? Visibility.Visible : ((parameter as string)?.Contains("Collapsed") == true ? Visibility.Collapsed : Visibility.Hidden);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
