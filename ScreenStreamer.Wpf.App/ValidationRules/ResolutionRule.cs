using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ScreenStreamer.Wpf.ValidationRules
{
	public class ResolutionRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var stringValue = value as string;
			var result = new ValidationResult(true, "");
			var failResult = new ValidationResult(false, "Please provide valid resolution in format: <width>x<height>");

			if (string.IsNullOrWhiteSpace(stringValue))
				return failResult;

			var splitted = stringValue.Split('x');
			if (splitted.Length != 2)
				return failResult;

			if (!double.TryParse(splitted[0], out var tmp) || tmp > 1000000)
				return failResult;

			if (!double.TryParse(splitted[1], out var tmp2) || tmp2 > 1000000)
				return failResult;

			return result;
		}
	}
}
