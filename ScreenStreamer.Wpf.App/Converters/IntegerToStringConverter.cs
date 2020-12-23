using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScreenStreamer.Wpf.Converters
{

    public class IntegerToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var intValue = System.Convert.ToInt32(value);

                return intValue.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (string.IsNullOrEmpty(strValue))
            {
                return 0;
            }
            try
            {
                return int.Parse(strValue);
            }
            catch (Exception exception)
            {
                return new ValidationResult(false, exception.Message);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class NetworkPortToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {string portStr = string.Empty;//LocalizationManager.GetString("CommonStringsAuto");

			if (value != null)
            {
                var intValue = System.Convert.ToInt32(value);
                if (intValue <= 0)
                {
                    return null;
                }
                //else if(intValue > ushort.MaxValue)
                //{
                //    return null;
                //}

                return intValue.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;

			//ushort port = 0;
			//if(ushort.TryParse(strValue, out port))
			//{

			//}

			//return port;

			if (string.IsNullOrEmpty(strValue))
			{
				return 0;
			}
			try
			{
				return ushort.Parse(strValue);
			}
			catch (Exception exception)
			{
				return new ValidationResult(false, exception.Message);
			}
		}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
