using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Converters
{
    public class SizeToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Size))
            {
                throw new ArgumentException(nameof(value));
            }

            var size = (Size)value;

            return $"{size.Width.ToString(CultureInfo.InvariantCulture)}x{size.Height.ToString(CultureInfo.InvariantCulture)}";
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var str = value as string;
			try
			{
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new ArgumentNullException(nameof(value));
				}
				var splitted = str.Split('x');
				if (splitted.Length < 2)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				return new Size(double.Parse(splitted[0]), double.Parse(splitted[1]));
			}
			catch (Exception)
			{
				return new ValidationResult(false, "Please provide valid resolution in format: <width>x<height>");
			}
		}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
