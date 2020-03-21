using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Common.Converters
{
    public class MultiDoubleToStringConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var width = (double)values[0];
            var height = (double)values[1];
            return $"{((int)width).ToString()}x{((int)height).ToString()}";
        }

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			var values = stringValue.Split('x');
			return new object[] { double.Parse(values[0]), double.Parse(values[1]) };
		}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
